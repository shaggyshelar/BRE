using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// Used by Code Effects control for rule evaluation-related exceptions
    /// </summary>
    public class EvaluationException : RuleException
    {
        public enum ErrorIds
        {
            ParameterCountMismatch = 102,
            NoRulesWithGivenType = 106,
            InvalidPostbackArgument = 110
        }

        internal EvaluationException(EvaluationException.ErrorIds error, params string[] parameters)
            : base("e" + (int)error, parameters)
        {
        }
    }
}
