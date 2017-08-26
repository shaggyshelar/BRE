using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// Provides rule evaluation functionality
    /// </summary>
    public class Evaluator : EvaluatorBase
    {
        private Dictionary<string, Predicate> predicates = new Dictionary<string, Predicate>();

        /// <summary>
        /// Initializes a new instance of the Evaluator to a specified set of rules. A delegate of type GetRuleDelegate is used
        /// to request any rule that may be referenced inside another rule.
        /// </summary>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule/4 schema.</param>
        /// <param name="getRule">The delegate to a method used to retrieve external rules (rules referenced by other rules).
        /// If this parameter is null, the Evaluator will attempt to find any referenced rules inside the ruleset. If none are found
        /// an exception will be thrown.</param>
        public Evaluator(Type sourceType, string rulesetXml, GetRuleDelegate getRule = null, int maxIterations = -1)
            : base(sourceType, rulesetXml, maxIterations, getRule)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Evaluator to a specified set of rules. A delegate of type GetRuleDelegate is used
        /// to request any rule that may be referenced inside another rule.
        /// </summary>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule/4 schema.</param>
        /// <param name="parameters">A set of parameters that determine various aspects of rule compilation and evaluation. 
        /// See <see cref="T:CodeEffects.Rule.Core.EvaluationParameters" /> for more information</param>
        public Evaluator(Type sourceType, string rulesetXml, EvaluationParameters parameters)
            : base(sourceType, rulesetXml, parameters)
        {
        }

        /// <summary>
        /// Builds an expression tree that represents the given rule. The expression tree is then cached
        /// and compiled into byte code.
        /// </summary>
        /// <param name="rule">A rule to be compiled.</param>
        protected override void CompileRule(XElement rule)
        {
            ExpressionBuilder expressionBuilder = new ExpressionBuilder(this.sourceType, new GetRuleInternalDelegate(base.GetRule));
            expressionBuilder.MaxIterations = this.parameters.MaxIterations;
            expressionBuilder.PerformNullChecks = this.parameters.PerformNullChecks;
            expressionBuilder.EvaluationScope = this.parameters.Scope;
            expressionBuilder.ShortCircuit = this.parameters.ShortCircuit;
            expressionBuilder.Precision = this.parameters.Precision;
            LambdaExpression predicateExpression = expressionBuilder.GetPredicateExpression(rule);
            base.LogExpression(predicateExpression);
            Delegate @delegate = expressionBuilder.CompileRule(predicateExpression);
            string key = (string)rule.Attribute("id");
            this.predicates[key] = new Predicate
            {
                Delegate = @delegate,
                Expression = predicateExpression
            };
        }

        /// <summary>
        /// Executes (evaluates) the rule against the source object. If no id is provided,
        /// the first rule of the ruleset will be used.
        /// </summary>
        /// <param name="source">The object which needs to be evaluated. It acts as a source of data for the rule.</param>
        /// <param name="ruleId">An optional id of a rule. If there are more than one rule in the ruleset, you may evaluate a specific
        /// rule by providing its id. <remarks>Not to be confused with external rules referenced inside other rules. This rule must be present as a top
        /// level rule inside the ruleset or an exception will be thrown.</remarks></param>
        /// <returns>Returns boolean result of evaluation: true if successful, false otherwise.</returns>
        public bool Evaluate(object source, string ruleId = null)
        {
            base.DelayIfDemo();
            if (ruleId == null)
            {
                return (bool)this.predicates.First<KeyValuePair<string, Predicate>>().Value.Delegate.DynamicInvoke(new object[]
				{
					source
				});
            }
            return (bool)this.predicates[ruleId].Delegate.DynamicInvoke(new object[]
			{
				source
			});
        }

        /// <summary>
        /// Executes (evaluates) the specific rule against the source object. 
        /// </summary>
        /// <param name="source">The object which needs to be evaluated. It acts as a source of data for the rule.</param>
        /// <param name="ruleIndex">An index of a rule to be executed. If there are more than one rule in the ruleset, you may evaluate a specific
        /// rule by providing its index. <remarks>Not to be confused with external rules referenced inside other rules. This rule must be present as a top
        /// level rule inside the ruleset or exception wil be thrown.</remarks></param>
        /// <returns>Returns boolean result of evaluation: true if successful, false otherwise.</returns>
        public bool Evaluate(object source, int ruleIndex)
        {
            base.DelayIfDemo();
            return (bool)this.predicates.ElementAt(ruleIndex).Value.Delegate.DynamicInvoke(new object[]
			{
				source
			});
        }

        /// <summary>
        /// Executes (evaluates) all rules in a ruleset based on scope against the source object. 
        /// </summary>
        /// <param name="source">The object which needs to be evaluated. It acts as a source of data for the rule.</param>
        /// <param name="scope">Scope determines a logical operator (AND, OR) to be used when executing rules.</param>
        /// <param name="shortCircuit">Short-circuting tells evaluator to stop as soon as scope is satisfied.
        /// If false, the evaluator will continue executing all rules even if answer is already known. Default is true.</param>
        /// <returns>Returns boolean result of evaluation: true if successful, false otherwise.</returns>
        public bool Evaluate(object source, EvaluationScope scope, bool shortCircuit = true)
        {
            base.DelayIfDemo();
            bool flag = scope == EvaluationScope.All;
            for (int i = 0; i < this.predicates.Count; i++)
            {
                if (scope == EvaluationScope.All)
                {
                    flag &= (bool)this.predicates.ElementAt(i).Value.Delegate.DynamicInvoke(new object[]
					{
						source
					});
                    if (shortCircuit && !flag)
                    {
                        break;
                    }
                }
                else
                {
                    flag |= (bool)this.predicates.ElementAt(i).Value.Delegate.DynamicInvoke(new object[]
					{
						source
					});
                    if (shortCircuit && flag)
                    {
                        break;
                    }
                }
            }
            return flag;
        }

        internal Delegate GetPredicate(string ruleId)
        {
            if (ruleId == null)
            {
                return this.predicates.First<KeyValuePair<string, Predicate>>().Value.Delegate;
            }
            return this.predicates[ruleId].Delegate;
        }

        internal Delegate GetPredicate(int ruleIndex)
        {
            return this.predicates.ElementAt(ruleIndex).Value.Delegate;
        }

        internal LambdaExpression GetPredicateExpression(string ruleId)
        {
            if (ruleId == null)
            {
                return this.predicates.First<KeyValuePair<string, Predicate>>().Value.Expression;
            }
            return this.predicates[ruleId].Expression;
        }

        internal LambdaExpression GetPredicateExpression(int ruleIndex)
        {
            return this.predicates.ElementAt(ruleIndex).Value.Expression;
        }
    }

    /// <summary>
    /// <para>Compiles and executes business rules against source objects.</para>
    /// <para>One or more rules may be combined into one single XML string. If a rule contains references to other external rules,
    /// the Evaluator will try to locate them among rules in the given XML or call a delegate, if one is provided. Rules then are
    /// compiled and stored in memory for future execution by Evaluate methods.</para>
    /// </summary>
    /// <remarks>See http://codeeffects.com/schemas/rule/3 schema for information on full ruleset format.</remarks>
    /// <typeparam name="TSource">The type of a source object against which rules are evaluated.</typeparam>
    public class Evaluator<TSource> : EvaluatorBase
    {
        private Dictionary<string, Predicate<TSource>> predicates = new Dictionary<string, Predicate<TSource>>();

        /// <summary>
        /// Initializes a new instance of the Evaluator to a specified set of rules. A delegate of type GetRuleDelegate is used
        /// to request any rule that may be referenced inside another rule.
        /// </summary>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule/4 schema.</param>
        /// <param name="getRule">The delegate to a method used to retrieve external rules (rules referenced by other rules).
        /// If this parameter is null, the Evaluator will attempt to find any referenced rules inside the ruleset. If none are found
        /// an exception will be thrown.</param>
        public Evaluator(string rulesetXml, GetRuleDelegate getRule = null, int maxIterations = -1)
            : base(typeof(TSource), rulesetXml, maxIterations, getRule)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Evaluator to a specified set of rules.
        /// </summary>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule/4 schema.</param>
        /// <param name="parameters">A set of parameters that determine various aspects of rule compilation and evaluation. 
        /// See <see cref="T:CodeEffects.Rule.Core.EvaluationParameters" /> for more information</param>
        public Evaluator(string rulesetXml, EvaluationParameters parameters)
            : base(typeof(TSource), rulesetXml, parameters)
        {
        }

        /// <summary>
        /// Builds an expression tree that represents the given rule. The expression tree is then cached
        /// and compiled into byte code.
        /// </summary>
        /// <param name="rule">A rule to be compiled.</param>
        protected override void CompileRule(XElement rule)
        {
            ExpressionBuilder<TSource> expressionBuilder = new ExpressionBuilder<TSource>(new GetRuleInternalDelegate(base.GetRule));
            expressionBuilder.MaxIterations = this.parameters.MaxIterations;
            expressionBuilder.PerformNullChecks = this.parameters.PerformNullChecks;
            expressionBuilder.EvaluationScope = this.parameters.Scope;
            expressionBuilder.ShortCircuit = this.parameters.ShortCircuit;
            expressionBuilder.Precision = this.parameters.Precision;
            Expression<Func<TSource, bool>> predicateExpression = expressionBuilder.GetPredicateExpression(rule);
            base.LogExpression(predicateExpression);
            Func<TSource, bool> @delegate = expressionBuilder.CompileRule(predicateExpression);
            string key = (string)rule.Attribute("id");
            this.predicates[key] = new Predicate<TSource>
            {
                Delegate = @delegate,
                Expression = predicateExpression
            };
        }

        /// <summary>
        /// Executes (evaluates) the rule against the source object. If no id is provided,
        /// the first rule of the ruleset will be used.
        /// </summary>
        /// <param name="source">The object which needs to be evaluated. It acts as a source of data for the rule.</param>
        /// <param name="ruleId">An optional id of a rule. If there are more than one rule in the ruleset, you may evaluate a specific
        /// rule by providing its id. <remarks>Not to be confused with external rules referenced inside other rules. This rule must be present as a top
        /// level rule inside the ruleset or an exception will be thrown.</remarks></param>
        /// <returns>Returns boolean result of evaluation: true if successful, false otherwise.</returns>
        public bool Evaluate(TSource source, string ruleId = null)
        {
            if (ruleId == null)
            {
                return this.predicates.First<KeyValuePair<string, Predicate<TSource>>>().Value.Delegate(source);
            }
            return this.predicates[ruleId].Delegate(source);
        }

        /// <summary>
        /// Executes (evaluates) the specific rule against the source object. 
        /// </summary>
        /// <param name="source">The object which needs to be evaluated. It acts as a source of data for the rule.</param>
        /// <param name="ruleIndex">An index of a rule to be executed. If there are more than one rule in the ruleset, you may evaluate a specific
        /// rule by providing its index. <remarks>Not to be confused with external rules referenced inside other rules. This rule must be present as a top
        /// level rule inside the ruleset or exception will be thrown.</remarks></param>
        /// <returns>Returns boolean result of evaluation: true if successful, false otherwise.</returns>
        public bool Evaluate(TSource source, int ruleIndex)
        {
            base.DelayIfDemo();
            return this.predicates.ElementAt(ruleIndex).Value.Delegate(source);
        }

        /// <summary>
        /// Executes (evaluates) all rules in a ruleset based on scope against the source object. 
        /// </summary>
        /// <param name="source">The object which needs to be evaluated. It acts as a source of data for the rule.</param>
        /// <param name="scope">Scope determines a logical operator (AND, OR) to be used when executing rules.</param>
        /// <param name="shortCircuit">Short-circuting tells evaluator to stop as soon as scope is satisfied.
        /// If false, the evaluator will continue executing all rules even if answer is already known. Default is true.</param>
        /// <returns>Returns boolean result of evaluation: true if successful, false otherwise.</returns>
        public bool Evaluate(TSource source, EvaluationScope scope, bool shortCircuit = true)
        {
            base.DelayIfDemo();
            bool flag = scope == EvaluationScope.All;
            for (int i = 0; i < this.predicates.Count; i++)
            {
                if (scope == EvaluationScope.All)
                {
                    flag &= this.predicates.ElementAt(i).Value.Delegate(source);
                    if (shortCircuit && !flag)
                    {
                        break;
                    }
                }
                else
                {
                    flag |= this.predicates.ElementAt(i).Value.Delegate(source);
                    if (shortCircuit && flag)
                    {
                        break;
                    }
                }
            }
            return flag;
        }

        internal Func<TSource, bool> GetPredicate(string ruleId)
        {
            if (ruleId == null)
            {
                return this.predicates.First<KeyValuePair<string, Predicate<TSource>>>().Value.Delegate;
            }
            return this.predicates[ruleId].Delegate;
        }

        internal Func<TSource, bool> GetPredicate(int ruleIndex)
        {
            return this.predicates.ElementAt(ruleIndex).Value.Delegate;
        }

        internal Expression<Func<TSource, bool>> GetPredicateExpression(string ruleId)
        {
            if (ruleId == null)
            {
                return this.predicates.First<KeyValuePair<string, Predicate<TSource>>>().Value.Expression;
            }
            return this.predicates[ruleId].Expression;
        }

        internal Expression<Func<TSource, bool>> GetPredicateExpression(int ruleIndex)
        {
            return this.predicates.ElementAt(ruleIndex).Value.Expression;
        }
    }
}
