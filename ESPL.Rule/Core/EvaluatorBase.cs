using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// This class in not intended for public use
    /// </summary>
    public class EvaluatorBase
    {
        protected Type sourceType;

        protected XElement ruleSet;

        protected EvaluationParameters parameters = new EvaluationParameters();

        /// <summary>
        /// Gets or sets an output stream for logging expression trees. This should only be set when debugging.
        /// </summary>
        public static TextWriter Log
        {
            get;
            set;
        }

        internal bool SuspendDemoDelay
        {
            get;
            set;
        }

        protected void DelayIfDemo()
        {
            if (!this.SuspendDemoDelay)
            {
                Vector.DelayIfDemo();
            }
        }

        internal EvaluatorBase(Type sourceType, string rulesetXml, EvaluationParameters parameters)
        {
            this.sourceType = sourceType;
            this.ruleSet = XElement.Parse(rulesetXml);
            this.parameters = parameters;
            XNamespace defaultNamespace = this.ruleSet.GetDefaultNamespace();
            int num = 0;
            foreach (XElement current in this.ruleSet.Elements(defaultNamespace + "rule"))
            {
                string value = (string)current.Attribute("id");
                string text = (string)current.Attribute("type");
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = num.ToString();
                }
                if (!string.IsNullOrWhiteSpace(text) && text != sourceType.AssemblyQualifiedName)
                {
                    string[] array = text.Split(new char[]
					{
						','
					}, 2);
                    string name = array[0];
                    string a = (array.Length > 1) ? new AssemblyName(array[1]).FullName : "";
                    Type type = null;
                    if (a == sourceType.Assembly.FullName)
                    {
                        type = sourceType.Assembly.GetType(name, false, true);
                    }
                    if (type == null)
                    {
                        type = Type.GetType(text, false, true);
                    }
                    if (type == null || !type.IsAssignableFrom(sourceType))
                    {
                        continue;
                    }
                }
                this.CompileRule(current);
                num++;
            }
            if (num == 0)
            {
                throw new EvaluationException(EvaluationException.ErrorIds.NoRulesWithGivenType, new string[]
				{
					sourceType.AssemblyQualifiedName
				});
            }
        }

        internal EvaluatorBase(Type sourceType, string rulesetXml, int maxIterations, GetRuleDelegate getRule = null)
            : this(sourceType, rulesetXml, new EvaluationParameters
            {
                RuleGetter = getRule,
                MaxIterations = maxIterations
            })
        {
        }

        /// <summary>
        /// Outputs debug information about an expression into logging stream.
        /// </summary>
        /// <param name="expression">An expression to be converted into a text representation.</param>
        protected void LogExpression(Expression expression)
        {
            if (EvaluatorBase.Log != null)
            {
                EvaluatorBase.Log.WriteLine("EXPRESSION:");
                EvaluatorBase.Log.WriteLine(expression);
                EvaluatorBase.Log.WriteLine();
                EvaluatorBase.Log.WriteLine("DEBUG EXPRESSION:");
                EvaluatorBase.Log.WriteLine(typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(expression, null));
                EvaluatorBase.Log.WriteLine();
            }
        }

        /// <summary>
        /// Compiles a rule. The base method is a placeholder. Override to implement specific compilation algorithm.
        /// </summary>
        /// <param name="rule">A rule to be compiled.</param>
        protected virtual void CompileRule(XElement rule)
        {
        }

        /// <summary>
        /// Retrieves a rule based on id. If delegate is present, it calls the delegate.
        /// Otherwise searches within the ruleset that this rule belongs to.
        /// If none are found, returns null.
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        protected XElement GetRule(string ruleId)
        {
            if (this.parameters.RuleGetter != null)
            {
                return XElement.Parse(this.parameters.RuleGetter(ruleId));
            }
            XNamespace defaultNamespace = this.ruleSet.GetDefaultNamespace();
            return (from x in this.ruleSet.Elements(defaultNamespace + "rule")
                    where (string)x.Attribute("id") == ruleId
                    select x).FirstOrDefault<XElement>();
        }
    }
}
