using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    public enum OperatorType
    {
        String,
        Numeric,
        Date,
        Time,
        Bool,
        Enum = 6,
        Collection = 8,
        None = 16
    }
}
