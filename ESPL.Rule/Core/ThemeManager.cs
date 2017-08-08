using ESPL.Rule.Common;
using ESPL.Rule.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace ESPL.Rule.Core
{
    internal sealed class ThemeManager
    {
        private ThemeType theme;

        internal string StyleTagAttribute
        {
            get
            {
                return "ce008";
            }
        }

        internal string StyleTagID
        {
            get
            {
                return "ce003" + this.theme.ToString();
            }
        }

        public ThemeManager(ThemeType theme)
        {
            this.theme = theme;
        }

        internal string GetLinkUrl()
        {
            return new Page().ClientScript.GetWebResourceUrl(typeof(RuleEditor), Converter.ThemeTypeToResourceName(this.theme));
            //throw new NotImplementedException();//TODO:
        }
    }
}
