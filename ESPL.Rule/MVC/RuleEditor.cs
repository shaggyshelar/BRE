using ESPL.Rule.Client;
using ESPL.Rule.Common;
using ESPL.Rule.Core;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using System.Xml;

namespace ESPL.Rule.MVC
{
    /// <summary>
    /// Provides MVC UI functionality
    /// </summary>
    public class RuleEditor : IControl
    {
        private RuleType mode;

        private ViewContext viewContext;

        private bool showToolBar = true;

        private RuleModel rule;

        private string dataFieldID
        {
            get
            {
                return this.Id + "Data";
            }
        }

        private bool valid
        {
            get
            {
                return this.Rule.InvalidElements.Count == 0 && !this.Rule.NameIsInvalid;
            }
        }

        /// <summary>
        /// Gets or sets server ID of the instance of RuleEditor. Used to distinguish this instance from
        /// all other instances that may be declared on the same view. The value of this property should
        /// be unique for the current view. This requirement is not enforced by Code Effects component.
        /// </summary>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the controller that declares the Save action.
        /// This property is ignored id ShowToolBar is set to False.
        /// </summary>
        public string SaveController
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the Save action.
        /// This property is ignored id ShowToolBar is set to False.
        /// </summary>
        public string SaveAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the controller that declares the Delete action.
        /// This property is ignored id ShowToolBar is set to False.
        /// </summary>
        public string DeleteController
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the Delete action.
        /// This property is ignored id ShowToolBar is set to False.
        /// </summary>
        public string DeleteAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the controller that declares the Load action.
        /// This property is ignored id ShowToolBar is set to False.
        /// </summary>
        public string LoadController
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the Load action.
        /// This property is ignored id ShowToolBar is set to False.
        /// </summary>
        public string LoadAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the XML document of the custom help XML. HelpXml property allows you to specify
        /// a Help XML that contains different values for Help String messages, Tool Bar
        /// labels and rule validation error messages. This could be helpful for multilingual applications.
        /// </summary>
        public XmlDocument HelpXml
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the full path to the custom help XML file. HelpXmlFile property allows you to specify
        /// a Help XML that contains different values for Help String messages, Tool Bar
        /// labels and rule validation error messages. This could be helpful for multilingual applications.
        /// </summary>
        public string HelpXmlFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value that toggles the visibility of Help String. The default value is True.
        /// </summary>
        public bool ShowHelpString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value that toggles "dots" at the beginning of each new
        /// line in the Rule Area. The default value is False.
        /// </summary>
        public bool ShowLineDots
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value that toggles the visibility of the context menu when user uses keyboard's
        /// "right arrow" key to navigate the rule. The default value is True.
        /// </summary>
        public bool ShowMenuOnRightArrowKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value that toggles the visibility of the context menu when user
        /// clicks a rule element. The default value is True.
        /// </summary>
        public bool ShowMenuOnElementClicked
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value that toggles the visibility of description of rule fields, in-rule methods,
        /// actions and nested rules for those elements that have their descriptions set. The default value is True.
        /// </summary>
        public bool ShowDescriptionsOnMouseHover
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value that toggles the visibility of Tool Bar. The default value is
        /// False if Mode property is set to CodeEffects.Rule.Common.RuleType.Filter and True for all other modes.
        /// </summary>
        public bool ShowToolBar
        {
            get
            {
                return this.showToolBar;
            }
            set
            {
                if (!value)
                {
                    this.Rule.SkipNameValidation = true;
                }
                this.showToolBar = value;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether Code Effects component should alphabetically order fields and actions
        /// in menus or keep the order in which they are declared in the source object. The default value is False.
        /// </summary>
        public bool KeepDeclaredOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value that defines what type of rules users can create.
        /// Value of RuleType.Evaluation limits Code Effects UI to evaluation type rules only.
        /// Value of RuleType.Execution permits both execution and evaluation types.
        /// The default value is CodeEffects.Rule.Common.RuleType.Evaluation.
        /// </summary>
        public RuleType Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (value == RuleType.Filter)
                {
                    this.showToolBar = false;
                }
                this.mode = value;
            }
        }

