using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Models
{
    internal interface IControl
    {
        bool ShowHelpString
        {
            get;
            set;
        }

        bool ClientOnly
        {
            get;
            set;
        }

        bool ShowLineDots
        {
            get;
            set;
        }

        bool ShowMenuOnRightArrowKey
        {
            get;
            set;
        }

        bool ShowMenuOnElementClicked
        {
            get;
            set;
        }

        bool ShowDescriptionsOnMouseHover
        {
            get;
            set;
        }

        bool ShowToolBar
        {
            get;
            set;
        }

        bool KeepDeclaredOrder
        {
            get;
            set;
        }

        RuleType Mode
        {
            get;
            set;
        }

        ThemeType Theme
        {
            get;
            set;
        }

        ICollection<Operator> ExcludedOperators
        {
            get;
            set;
        }
    }
}
