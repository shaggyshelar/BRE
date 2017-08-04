using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ESPL.Rule.MVC
{
    public static class HtmlHelperExtension
    {
        public static ComponentFactory CodeEffects(this HtmlHelper helper)
        {
            return new ComponentFactory(helper);
        }
    }
}
