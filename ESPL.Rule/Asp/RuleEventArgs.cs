using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Asp
{
    /// <summary>
    /// Argu,emts used by DeleteRule and LoadRule events of the ...Asp.RuleEditor class
    /// </summary>
    public class RuleEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the ID of the rule. By default, Code Effects control uses Guid values for rule IDs.
        /// </summary>
        public string RuleID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating if the rule is of evaluation type.
        /// </summary>
        public bool? IsEvaluationTypeRule
        {
            get;
            set;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="id">Rule ID</param>
        /// <param name="isEval">Indicates if the rule is of evaluation type.</param>
        public RuleEventArgs(string id, bool? isEval)
        {
            this.RuleID = id;
            this.IsEvaluationTypeRule = isEval;
        }
    }
}
