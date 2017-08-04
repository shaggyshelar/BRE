using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Models
{
    /// <summary>
    /// Used by Asp.RuleEditor and Mvc.RuleEditor classes to hold lists of excluded client operators.
    /// </summary>
    public class Operator
    {
        /// <summary>
        /// Client type of the excluded operator
        /// </summary>
        public OperatorType Type
        {
            get;
            set;
        }

        /// <summary>
        /// List of excluded operators for defined client type
        /// </summary>
        public ExcludedOperator ExcludedOperator
        {
            get;
            set;
        }

        public Operator()
        {
            this.Type = OperatorType.None;
        }
    }
}
