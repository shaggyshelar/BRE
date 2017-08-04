using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// Used by RuleEditor class to specify the current Mode of the rule editor
    /// </summary>
    public enum RuleType
    {
        /// <summary>
        /// This member sets the mode of the rule editor that allows users
        /// to create rules of both types - evaluation and execution.
        /// In this mode ESPL control uses rule-related labels from Help XML.
        /// </summary>
        Execution,
        /// <summary>
        /// This member sets the mode of the rule editor that allows users
        /// to create only evaluation type rules.
        /// In this mode ESPL control uses rule-related labels from Help XML.
        /// </summary>
        Evaluation,
        /// <summary>
        /// This member sets the mode of the rule editor that allows users
        /// to create only evaluation type rules.
        /// In this mode ESPL control uses filter-related labels from Help XML.
        /// </summary>
        Filter,
        /// <summary>
        /// This member instructs the Evaluator to re-evaluate the same rule
        /// over and over until its conditions return False.
        /// </summary>
        Loop,
        /// <summary>
        /// This member instructs the Evaluator to evaluate multiple
        /// execution type rules in one step.
        /// </summary>
        Ruleset
    }
}
