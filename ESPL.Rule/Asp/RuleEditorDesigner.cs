using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.Design;

namespace ESPL.Rule.Asp
{
    internal class RuleEditorDesigner : ControlDesigner
    {
        private string html = "<div><span style=\"color: Gray\">Rule authoring is not available at design-time.<span></div>";

        public override string GetDesignTimeHtml()
        {
            return this.html;
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            return this.html;
        }

        protected override string GetEmptyDesignTimeHtml()
        {
            return this.html;
        }
    }
}
