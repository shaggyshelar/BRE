using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// Specifies how to handle various LINQ providers when building expression trees. 
    /// The Default means that the engine will try to identify a type of a LINQ provider automatically.
    /// SQL is for LINQ-to-SQL providers and Entities is for LINQ-to-Entities. The reason why this enum
    /// exists is because various providers handle string operation differently and most of the time it
    /// is impossible to auto-determine the behavior of the current provider.
    /// </summary>
    public enum LinqProviderType
    {
        Default,
        SQL,
        Entities
    }
}
