using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    internal sealed class Build
    {
        internal static bool Limits
        {
            get
            {
                return true;
            }
        }

        internal static bool Wildcard
        {
            get
            {
                return false;
            }
        }

        internal static bool Compiled
        {
            get
            {
                return false;
            }
        }

        internal static string Pattern
        {
            get
            {
                return string.Empty;
            }
        }

        internal static string Number
        {
            get
            {
                return "Free version";
            }
        }

        private Build()
        {
        }
    }
}
