using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    public enum CalculationType
    {
        Field,
        LeftParenthesis,
        RightParenthesis,
        Multiplication,
        Division,
        Addition = 6,
        Subtraction,
        Number,
        None,
        Function
    }
}
