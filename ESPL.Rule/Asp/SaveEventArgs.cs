using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Asp
{
    /// <summary>
    /// Arguments used by SaveRule event of the ...Asp.RuleEditor class
    /// </summary>
    public class SaveEventArgs : EventArgs
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
        /// Gets or sets the name of the rule. Users set this value by
        /// typing the rule's name into the Name text box of the Tool Bar
        /// </summary>
        public string RuleName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the rule. Users set this value by
        /// typing the rule's description into the Description text box of the Tool Bar
        /// </summary>
        public string RuleDescription
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the full XML document that contains the rule
        /// </summary>
        public string RuleXmlAsDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the XML node of the rule. Useful when saving rules as a single large ruleset document.
        /// </summary>
        public string RuleXmlAsNode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating if the rule is of evaluation type.
        /// </summary>
        public bool IsEvaluationTypeRule
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the list of IDs of all reusable rules referenced in this rule
        /// </summary>
        public List<string> ReferencedRules
        {
            get;
            set;
        }

        /// <summary>
        /// Empty public c-tor
        /// </summary>
        public SaveEventArgs()
        {
            this.IsEvaluationTypeRule = false;
            this.ReferencedRules = new List<string>();
        }
    }
}
