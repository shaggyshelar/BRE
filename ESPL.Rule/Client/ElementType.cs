using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    public enum ElementType
    {
        Flow,
        Field,
        Function,
        Operator,
        Value,
        Clause = 6,
        Action,
        LeftParenthesis,
        RightParenthesis,
        LeftBracket,
        RightBracket,
        Calculation,
        Tab,
        NewLine = 15,
        HtmlTag,
        Setter,
        LeftSource,
        RightSource,
        Where,
        Exists = 22
    }
}
