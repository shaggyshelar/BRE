using ESPL.Rule.Common;
using ESPL.Rule.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;

namespace ESPL.Rule.MVC
{
    /// <summary>
    /// Code Effects style manager
    /// </summary>
    public class StyleManager
    {
        private ViewContext viewContext;

        public static object Key
        {
            get
            {
                return typeof(StyleManager).AssemblyQualifiedName;
            }
        }

        public ThemeType Theme
        {
            get;
            set;
        }

        public StyleManager(ViewContext viewContext)
        {
            this.viewContext = viewContext;
            if (this.viewContext.HttpContext.Items[StyleManager.Key] != null)
            {
                throw new RuleEditorMVCException(RuleEditorMVCException.ErrorIds.OnlyOneStyleManagerIsAllowed, new string[0]);
            }
            this.viewContext.HttpContext.Items[StyleManager.Key] = this;
            this.Theme = ThemeType.Gray;
        }

        public StyleManager SetTheme(ThemeType theme)
        {
            this.Theme = theme;
            return this;
        }

        public void Render()
        {
            if (this.Theme == ThemeType.None)
            {
                return;
            }
            ThemeManager themeManager = new ThemeManager(this.Theme);
            using (HtmlTextWriter htmlTextWriter = new HtmlTextWriter(this.viewContext.Writer))
            {
                htmlTextWriter.Indent++;
                htmlTextWriter.WriteLine();
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                htmlTextWriter.AddAttribute(themeManager.StyleTagAttribute, "true");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Href, themeManager.GetLinkUrl());
                htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.Link);
                htmlTextWriter.RenderEndTag();
                htmlTextWriter.WriteLine();
            }
        }
    }
}
