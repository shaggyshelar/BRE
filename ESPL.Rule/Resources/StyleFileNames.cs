using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Resources
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct StyleFileNames
    {
        internal const string Gray = "CodeEffects.Rule.Resources.Styles.Gray.css";

        internal const string White = "CodeEffects.Rule.Resources.Styles.White.css";

        internal const string Green = "CodeEffects.Rule.Resources.Styles.Green.css";

        internal const string Red = "CodeEffects.Rule.Resources.Styles.Red.css";

        internal const string Black = "CodeEffects.Rule.Resources.Styles.Black.css";

        internal const string Navy = "CodeEffects.Rule.Resources.Styles.Navy.css";

        internal const string Blue = "CodeEffects.Rule.Resources.Styles.Blue.css";
    }
}
