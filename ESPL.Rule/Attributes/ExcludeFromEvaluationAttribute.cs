using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// This attribute is used to exclude class members from rule evaluation and creation processes
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ExcludeFromEvaluationAttribute : Attribute
    {
    }
}
