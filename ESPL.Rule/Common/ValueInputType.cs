using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// Defines the input type that user can use to set the value of particular field
    /// </summary>
    public enum ValueInputType
    {
        /// <summary>
        /// Only allows selection of other fields, in-rule methods or reusable rules
        /// </summary>
        Fields,
        /// <summary>
        /// Only allows user to input the value or pick it from predefined ranges
        /// </summary>
        User = 2,
        /// <summary>
        /// Allows all types of value input
        /// </summary>
        All = 4
    }
}
