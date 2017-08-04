using ESPL.Rule.Client;
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
    /// Code Effects script manager class
    /// </summary>
    public class ScriptManager
    {
        private ViewContext viewContext;

        private List<RuleEditor> ruleEditors = new List<RuleEditor>();

        public static object Key
        {
            get
            {
                return typeof(ScriptManager).AssemblyQualifiedName;
            }
        }

        public ScriptManager(ViewContext viewContext)
        {
            this.viewContext = viewContext;
            if (this.viewContext.HttpContext.Items[ScriptManager.Key] != null)
            {
                throw new RuleEditorMVCException(RuleEditorMVCException.ErrorIds.OnlyOneScriptManagerIsAllowed, new string[0]);
            }
            this.viewContext.HttpContext.Items[ScriptManager.Key] = this;
        }

        public void Register(RuleEditor editor)
        {
            if (!this.ruleEditors.Contains(editor))
            {
                this.ruleEditors.Add(editor);
            }
        }

        public void Render()
        {
            if (this.ruleEditors.Count == 0)
            {
                return;
            }
            using (HtmlTextWriter htmlTextWriter = new HtmlTextWriter(this.viewContext.Writer))
            {
                htmlTextWriter.Indent++;
                htmlTextWriter.WriteLine();
                Page page = new Page();
                string webResourceUrl = page.ClientScript.GetWebResourceUrl(typeof(ESPL.Rule.Asp.RuleEditor), "CodeEffects.Rule.Resources.Scripts.Control.js");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Src, webResourceUrl);
                htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.Script);
                htmlTextWriter.RenderEndTag();
                htmlTextWriter.WriteLine();
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
                htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.Script);
                htmlTextWriter.Write("//<![CDATA[");
                htmlTextWriter.Write(MarkupManager.RenderInitials());
                bool flag = false;
                bool flag2 = false;
                foreach (RuleEditor current in this.ruleEditors)
                {
                    if (!flag && current.ShowHelpString)
                    {
                        flag = true;
                    }
                    if (!flag2 && current.ShowToolBar)
                    {
                        flag2 = true;
                    }
                }
                this.ruleEditors[0].GetHelpXml();
                Labels labels = new Labels(this.ruleEditors[0].GetHelpXml(), flag2, RuleType.Evaluation);
                if (flag)
                {
                    htmlTextWriter.Write(MarkupManager.RenderHelp(labels.GetUiMessages()));
                }
                htmlTextWriter.Write(MarkupManager.RenderErrors(labels.GetErrorMessages()));
                foreach (RuleEditor current2 in this.ruleEditors)
                {
                    current2.RenderScript(htmlTextWriter);
                }
                htmlTextWriter.Write("//]]>");
                htmlTextWriter.RenderEndTag();
                htmlTextWriter.WriteLine();
            }
        }
    }
}
