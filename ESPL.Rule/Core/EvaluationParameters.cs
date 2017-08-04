using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// A set of parameters that determine how engine handles the expression builds and rule evaluations.
    /// </summary>
    public class EvaluationParameters
    {
        /// <summary>
        /// Id of the rule to be evaluated or compiled.
        /// </summary>
        public string RuleId;

        /// <summary>
        /// A delegate for requesting missing rules
        /// (rules that are being referenced but not included in the ruleset).
        /// </summary>
        public GetRuleDelegate RuleGetter;

        /// <summary>
        /// A type of LINQ provider. Right now only three providers are supported: LINQ-to-Object, LINQ-to-SQL, and LINQ-to-Entities.
        /// Other providers may work as well as long as they implement the same sub-set of commands as LINQ-to-SQL.
        /// Use the Default value for all other providers.
        /// </summary>
        public LinqProviderType LINQProviderType;

        /// <summary>
        /// Gets or sets the value indicating whether the engine builds null-pointer
        /// safety checks into compiled code to avoid run-time exceptions.
        /// </summary>
        public bool PerformNullChecks = true;

        /// <summary>
        /// Gets or sets the value of the maximum number of iterations before evaluation
        /// fails in the Loop mode. The value of -1 sets no limits. The default value is -1.
        /// </summary>
        public int MaxIterations = -1;

        /// <summary>
        /// Gets or sets the value that specifies how to treat multiple rules in a ruleset
        /// when they are evaluated in one step using the Ruleset mode. The default
        /// value is EvaluationScope.All.
        /// </summary>
        public EvaluationScope Scope;

        /// <summary>
        /// Gets or sets the value indicating whether to stop further evaluation of a rule
        /// immediatelly after the result is determined. The default value is True.
        /// </summary>
        public bool ShortCircuit = true;

        /// <summary>
        /// Gets or sets the number of fractional digits to round decimal or double values to.
        /// ounding is done only in comparison operators using the Math.Round() method.
        /// The default value is -1, no rounding.
        /// </summary>
        public int Precision = -1;
    }
}
