using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// This enum specifies how to treat multiple rules when evaluated at once.
    /// EvaluationScope.All means all rules will be evaluated using logical AND operator.
    /// EvaluationScope.AtLeastOne means that the OR operator will be used.
    /// </summary>
    public enum EvaluationScope
    {
        All,
        AtLeastOne
    }
}