        /// <summary>
        /// This property is not intended for public use. Use the StyleManager class instead.
        /// </summary>
        public ThemeType Theme
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value that toggles Code Effects' UI "client-only" mode
        /// </summary>
        public bool ClientOnly
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets collection of rules that appear in the Rules menu of the Tool Bar. This collection can contain
        /// rules of all types. This property is ignored if ShowToolBar is set to False. 
        /// </summary>
        public ICollection<MenuItem> ToolBarRules
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets collection of rules that appear in context menus of Code Effects component. This collection can contain
        /// only rules of evaluation type (this requirement is not enforced by Code Effects component).
        /// </summary>
        public ICollection<MenuItem> ContextMenuRules
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets collection of operators that Code Effects component should exclude from using on the client
        /// </summary>
        public ICollection<Operator> ExcludedOperators
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets collection of named Dynamic Menu Data Source delegates.
        /// </summary>
        public ICollection<DataSourceHolder> DataSources
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the rule model
        /// </summary>
        public RuleModel Rule
        {
            get
            {
                return this.rule;
            }
            set
            {
                this.rule = value;
                if (!this.showToolBar)
                {
                    this.rule.SkipNameValidation = true;
                }
            }
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="id">ID of this instance of Code Effects component</param>
        public RuleEditor(string id)
        {
            this.Id = (string.IsNullOrWhiteSpace(id) ? null : id);
            this.Mode = RuleType.Evaluation;
            this.ClientOnly = (this.ShowLineDots = (this.KeepDeclaredOrder = false));
            this.rule = new RuleModel();
            this.showToolBar = (this.Mode != RuleType.Filter);
            this.ShowMenuOnRightArrowKey = (this.ShowMenuOnElementClicked = (this.ShowDescriptionsOnMouseHover = (this.ShowHelpString = true)));
            this.Theme = ThemeType.Gray;
            this.ExcludedOperators = new List<Operator>();
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        public RuleEditor(ViewContext viewContext)
            : this(string.Empty)
        {
            this.viewContext = viewContext;
        }

        internal void RenderScript(HtmlTextWriter writer)
        {
            MarkupData conditions = this.GetConditions();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(MarkupManager.RenderControlInstance(this.Id, this.Id, this.ClientOnly ? null : this.dataFieldID, this.viewContext.HttpContext.Request.Browser.Platform.ToLower() == "winxp", false));
            if (!this.ClientOnly)
            {
                stringBuilder.Append(MarkupManager.RenderSettings(this, conditions, true));
                RuleValidator.ForceTokens(this.Rule.Elements);
                stringBuilder.Append(MarkupManager.RenderRuleDataForLoading(this.Rule, this.Id));
                if (!this.valid)
                {
                    if (this.Rule.InvalidElements != null && this.Rule.InvalidElements.Count > 0)
                    {
                        foreach (InvalidElement current in this.Rule.InvalidElements)
                        {
                            foreach (XmlNode xmlNode in this.GetHelpXml().SelectSingleNode("/codeeffects/validation").ChildNodes)
                            {
                                if (xmlNode.NodeType != XmlNodeType.Comment && xmlNode.Name == current.Hint)
                                {
                                    current.Hint = xmlNode.InnerText;
                                    break;
                                }
                            }
                        }
                    }
                    stringBuilder.Append(MarkupManager.RenderInvalidDataForLoading(this.Rule, this.Id));
                }
            }
            writer.Write(stringBuilder.ToString());
        }

        /// <summary>
        /// Renders this instance of RuleEditor
        /// </summary>
        public void Render()
        {
            if (!this.Rule.IsLoadedRuleOfEvalType.HasValue)
            {
                this.Rule.IsLoadedRuleOfEvalType = new bool?(this.Mode == RuleType.Evaluation || this.mode == RuleType.Filter);
            }
            if (this.Rule.InvalidElements.Count == 0 && !this.Rule.NameIsInvalid)
            {
                RuleValidator.EnsureValues(this.Rule.Elements);
            }
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                throw new RuleEditorMVCException(RuleEditorMVCException.ErrorIds.RuleEditorIdIsNull, new string[0]);
            }
            using (HtmlTextWriter htmlTextWriter = new HtmlTextWriter(this.viewContext.Writer))
            {
                htmlTextWriter.Indent++;
                htmlTextWriter.WriteLine();
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Id, this.Id);
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Name, this.Id);
                htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.Div);
                if (!this.ClientOnly)
                {
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Id, this.dataFieldID);
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Name, this.Id);
                    htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.Input);
                }
                htmlTextWriter.WriteLine();
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Href, "http://codeeffects.com");
                htmlTextWriter.AddAttribute("ce002", this.IsDemo() ? "true" : "false");
                htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.A);
                htmlTextWriter.WriteEncodedText(".");
                htmlTextWriter.RenderEndTag();
                if (!this.ClientOnly)
                {
                    htmlTextWriter.RenderEndTag();
                }
                htmlTextWriter.RenderEndTag();
                htmlTextWriter.WriteLine();
            }
        }

        /// <summary>
        /// Returns Code Effects' UI settings. Should be used by the "client-only" RuleEditor instances.
        /// </summary>
        /// <returns>Json-formatted UI settings</returns>
        public string GetClientSettings()
        {
            return MarkupManager.RenderSettings(this, this.GetConditions(), false);
        }

        /// <summary>
        /// Returns rule's client data. Should be used only by the "client-only" RuleEditor instances.
        /// </summary>
        /// <returns>This method returns null if the rule is not valid or empty. Otherwise, returns json-formatted rule data</returns>
        public string GetClientRuleData()
        {
            if (this.Rule.IsEmpty() || !this.Rule.IsValid())
            {
                return null;
            }
            return MarkupManager.RenderRuleClientData(this.Rule);
        }

        /// <summary>
        /// Loads rule's client data into the RuleEditor.
        /// </summary>
        /// <param name="ruleClientData">Json-formatted rule data</param>
        public void LoadClientData(string ruleClientData)
        {
            RuleDataTypeConverter ruleDataTypeConverter = new RuleDataTypeConverter();
            this.Rule = ruleDataTypeConverter.GetRuleModel(ruleClientData, this.Rule.SourceAssembly, this.Rule.SourceType, this.Rule.SourceXml);
            if (this.Rule.Elements != null && this.Rule.Elements.Count > 0)
            {
                RuleValidator.EnsureTokens(this.Rule.Elements, this.GetSourceXml());
            }
            if (string.IsNullOrWhiteSpace(this.Rule.Id))
            {
                this.Rule.Id = Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Returns data of the invalid rule members. Should be used by the "client-only" RuleEditor instances.
        /// </summary>
        /// <returns>Json-formatted array of invalid elements</returns>
        public string GetClientInvalidData()
        {
            XmlDocument helpXml = this.GetHelpXml();
            foreach (InvalidElement current in this.Rule.InvalidElements)
            {
                current.Hint = RuleValidator.GetValidationMessage(helpXml, current.Hint);
            }
            return MarkupManager.RenderRuleInvalidClientData(this.Rule);
        }

        /// <summary>
        /// Returns XML document of the source object
        /// </summary>
        /// <returns>Xml document as string</returns>
        public XmlDocument GetSourceXml()
        {
            if (this.Rule.SourceXml != null)
            {
                return this.Rule.SourceXml;
            }
            List<Type> processedTypes = new List<Type>();
            this.Rule.SourceXml = SourceLoader.LoadSourceXml(this.Rule.SourceAssembly, this.Rule.SourceType, this.Rule.SourceXmlFile, processedTypes);
            return this.Rule.SourceXml;
        }

        /// <summary>
        /// Returns the string representation of the default Help XML document. Use this document as a template for custom help documents.
        /// </summary>
        /// <returns>String representation of the default help XML</returns>
        public XmlDocument GetHelpXml()
        {
            if (this.HelpXml != null)
            {
                return this.HelpXml;
            }
            if (string.IsNullOrEmpty(this.HelpXmlFile))
            {
                string xml = (this.Mode == RuleType.Filter) ? Resource.FilterHelp : Resource.RuleHelp;
                this.HelpXml = new XmlDocument();
                this.HelpXml.LoadXml(xml);
                return this.HelpXml;
            }
            XmlDocument helpXml;
            try
            {
                this.HelpXml = new XmlDocument();
                this.HelpXml.Load(this.HelpXmlFile);
                helpXml = this.HelpXml;
            }
            catch (Exception ex)
            {
                throw new MalformedXmlException(MalformedXmlException.ErrorIds.MalformedOrMissingHelpXML, new string[]
				{
					ex.Message
				});
            }
            return helpXml;
        }

        /// <summary>
        /// Returns text representation of the rule that is currently displayed in the Rule Area. Returns base.ToString() if no rules have been loaded and the Rule Area is empty.
        /// </summary>
        /// <returns>Text representation of the current rule</returns>
        public override string ToString()
        {
            return this.ToString(null, null);
        }

        /// <summary>
        /// Returns text representation of the rule that is currently displayed in the Rule Area. Returns base.ToString() if no rules have been loaded and the Rule Area is empty.
        /// </summary>
        /// <param name="ruleDelegate">Delegate of the method that takes a rule ID and returns rule's XML string</param>
        /// <returns></returns>
        public string ToString(GetRuleDelegate ruleDelegate)
        {
            return this.ToString(ruleDelegate, null);
        }

        public string ToString(Dictionary<string, GetDataSourceDelegate> dataSources)
        {
            return this.ToString(null, dataSources);
        }

        public string ToString(GetRuleDelegate ruleDelegate, Dictionary<string, GetDataSourceDelegate> dataSources)
        {
            if (this.Rule.IsEmpty())
            {
                return base.ToString();
            }
            Labels labels = new Labels(this.GetHelpXml(), this.ShowToolBar, this.Mode);
            return RuleLoader.GetString(this.Rule.Elements, this.GetSourceXml(), labels, ruleDelegate, dataSources);
        }

        private bool IsDemo()
        {
            return Vector.IsDemo(this.viewContext.HttpContext.Request.Url.Host, this.viewContext.HttpContext.Request.ServerVariables["SERVER_NAME"], this.viewContext.HttpContext.Request.IsLocal);
        }

        private MarkupData GetConditions()
        {
            MarkupData markupData = new MarkupData();
            markupData.Pattern = Pattern.Mvc;
            if (this.ShowToolBar && !this.ClientOnly)
            {
                markupData.MvcActions = new ActionUrls();
                UrlHelper urlHelper = new UrlHelper(this.viewContext.RequestContext);
                if (!string.IsNullOrWhiteSpace(this.SaveAction) && !string.IsNullOrWhiteSpace(this.SaveController))
                {
                    markupData.MvcActions.SaveAction = urlHelper.Action(this.SaveAction, this.SaveController);
                }
                if (!string.IsNullOrWhiteSpace(this.DeleteAction) && !string.IsNullOrWhiteSpace(this.DeleteController))
                {
                    markupData.MvcActions.DeleteAction = urlHelper.Action(this.DeleteAction, this.DeleteController, new
                    {
                        id = "_ce_"
                    });
                }
                if (!string.IsNullOrWhiteSpace(this.LoadAction) && !string.IsNullOrWhiteSpace(this.LoadController))
                {
                    markupData.MvcActions.LoadAction = urlHelper.Action(this.LoadAction, this.LoadController, new
                    {
                        id = "_ce_"
                    });
                }
            }
            if (this.Theme != ThemeType.None)
            {
                markupData.ThemeFactory = new ThemeManager(this.Theme);
            }
            Settings settings = new Settings(this.GetHelpXml(), this.ToolBarRules, this.ShowToolBar, this.Mode);
            settings.Load(this, this.ContextMenuRules, this.GetSourceXml(), this.DataSources);
            markupData.Settings = settings;
            markupData.ControlServerID = this.Id;
            if (this.Mode == RuleType.Loop || this.Mode == RuleType.Ruleset)
            {
                markupData.IsLoadedRuleOfEvalType = false;
                this.Rule.IsLoadedRuleOfEvalType = new bool?(false);
            }
            else if (!this.Rule.IsLoadedRuleOfEvalType.HasValue)
            {
                markupData.IsLoadedRuleOfEvalType = (this.Mode == RuleType.Evaluation || this.Mode == RuleType.Filter);
            }
            else
            {
                markupData.IsLoadedRuleOfEvalType = (this.Rule.IsLoadedRuleOfEvalType == true);
            }
            markupData.Mode = this.Mode;
            return markupData;
        }
    }
}
