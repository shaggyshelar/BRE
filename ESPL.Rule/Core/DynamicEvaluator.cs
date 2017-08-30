using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// <para>Provides functionality for rule evaluation based on its type.
    /// During compilation a rule engine needs to know the type of the source object
    /// so that it can be loaded and analyzed using reflection.</para>
    /// <para>This evaluator attempts to determine the type of the object dynamically and place it in the cache.</para>
    /// <para>This is used when rules for different source types are stored in the same ruleset.</para>
    /// </summary>
    public class DynamicEvaluator
    {
        protected string rulesetXml;

        protected GetRuleDelegate getRule;

        private Dictionary<Type, Evaluator> evaluators;

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

        /// <summary>
        /// Initializes the evaluator with the ruleset and optional delegate for retrieving external rules.
        /// </summary>
        /// <param name="rulesetXml">A ruleset containing multiple rules with different source types.</param>
        /// <param name="getRule">A method to retieve external rules.</param>
        public DynamicEvaluator(string rulesetXml, GetRuleDelegate getRule = null)
        {
            this.rulesetXml = rulesetXml;
            this.getRule = getRule;
            this.evaluators = new Dictionary<Type, Evaluator>();
        }

        /// <summary>
        /// Evaluates or executes the rule against the source object.
        /// </summary>
        /// <param name="source">An object against which the rule is evaluated.</param>
        /// <param name="ruleId">An id of a rule to be evaluated.</param>
        /// <returns>Boolean result of evaluation. True if the object passed evaluation, false otherwise.</returns>
        public bool Evaluate(object source, string ruleId = null)
        {
            Type type = source.GetType();
            Evaluator evaluator;
            if (this.evaluators.ContainsKey(type))
            {
                evaluator = this.evaluators[type];
            }
            else
            {
                evaluator = new Evaluator(source.GetType(), this.rulesetXml, this.getRule, -1);
                this.evaluators.Add(source.GetType(), evaluator);
            }
            return evaluator.Evaluate(source, ruleId);
        }

        /// <summary>
        /// Obsolete. Evaluates or executes the rule against the source object.
        /// </summary>
        /// <param name="source">An object against which the rule is evaluated.</param>
        /// <param name="ruleIndex">An index of a rule to be evaluated.</param>
        /// <returns>Boolean result of evaluation. True if the object passed evaluation, false otherwise.</returns>
        [Obsolete("Consider using a Evaluate overload with the ruleId parameter instead.")]
        public bool Evaluate(object source, int ruleIndex)
        {
            this.DelayIfDemo();
            Type type = source.GetType();
            Evaluator evaluator;
            if (this.evaluators.ContainsKey(type))
            {
                evaluator = this.evaluators[type];
            }
            else
            {
                evaluator = new Evaluator(source.GetType(), this.rulesetXml, this.getRule, -1);
                this.evaluators.Add(source.GetType(), evaluator);
            }
            return evaluator.Evaluate(source, ruleIndex);
        }

        public bool Evaluate(object source, EvaluationScope scope, bool shortCircuit = true)
        {
            this.DelayIfDemo();
            Type type = source.GetType();
            Evaluator evaluator;
            if (this.evaluators.ContainsKey(type))
            {
                evaluator = this.evaluators[type];
            }
            else
            {
                evaluator = new Evaluator(source.GetType(), this.rulesetXml, this.getRule, -1);
                this.evaluators.Add(source.GetType(), evaluator);
            }
            return evaluator.Evaluate(source, scope, shortCircuit);
        }
    }
}
