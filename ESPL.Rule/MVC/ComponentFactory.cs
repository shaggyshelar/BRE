using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;

namespace ESPL.Rule.MVC
{
    public class ComponentFactory
    {
        private ScriptManager scriptManager;

        private StyleManager styleManager;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public HtmlHelper HtmlHelper
        {
            get;
            set;
        }

        public ComponentFactory(HtmlHelper helper)
        {
            this.HtmlHelper = helper;
            this.scriptManager = ((this.HtmlHelper.ViewContext.HttpContext.Items[ScriptManager.Key] as ScriptManager) ?? new ScriptManager(this.HtmlHelper.ViewContext));
            this.styleManager = ((this.HtmlHelper.ViewContext.HttpContext.Items[StyleManager.Key] as StyleManager) ?? new StyleManager(this.HtmlHelper.ViewContext));
        }

        public RuleEditorBuilder RuleEditor()
        {
            RuleEditorBuilder ruleEditorBuilder = new RuleEditorBuilder(this.HtmlHelper.ViewContext);
            ruleEditorBuilder.Theme(this.styleManager.Theme);
            this.scriptManager.Register(ruleEditorBuilder.Editor);
            return ruleEditorBuilder;
        }

        public StyleManager Styles()
        {
            return this.styleManager;
        }

        public ScriptManager Scripts()
        {
            return this.scriptManager;
        }
    }
}
