using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    /// <summary>
    /// This enum is not intended for public use
    /// </summary>
    public enum CollectionType
    {
        Value,
        Reference = 2,
        Generic = 4,
        None = 8
    }
}
