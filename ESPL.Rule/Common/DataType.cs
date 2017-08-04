using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// Data types used on the client by ESPL control
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Represents String type on the client
        /// </summary>
        String,
        /// <summary>
        /// Represents all supported numeric types on the client
        /// </summary>
        Numeric,
        /// <summary>
        /// Represents DateTime types on the client
        /// </summary>
        Date,
        /// <summary>
        /// Represents TimeSpan types on the client
        /// </summary>
        Time,
        /// <summary>
        /// Represents Boolean type on the client
        /// </summary>
        Bool,
        /// <summary>
        /// Represents enumerators on the client
        /// </summary>
        Enum,
        /// <summary>
        /// Represents enumerables and arrays on the client
        /// </summary>
        Collection
    }
}
