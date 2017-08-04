using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    internal static class Encoder
    {
        internal static string Sanitize(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }
            return str.Replace("'", "&#39;").Replace("\"", "&quot;").Replace("\\", "&#92;");
        }

        internal static string Desanitize(string str)
        {
            return str;
        }

        internal static string ClearXml(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }
            return str.Replace("&quot;", "\"").Replace("&#92;", "\\");
        }

        internal static string GetHashToken(MethodInfo m)
        {
            return Encoder.GetHashToken(m.DeclaringType.FullName + m.ToString());
        }

        internal static string GetHashToken(string value)
        {
            string result;
            using (MD5 mD = MD5.Create())
            {
                byte[] source = mD.ComputeHash(Encoding.Unicode.GetBytes(value));
                result = string.Join(string.Empty, (from x in source
                                                    select x.ToString("X2")).ToArray<string>());
            }
            return result;
        }
    }
}
