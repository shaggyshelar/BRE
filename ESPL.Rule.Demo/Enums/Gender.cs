using ESPL.Rule.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESPL.Rule.Demo.Enums
{
    public enum Gender
    {
        Male,
        Female,
        [ExcludeFromEvaluation]
        Unknown
    }

    public enum State
    {
        Arizona,
        California,
        Florida,
        [EnumItem("North Carolina")]
        NorthCarolina,
        Georgia,
        [ExcludeFromEvaluation]
        Unknown
    }
}