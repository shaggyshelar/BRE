using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    internal sealed class Vector
    {
        internal static bool Compiled
        {
            get
            {
                bool result;
                try
                {
                    if (!Build.Limits)
                    {
                        result = true;
                    }
                    else
                    {
                        result = Build.Compiled;
                    }
                }
                catch
                {
                    result = false;
                }
                return result;
            }
        }

        internal static void DelayIfDemo()
        {
            if (!Vector.Compiled)
            {
                Thread.Sleep(1000);
            }
        }

        internal static bool IsDemo(string requestHost, string serverName, bool isLocal)
        {
            bool result;
            try
            {
                if (isLocal)
                {
                    result = false;
                }
                else if (!Build.Limits)
                {
                    result = false;
                }
                else if (!Vector.Compiled)
                {
                    result = true;
                }
                else
                {
                    result = (!Vector.Match(requestHost) && !Vector.Match(serverName));
                }
            }
            catch
            {
                result = true;
            }
            return result;
        }

        private static bool Match(string input)
        {
            if (!Build.Limits)
            {
                return true;
            }
            if (string.IsNullOrEmpty(Build.Pattern))
            {
                return false;
            }
            if (Build.Wildcard)
            {
                return Regex.IsMatch(input, string.Format("^([\\w\\-]+\\.)*({0})$", Build.Pattern.Replace(".", "\\.")), RegexOptions.IgnoreCase);
            }
            return input.ToLower() == Build.Pattern.ToLower();
        }
    }
}
