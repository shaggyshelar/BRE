using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Models
{
    /// <summary>
    /// This enum can be used to exclude unwanted operators from being used by rule editor.
    /// This lowers the amount of data that Code Effects control sends to the client on page load.
    /// See the ExcludedOperators properties of ...Mvc.RuleEditor and ...Asp.RuleEditor classes for details.
    /// </summary>
    public enum ExcludedOperator
    {
        /// <summary>
        /// Exclude the Equal operator
        /// </summary>
        Equal = 1,
        /// <summary>
        /// Exclude the NotEqual operator
        /// </summary>
        NotEqual,
        /// <summary>
        /// Exclude the Less operator
        /// </summary>
        Less = 4,
        /// <summary>
        /// Exclude the LessOrEqual operator
        /// </summary>
        LessOrEqual = 8,
        /// <summary>
        /// Exclude the Greater operator
        /// </summary>
        Greater = 16,
        /// <summary>
        /// Exclude the GreaterOrEqual operator
        /// </summary>
        GreaterOrEqual = 32,
        /// <summary>
        /// Exclude the Contains operator
        /// </summary>
        Contains = 64,
        /// <summary>
        /// Exclude the DoesNotContain operator
        /// </summary>
        DoesNotContain = 128,
        /// <summary>
        /// Exclude the EndsWith operator
        /// </summary>
        EndsWith = 256,
        /// <summary>
        /// Exclude the DoesNotEndWith operator
        /// </summary>
        DoesNotEndWith = 512,
        /// <summary>
        /// Exclude the StartsWith operator
        /// </summary>
        StartsWith = 1024,
        /// <summary>
        /// Exclude the DoesNotStartWith operator
        /// </summary>
        DoesNotStartWith = 2048
    }
}
