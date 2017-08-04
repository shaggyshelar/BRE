using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Resources
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct SchemaFileNames
    {
        internal const string Rule = "CodeEffects.Rule.Resources.Schemas.Rule.xsd";

        internal const string UI = "CodeEffects.Rule.Resources.Schemas.UI.xsd";

        internal const string Source2 = "CodeEffects.Rule.Resources.Schemas.Source.2.xsd";

        internal const string Source3 = "CodeEffects.Rule.Resources.Schemas.Source.3.xsd";

        internal const string Source4 = "CodeEffects.Rule.Resources.Schemas.Source.4.xsd";

        internal const string Source42 = "CodeEffects.Rule.Resources.Schemas.Source.4.2.xsd";
    }
}
