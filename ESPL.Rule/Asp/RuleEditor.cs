using ESPL.Rule.Client;
using ESPL.Rule.Common;
using ESPL.Rule.Core;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace ESPL.Rule.Asp
{
    /// <summary>
    /// Provides Code Effects' ASP.NET client functionality
    /// </summary>
    [DefaultProperty("SourceType"), Designer(typeof(RuleEditorDesigner)), DesignTimeVisible(true), ToolboxData("<{0}:RuleEditor runat=\"server\"></{0}:RuleEditor>")]
    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class RuleEditor : Panel, IControl
    {
        private const string HiddenFieldIdPrefix = "ce004_";

        private const string CacheSourceKeyPrefix = "appCodeEffectsRuleSourceXmlPath";

        private const string CacheHelpKeyPrefix = "appCodeEffectsRuleHelpXmlPath";

        private const string AnchorIdPrefix = "ce002";

        private HiddenField hiddenField;

        private RuleModel rule;

        /// <summary>
        /// Code Effects raises this event when the user clicks the Save button on the Tool Bar.
        /// Ignored by Code Effects if ShowToolBar is set to False
        /// </summary>
        public event SaveEventHandler SaveRule;

        /// <summary>
        /// Code Effects raises this event when the user clicks the Delete button on the Tool Bar.
        /// Ignored by Code Effects if ShowToolBar is set to False
        /// </summary>
        public event RuleEventHandler DeleteRule;

        /// <summary>
        /// Code Effects raises this event when the user selects a rule from the Rules menu on the Tool Bar.
        /// Ignored by Code Effects if ShowToolBar is set to False
        /// </summary>
        public event RuleEventHandler LoadRule;

        /// <summary>
        /// Gets or sets collection of rules that appear in the Rules menu of the Tool Bar. This collection can contain
        /// rules of all types. This property is ignored if ShowToolBar is set to False. 
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always)]
        public ICollection<ESPL.Rule.Common.MenuItem> ToolBarRules
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets collection of rules that appear in context menus of Code Effects. This collection can contain
        /// only rules of evaluation type (this requirement is not enforced by Code Effects).
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always)]
        public ICollection<ESPL.Rule.Common.MenuItem> ContextMenuRules
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the value indicating whether the submitted rule is empty
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always)]
        public bool IsEmpty
        {
            get
            {
                return this.rule.Elements.Count == 0;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the submitted rule is valid
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always)]
        public bool IsValid
        {
            get
            {
                return this.rule.InvalidElements.Count == 0 && !this.rule.NameIsInvalid;
            }
        }

        /// <summary>
        /// Gets or sets collection of named Dynamic Menu Data Source delegates.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always)]
        public ICollection<DataSourceHolder> DataSources
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a method delegate that returns rule XML by rule's ID.
        /// Set this property if you store each rule in a separate XML document.
        /// It can be used by LoadRuleXml, LoadRuleFile and ToString() overload to
        /// look up rules by their IDs if you store each rule in a separate document.
        /// It can be used by rule validation process to check for circular
        /// references in the current rule as well. See the Reusable Rules
        /// documentation topic for details.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always)]
        public GetRuleDelegate GetRuleDelegate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the XML document of the custom help XML. HelpXml property allows you to specify
        /// a Help XML that contains different values for Help String messages, Tool Bar
        /// labels and rule validation error messages. This could be helpful for multilingual applications.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always)]
        public XmlDocument HelpXml
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets collection of operators that Code Effects should exclude from using on the client
        /// </summary>
        public ICollection<Operator> ExcludedOperators
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of XML document of source object used by this instance of RuleEditor.
        /// Use the GetSourceXml() method to obtain XML document of the current source if you intend to
        /// manually alter source object's XML.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always)]
        public XmlDocument SourceXml
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of source object used by this instance of RuleEditor.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always)]
        public Type Source
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of the full path of custom XML file that this instance of RuleEditor should use as its source object.
        /// Use the GetSourceXml() method to obtain XML document of the current source if you intend to
        /// manually alter source object's XML.
        /// </summary>
        [Bindable(true), Category("Source"), Localizable(true)]
        public string SourceXmlFile
        {
            get
            {
                return (string)this.ViewState["ce701"];
            }
            set
            {
                this.ViewState["ce701"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the type name of the source object. Use typeof(YourSource).FullName to obtain this value
        /// </summary>
        [Bindable(true), Category("Source"), Localizable(true)]
        public string SourceType
        {
            get
            {
                return (string)this.ViewState["ce702"];
            }
            set
            {
                this.ViewState["ce702"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the qualified name of the assembly that declares the source object.
        /// </summary>
        [Bindable(true), Category("Source"), Localizable(true)]
        public string SourceAssembly
        {
            get
            {
                return (string)this.ViewState["ce703"];
            }
            set
            {
                this.ViewState["ce703"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the source object's data should
        /// be cached by the server. The default value is False.
        /// </summary>
        [Bindable(true), Category("Source"), DefaultValue(false), Localizable(true)]
        public bool CacheSource
        {
            get
            {
                return ((bool?)this.ViewState["ce704"]).GetValueOrDefault();
            }
            set
            {
                this.ViewState["ce704"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that toggles Code Effects' UI "client-only"
        /// mode. The default value is False.
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(false), Localizable(true)]
        public bool ClientOnly
        {
            get
            {
                return ((bool?)this.ViewState["ce705"]).GetValueOrDefault();
            }
            set
            {
                this.ViewState["ce705"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that toggles "dots" at the beginning of
        /// each new line in the Rule Area. The default value is False.
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(false), Localizable(true)]
        public bool ShowLineDots
        {
            get
            {
                return ((bool?)this.ViewState["ce706"]).GetValueOrDefault();
            }
            set
            {
                this.ViewState["ce706"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that toggles the visibility of the context menu when user uses keyboard's
        /// "right arrow" key to navigate the rule. The default value is True.
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(true), Localizable(true)]
        public bool ShowMenuOnRightArrowKey
        {
            get
            {
                return ((bool?)this.ViewState["ce707"]).GetValueOrDefault();
            }
            set
            {
                this.ViewState["ce707"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that toggles the visibility of the context menu containing
        /// related items when user clicks a rule element. The default value is True.
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(true), Localizable(true)]
        public bool ShowMenuOnElementClicked
        {
            get
            {
                return ((bool?)this.ViewState["ce708"]).GetValueOrDefault();
            }
            set
            {
                this.ViewState["ce708"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that toggles the visibility of description of rule fields, in-rule methods,
        /// actions and nested rules for those elements that have their descriptions set. The default value is True.
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(true), Localizable(true)]
        public bool ShowDescriptionsOnMouseHover
        {
            get
            {
                return ((bool?)this.ViewState["ce711"]).GetValueOrDefault();
            }
            set
            {
                this.ViewState["ce711"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether Code Effects should require each rule to have a name.
        /// This property is ignored if ShowToolBar is set to False. The default value is True.
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(true), Localizable(true)]
        public bool RuleNameIsRequired
        {
            get
            {
                return ((bool?)this.ViewState["ce712"]).GetValueOrDefault();
            }
            set
            {
                this.ViewState["ce712"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that toggles the visibility of Tool Bar. The default value is
        /// False if Mode property is set to CodeEffects.Rule.Common.RuleType.Filter and True for all other modes.
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(true), Localizable(true)]
        public bool ShowToolBar
        {
            get
            {
                return ((bool?)this.ViewState["ce709"]).GetValueOrDefault();
            }
            set
            {
                if (!value)
                {
                    this.rule.SkipNameValidation = true;
                    this.RuleNameIsRequired = false;
                }
                this.ViewState["ce709"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether Code Effects should alphabetically order fields and actions
        /// in menus or keep the order in which they are declared in the source object. The default value is False.
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(true), Localizable(true)]
        public bool KeepDeclaredOrder
        {
            get
            {
                return ((bool?)this.ViewState["ce713"]).GetValueOrDefault();
            }
            set
            {
                this.ViewState["ce713"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that defines what type of rules users can create.
        /// Value of RuleType.Evaluation limits Code Effects UI to evaluation type rules only.
        /// Value of RuleType.Execution permits both execution and evaluation types.
        /// The default value is CodeEffects.Rule.Common.RuleType.Evaluation.
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(RuleType.Evaluation), Localizable(true)]
        public RuleType Mode
        {
            get
            {
                if (this.ViewState["ce710"] != null)
                {
                    return (RuleType)this.ViewState["ce710"];
                }
                return RuleType.Evaluation;
            }
            set
            {
                if (value == RuleType.Filter)
                {
                    this.ShowToolBar = false;
                }
                this.ViewState["ce710"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of Code Effects UI theme. Set this property to ThemeType.None if you
        /// intend to use your own styles. The default value is CodeEffects.Rule.Common.ThemeType.Gray.
        /// </summary>
        [Bindable(true), Category("Style"), DefaultValue(ThemeType.Gray), Localizable(true)]
        public ThemeType Theme
        {
            get
            {
                if (this.ViewState["ce824"] != null)
                {
                    return (ThemeType)this.ViewState["ce824"];
                }
                return ThemeType.Gray;
            }
            set
            {
                this.ViewState["ce824"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the full path to the custom help XML file. HelpXmlFile property allows you to specify
        /// a Help XML that contains different values for Help String messages, Tool Bar
        /// labels and rule valiadation error messages. This could be helpful for multilingual applications.
        /// </summary>
        [Bindable(true), Category("Help"), Localizable(true)]
        public string HelpXmlFile
        {
            get
            {
                return (string)this.ViewState["ce601"];
            }
            set
            {
                this.ViewState["ce601"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that toggles the visibility of Help String. The default value is True.
        /// </summary>
        [Bindable(true), Category("Help"), DefaultValue(true), Localizable(true)]
        public bool ShowHelpString
        {
            get
            {
                return ((bool?)this.ViewState["ce604"]).GetValueOrDefault();
            }
            set
            {
                this.ViewState["ce604"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether Help XML should be cached
        /// by the server. The default value is False.
        /// </summary>
        [Bindable(true), Category("Help"), DefaultValue(false), Localizable(true)]
        public bool CacheHelp
        {
            get
            {
                return ((bool?)this.ViewState["ce603"]).GetValueOrDefault();
            }
            set
            {
                this.ViewState["ce603"] = value;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string AccessKey
        {
            get
            {
                return base.AccessKey;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string BackImageUrl
        {
            get
            {
                return base.BackImageUrl;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override BorderStyle BorderStyle
        {
            get
            {
                return base.BorderStyle;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override Unit BorderWidth
        {
            get
            {
                return base.BorderWidth;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override short TabIndex
        {
            get
            {
                return base.TabIndex;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override Unit Height
        {
            get
            {
                return base.Height;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string DefaultButton
        {
            get
            {
                return base.DefaultButton;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string GroupingText
        {
            get
            {
                return base.GroupingText;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToolTip
        {
            get
            {
                return base.ToolTip;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string CssClass
        {
            get
            {
                return "ceRule";
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override ControlCollection Controls
        {
            get
            {
                return base.Controls;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override FontInfo Font
        {
            get
            {
                return base.Font;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override ContentDirection Direction
        {
            get
            {
                return ContentDirection.NotSet;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override HorizontalAlign HorizontalAlign
        {
            get
            {
                return HorizontalAlign.NotSet;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override ScrollBars ScrollBars
        {
            get
            {
                return ScrollBars.None;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Enabled
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Wrap
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool EnableViewState
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool EnableTheming
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void DataBind()
        {
            throw new NotSupportedException("DataBind method is not supported in Code Effects control.");
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Focus()
        {
        }

        /// <summary>
        /// (Overriden by Code Effects)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Control FindControl(string id)
        {
            return null;
        }

        /// <summary>
        /// Clears the Rule Area and disposes all resources related to Code Effects
        /// </summary>
        public override void Dispose()
        {
            this.ClearCache();
            this.Clear();
            base.Dispose();
        }

        /// <summary>
        /// Returns text representation of the rule that is currently displayed in the Rule Area. Returns base.ToString() if no rules have been loaded and the Rule Area is empty.
        /// </summary>
        /// <returns>Text representation of the current rule</returns>
        public override string ToString()
        {
            return this.ToString(this.GetRuleDelegate, null);
        }

        /// <summary>
        /// Returns text representation of the rule that is currently displayed in the Rule Area. Returns base.ToString() if no rules have been loaded and the Rule Area is empty.
        /// </summary>
        /// <param name="ruleDelegate">Method that takes rule ID and returns rule XML string. Pass this parameter if you store each rule in a separate XML document.</param>
        /// <returns>Text representation of the current rule</returns>
        public string ToString(GetRuleDelegate ruleDelegate)
        {
            return this.ToString(ruleDelegate, null);
        }

        /// <summary>
        /// Returns text representation of the rule that is currently displayed in the Rule Area. Returns base.ToString() if no rules have been loaded and the Rule Area is empty.
        /// </summary>
        /// <param name="dataSources">Dictionary of data sources that might be used in the currently displayed rule</param>
        /// <returns>Text representation of the current rule</returns>
        public string ToString(Dictionary<string, GetDataSourceDelegate> dataSources)
        {
            return this.ToString(this.GetRuleDelegate, dataSources);
        }

        /// <summary>
        /// Returns text representation of the rule that is currently displayed in the Rule Area. Returns base.ToString() if no rules have been loaded and the Rule Area is empty.
        /// </summary>
        /// <param name="ruleDelegate">Method that takes rule ID and returns rule XML string. Pass this parameter if you store each rule in a separate XML document.</param>
        /// <param name="dataSources">Dictionary of data sources that might be used in the currently displayed rule</param>
        /// <returns>Text representation of the current rule</returns>
        public string ToString(GetRuleDelegate ruleDelegate, Dictionary<string, GetDataSourceDelegate> dataSources)
        {
            if (this.IsEmpty)
            {
                return base.ToString();
            }
            Labels labels = new Labels(this.LoadHelpXml(), this.ShowToolBar, this.Mode);
            return RuleLoader.GetString(this.rule.Elements, this.GetSourceXmlDocument(), labels, ruleDelegate, dataSources);
        }

        protected override void OnInit(EventArgs e)
        {
            if (base.DesignMode)
            {
                return;
            }
            base.OnInit(e);
            Literal literal = new Literal();
            literal.ID = "ce002" + this.ID;
            literal.EnableViewState = false;
            literal.Text = string.Format("<a href=\"http://codeeffects.com\" {0}=\"{1}\">*</a>", "ce002", this.IsDemo().ToString().ToLower());
            this.Controls.AddAt(0, literal);
            if (!this.ClientOnly)
            {
                this.hiddenField = new HiddenField();
                this.hiddenField.ID = "ce004_" + this.ID;
                this.hiddenField.EnableViewState = false;
                this.Controls.AddAt(0, this.hiddenField);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (base.DesignMode)
            {
                return;
            }
            base.OnLoad(e);
            Type type = base.GetType();
            ScriptManager current = ScriptManager.GetCurrent(this.Page);
            if (current != null)
            {
                try
                {
                    if (current.CompositeScript != null)
                    {
                        if (!current.CompositeScript.Scripts.Any((ScriptReference script) => script.Name == "CodeEffects.Rule.Resources.Scripts.Control.js"))
                        {
                            current.CompositeScript.Scripts.Add(new ScriptReference("CodeEffects.Rule.Resources.Scripts.Control.js", Assembly.GetExecutingAssembly().FullName));
                        }
                    }
                    else
                    {
                        current.Scripts.Add(new ScriptReference("CodeEffects.Rule.Resources.Scripts.Control.js", Assembly.GetExecutingAssembly().FullName));
                    }
                    goto IL_D8;
                }
                catch
                {
                    this.Page.ClientScript.RegisterClientScriptResource(type, "CodeEffects.Rule.Resources.Scripts.Control.js");
                    goto IL_D8;
                }
            }
            this.Page.ClientScript.RegisterClientScriptResource(type, "CodeEffects.Rule.Resources.Scripts.Control.js");
        IL_D8:
            if (!this.ClientOnly && !this.Page.ClientScript.IsOnSubmitStatementRegistered(type, "scrCodeEffectsControlOnSubmit" + this.ClientID))
            {
                this.Page.ClientScript.RegisterOnSubmitStatement(type, "scrCodeEffectsControlOnSubmit" + this.ClientID, string.Format("var ce{0} = $ce('{0}');if(ce{0})ce{0}.post();", this.ID));
            }
            if (this.Page.IsPostBack)
            {
                if (this.hiddenField == null)
                {
                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.MissingControl, new string[0]);
                }
                this.LoadClientData(this.hiddenField.Value);
                this.hiddenField.Value = string.Empty;
                if (this.Page.Request.Form["__EVENTTARGET"] == this.ClientID)
                {
                    if (this.IsValid)
                    {
                        string command;
                        if ((command = this.rule.Command) != null)
                        {
                            if (!(command == "ceSave"))
                            {
                                if (command == "ceDelete")
                                {
                                    if (string.IsNullOrWhiteSpace(this.rule.Id))
                                    {
                                        throw new EvaluationException(EvaluationException.ErrorIds.InvalidPostbackArgument, new string[0]);
                                    }
                                    if (this.DeleteRule != null)
                                    {
                                        this.DeleteRule(this, new RuleEventArgs(this.rule.Id, this.rule.IsLoadedRuleOfEvalType));
                                        return;
                                    }
                                    return;
                                }
                            }
                            else
                            {
                                if (this.IsEmpty)
                                {
                                    return;
                                }
                                if (string.IsNullOrWhiteSpace(this.rule.Id))
                                {
                                    this.rule.Id = Guid.NewGuid().ToString();
                                }
                                if (this.SaveRule != null)
                                {
                                    this.SaveRule(this, this.GetSaveEventArgs());
                                    return;
                                }
                                return;
                            }
                        }
                        try
                        {
                            this.rule.Id = this.rule.Command;
                        }
                        catch
                        {
                            throw new EvaluationException(EvaluationException.ErrorIds.InvalidPostbackArgument, new string[0]);
                        }
                        if (this.LoadRule != null)
                        {
                            this.LoadRule(this, new RuleEventArgs(this.rule.Id, this.rule.IsLoadedRuleOfEvalType));
                            return;
                        }
                    }
                }
                else if (this.ShowToolBar)
                {
                    this.rule.NameIsInvalid = false;
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (base.DesignMode)
            {
                return;
            }
            base.OnPreRender(e);
            if (this.Theme != ThemeType.None)
            {
                this.AddStyleLink(new ThemeManager(this.Theme));
            }
            Type type = this.Page.GetType();
            if (!this.Page.ClientScript.IsStartupScriptRegistered(typeof(Element).FullName))
            {
                this.Page.ClientScript.RegisterStartupScript(type, typeof(Element).FullName, MarkupManager.RenderInitials(), true);
            }
            Labels labels = null;
            if (this.ShowHelpString && !this.Page.ClientScript.IsStartupScriptRegistered(type, "ceStartupHelpMessages"))
            {
                labels = new Labels(this.LoadHelpXml(), this.ShowToolBar, this.Mode);
                this.Page.ClientScript.RegisterStartupScript(type, "ceStartupHelpMessages", MarkupManager.RenderHelp(labels.GetUiMessages()), true);
            }
            if (!this.Page.ClientScript.IsStartupScriptRegistered(type, "ceStartupErrorMessages"))
            {
                if (labels == null)
                {
                    labels = new Labels(this.LoadHelpXml(), this.ShowToolBar, this.Mode);
                }
                this.Page.ClientScript.RegisterStartupScript(type, "ceStartupErrorMessages", MarkupManager.RenderErrors(labels.GetErrorMessages()), true);
            }
            if (!this.Page.ClientScript.IsStartupScriptRegistered(type, "scrCodeEffectsControlStartUp" + this.ClientID))
            {
                string text = MarkupManager.RenderControlInstance(this.ID, this.ClientID, this.ClientOnly ? null : this.hiddenField.ClientID, this.Page.Request.Browser.Platform.ToLower() == "winxp", true);
                if (!this.ClientOnly)
                {
                    text += MarkupManager.RenderSettings(this, this.GetConditions(), true);
                }
                this.Page.ClientScript.RegisterStartupScript(type, "scrCodeEffectsControlStartUp" + this.ClientID, text, true);
            }
            StringBuilder stringBuilder = new StringBuilder();
            if (!this.ClientOnly && !this.IsEmpty)
            {
                RuleValidator.ForceTokens(this.rule.Elements);
                stringBuilder.Append(MarkupManager.RenderRuleDataForLoading(this.rule, this.ID));
                if (!this.IsValid)
                {
                    stringBuilder.Append(MarkupManager.RenderInvalidDataForLoading(this.rule, this.ID));
                }
            }
            if (stringBuilder.Length > 0)
            {
                this.Page.ClientScript.RegisterStartupScript(type, "scrCodeEffectsControlLoadInvalidStartUp" + this.ClientID, stringBuilder.ToString(), true);
            }
            this.Clear();
        }

        /// <summary>
        /// Returns XML document of the source objcet
        /// </summary>
        /// <returns>Xml document as string</returns>
        public string GetSourceXml()
        {
            return this.GetSourceXmlDocument().OuterXml;
        }

        /// <summary>
        /// Returns content of the default help XML document
        /// </summary>
        /// <returns>String representation of the default help XML</returns>
        public string GetHelpXml()
        {
            return this.LoadHelpXml().OuterXml;
        }

        /// <summary>
        /// Returns string representation of Rule XML. Calling this method is the only way to obtain Rule XML in Code Effects.
        /// </summary>
        /// <returns>Xml document as string</returns>
        public string GetRuleXml()
        {
            return this.GetRuleXml(RuleFormatType.CodeEffects);
        }

        /// <summary>
        /// Returns JSON string that contains Code Effects UI settings.
        /// </summary>
        /// <returns>Json-formatted UI settings</returns>
        public string GetClientSettings()
        {
            return MarkupManager.RenderSettings(this, this.GetConditions(), false);
        }

        /// <summary>
        /// Returns rule's client data. Should be used by the "client-only" RuleEditor instances.
        /// </summary>
        /// <returns>Json-formatted rule data</returns>
        public string GetClientRuleData()
        {
            if (this.IsEmpty || !this.IsValid)
            {
                return null;
            }
            return MarkupManager.RenderRuleClientData(this.rule);
        }

        /// <summary>
        /// Returns JSON string that contains invalid rule members.
        /// </summary>
        /// <returns>Json-formatted array of invalid elements</returns>
        public string GetClientInvalidData()
        {
            if (this.IsValid)
            {
                return null;
            }
            return MarkupManager.RenderRuleInvalidClientData(this.rule);
        }

        /// <summary>
        /// Returns save-related data
        /// </summary>
        /// <returns>Instance of the SaveEventArgs type</returns>
        public SaveEventArgs GetSaveArguments()
        {
            if (string.IsNullOrWhiteSpace(this.rule.Id))
            {
                this.rule.Id = Guid.NewGuid().ToString();
            }
            return this.GetSaveEventArgs();
        }

        /// <summary>
        /// Loads rule XML file into the RuleEditor
        /// </summary>
        /// <param name="ruleXmlFileFullPath">Full path to the rule XML file</param>
        public void LoadRuleFile(string ruleXmlFileFullPath)
        {
            this.LoadRuleFile(ruleXmlFileFullPath, this.GetRuleDelegate);
        }

        /// <summary>
        /// Loads rule XML file into the RuleEditor
        /// </summary>
        /// <param name="ruleXmlFileFullPath">Full path to the rule XML file</param>
        /// <param name="ruleDelegate">Method that takes rule ID and returns rule XML string. Pass this parameter if you store each rule in a separate XML document.</param>
        public void LoadRuleFile(string ruleXmlFileFullPath, GetRuleDelegate ruleDelegate)
        {
            if (!File.Exists(ruleXmlFileFullPath))
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.XMLFileNotFound, new string[0]);
            }
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(ruleXmlFileFullPath);
            }
            catch
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.XMLFileIsMalformed, new string[0]);
            }
            this.LoadRuleXml(xmlDocument.OuterXml, ruleDelegate);
        }

        /// <summary>
        /// Loads rule XML into the RuleEditor
        /// </summary>
        /// <param name="ruleXml">String representation of the rule XML document</param>
        public void LoadRuleXml(string ruleXml)
        {
            this.LoadRuleXml(ruleXml, this.GetRuleDelegate);
        }

        /// <summary>
        /// Loads rule XML into the RuleEditor
        /// </summary>
        /// <param name="ruleXml">String representation of the rule XML document</param>
        /// <param name="ruleDelegate">Method that takes rule ID and returns rule XML string. Pass this parameter if you store each rule in a separate XML document.</param>
        public void LoadRuleXml(string ruleXml, GetRuleDelegate ruleDelegate)
        {
            XmlDocument sourceXmlDocument = this.GetSourceXmlDocument();
            this.rule = RuleLoader.LoadXml(ruleXml, sourceXmlDocument, ruleDelegate);
            if (this.rule.Format == RuleFormatType.CodeEffects)
            {
                this.rule.IsLoadedRuleOfEvalType = new bool?(!RuleValidator.HasActions(this.rule.Elements));
                this.ValidateRule(this.rule.Elements);
                return;
            }
            throw new NotSupportedException("Old XML formats are not supported by this version of Code Effects control.");
        }

        /// <summary>
        /// Loads rule's client data into the RuleEditor.
        /// </summary>
        /// <param name="ruleClientData">Json-formatted rule data</param>
        public void LoadClientData(string ruleClientData)
        {
            if (string.IsNullOrEmpty(ruleClientData))
            {
                return;
            }
            this.rule = new JavaScriptSerializer().Deserialize<RuleModel>(ruleClientData);
            if (this.ShowToolBar)
            {
                if (this.rule.Name != null)
                {
                    this.rule.Name = ESPL.Rule.Core.Encoder.Desanitize(this.rule.Name);
                }
                if (this.rule.Desc != null)
                {
                    this.rule.Desc = ESPL.Rule.Core.Encoder.Desanitize(this.rule.Desc);
                }
            }
            if (this.rule.Elements.Count == 0)
            {
                return;
            }
            RuleValidator.EnsureTokens(this.rule.Elements, this.GetSourceXmlDocument());
            if (string.IsNullOrWhiteSpace(this.rule.Command) || this.rule.Command == "ceExtract" || this.rule.Command == "ceSave")
            {
                this.ValidateRule(this.rule.Elements);
                if (this.IsValid)
                {
                    RuleValidator.EnsureValues(this.rule.Elements);
                }
            }
        }

        /// <summary>
        /// Clears from server cache all items stored there by Code Effects. Items amy include source object XML and help XML.
        /// </summary>
        public void ClearCache()
        {
            if (HttpContext.Current != null)
            {
                string name = "appCodeEffectsRuleSourceXmlPath" + this.ID;
                string name2 = "appCodeEffectsRuleHelpXmlPath" + this.ID;
                HttpContext.Current.Application[name] = null;
                HttpContext.Current.Application[name2] = null;
            }
        }

        /// <summary>
        /// Removes all rule elements from the Rule Area
        /// </summary>
        public void Clear()
        {
            this.rule = new RuleModel();
        }

        private string GetRuleXml(RuleFormatType format)
        {
            if (this.IsEmpty)
            {
                return null;
            }
            if (!this.IsValid)
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleIsInvalid, new string[0]);
            }
            List<Element> items = this.CopyElements(this.rule.Elements);
            XmlDocument sourceXmlDocument = this.GetSourceXmlDocument();
            string ruleName = this.ShowToolBar ? this.rule.Name : null;
            string ruleDescription = this.ShowToolBar ? this.rule.Desc : null;
            RuleValidator.EnsureIgnoredProperties(items, sourceXmlDocument);
            RuleValidator.EnsureValues(items);
            return RuleLoader.GetXml(items, string.IsNullOrWhiteSpace(this.rule.Id) ? Guid.NewGuid().ToString() : this.rule.Id, ruleName, ruleDescription, sourceXmlDocument, format, this.Mode, (!this.rule.IsLoadedRuleOfEvalType.HasValue) ? (this.Mode == RuleType.Evaluation || this.Mode == RuleType.Filter) : this.rule.IsLoadedRuleOfEvalType.Value);
        }

        /// <summary>
        /// Indicates if control is in "demo" mode. Returning True means "demo", false - not
        /// </summary>
        /// <returns>True if in "demo" mode, False otherwise.</returns>
        private bool IsDemo()
        {
            return Vector.IsDemo(this.Page.Request.Url.Host, this.Page.Request.ServerVariables["SERVER_NAME"], this.Page.Request.IsLocal);
        }

        private List<Element> CopyElements(List<Element> elements)
        {
            return (from el in elements
                    where el.FuncType != FunctionType.Comma
                    select el.Clone()).ToList<Element>();
        }

        private MarkupData GetConditions()
        {
            MarkupData markupData = new MarkupData();
            markupData.Pattern = Pattern.Asp;
            markupData.ControlServerID = this.ID;
            if (!this.ClientOnly)
            {
                markupData.PostBackFunctionName = this.Page.ClientScript.GetPostBackEventReference(this, string.Empty);
            }
            if (!this.rule.IsLoadedRuleOfEvalType.HasValue)
            {
                markupData.IsLoadedRuleOfEvalType = (this.Mode == RuleType.Evaluation || this.Mode == RuleType.Filter);
            }
            else
            {
                markupData.IsLoadedRuleOfEvalType = (this.rule.IsLoadedRuleOfEvalType == true);
            }
            markupData.Mode = this.Mode;
            if (this.Theme != ThemeType.None)
            {
                markupData.ThemeFactory = new ThemeManager(this.Theme);
            }
            Settings settings = new Settings(this.LoadHelpXml(), this.ToolBarRules, this.ShowToolBar, this.Mode);
            settings.Load(this, this.ContextMenuRules, this.GetSourceXmlDocument(), this.DataSources);
            markupData.Settings = settings;
            return markupData;
        }

        private XmlDocument GetSourceXmlDocument()
        {
            if (this.SourceXml != null)
            {
                return this.SourceXml;
            }
            string text;
            if (!(this.Source != null) && string.IsNullOrEmpty(this.SourceType))
            {
                if (!string.IsNullOrEmpty(this.SourceXmlFile))
                {
                    text = "appCodeEffectsRuleSourceXmlPath" + this.SourceXmlFile;
                    try
                    {
                        return this.GetCachedXmlFile(text, this.SourceXmlFile, this.CacheSource);
                    }
                    catch
                    {
                        throw new MalformedXmlException(MalformedXmlException.ErrorIds.MalformedOrMissingSourceXML, new string[0]);
                    }
                }
                throw new SourceException("s100", new string[0]);
            }
            text = "appCodeEffectsRuleSourceXmlPath" + ((this.Source == null) ? this.SourceType : this.Source.FullName);
            if (HttpContext.Current != null && HttpContext.Current.Application[text] != null)
            {
                return (XmlDocument)HttpContext.Current.Application[text];
            }
            List<Type> processedTypes = new List<Type>();
            XmlDocument xml;
            if (this.Source != null)
            {
                xml = SourceLoader.GetXml(this.Source, processedTypes);
            }
            else
            {
                xml = SourceLoader.GetXml(this.SourceAssembly, this.SourceType, processedTypes);
            }
            if (HttpContext.Current != null && this.CacheSource)
            {
                HttpContext.Current.Application[text] = xml;
            }
            return xml;
        }

        private XmlDocument LoadHelpXml()
        {
            if (this.HelpXml != null)
            {
                return this.HelpXml;
            }
            if (string.IsNullOrEmpty(this.HelpXmlFile))
            {
                XmlDocument xmlDocument = new XmlDocument();
                var assembly = Assembly.GetExecutingAssembly();
                string strFileName = "CodeEffects.Rule.Resource.Scripts.Control.js";
                var stream = assembly.GetManifestResourceStream(this.GetType(), strFileName);
                throw new NotImplementedException();//TODO string xml = (this.Mode == RuleType.Filter) ? Resource.FilterHelp : Resource.RuleHelp;
                //xmlDocument.LoadXml(xml);
                return xmlDocument;
            }
            XmlDocument cachedXmlFile;
            try
            {
                cachedXmlFile = this.GetCachedXmlFile("appCodeEffectsRuleHelpXmlPath" + this.ID, this.HelpXmlFile, this.CacheHelp);
            }
            catch (Exception ex)
            {
                throw new MalformedXmlException(MalformedXmlException.ErrorIds.MalformedOrMissingHelpXML, new string[]
				{
					ex.Message
				});
            }
            return cachedXmlFile;
        }

        private SaveEventArgs GetSaveEventArgs()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(this.GetRuleXml());
            XmlNode firstChild = xmlDocument.DocumentElement.FirstChild;
            List<string> list = (from el in this.rule.Elements
                                 where el.Type == ElementType.Field && el.IsRule
                                 select el.Value).Distinct<string>().ToList<string>();
            return new SaveEventArgs
            {
                IsEvaluationTypeRule = this.rule.IsLoadedRuleOfEvalType.Value,
                RuleID = this.rule.Id,
                RuleXmlAsDocument = xmlDocument.OuterXml,
                RuleXmlAsNode = firstChild.OuterXml,
                RuleDescription = this.rule.Desc,
                RuleName = this.rule.Name,
                ReferencedRules = ((list.Count > 0) ? list : null)
            };
        }

        /// <summary>
        /// Returns a cached version of XML if one is available. 
        /// Otherwise, it reads one from a file and stores it in the cache if storeInCache is set to True.
        /// </summary>
        /// <param name="cacheKey">A application cache key for an XML.</param>
        /// <param name="xmlPath">Full path to an XML file.</param>
        /// <param name="storeInCache">If set, the XML will be stored in the application cache under cacheKey</param>
        /// <returns>An XMLDocument.</returns>
        private XmlDocument GetCachedXmlFile(string cacheKey, string xmlPath, bool storeInCache)
        {
            if (HttpContext.Current == null || HttpContext.Current.Application[cacheKey] == null)
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);
                if (storeInCache && HttpContext.Current != null)
                {
                    HttpContext.Current.Application[cacheKey] = xmlDocument;
                }
                return xmlDocument;
            }
            return (XmlDocument)HttpContext.Current.Application[cacheKey];
        }

        private void AddStyleLink(ThemeManager themeManager)
        {
            string styleTagID = themeManager.StyleTagID;
            Literal literal = new Literal();
            literal.ID = styleTagID;
            literal.EnableViewState = false;
            literal.Text = string.Format("<link {0}=\"true\" rel=\"stylesheet\" type=\"text/css\"{1}></link>", themeManager.StyleTagAttribute, this.ClientOnly ? string.Empty : (" href=\"" + themeManager.GetLinkUrl() + "\""));
            if (this.Page.Header != null)
            {
                if (this.Page.Header.FindControl(styleTagID) == null)
                {
                    this.Page.Header.Controls.Add(literal);
                    return;
                }
            }
            else if (this.Page.FindControl(styleTagID) == null)
            {
                this.Controls.Add(literal);
            }
        }

        private void ResetValidation()
        {
            this.rule.InvalidElements = new List<InvalidElement>();
            this.rule.NameIsInvalid = false;
        }

        private void ValidateRule(List<Element> elements)
        {
            XmlDocument help = this.LoadHelpXml();
            List<InvalidElement> list = RuleValidator.Validate(help, elements, this.GetSourceXmlDocument(), this.rule.IsLoadedRuleOfEvalType.GetValueOrDefault());
            if (list.Count > 0)
            {
                this.rule.InvalidElements = list;
            }
            else if (this.GetRuleDelegate != null)
            {
                list = RuleValidator.CheckRecursion(elements, this.GetRuleXml(), help, this.GetRuleDelegate);
            }
            if (list.Count > 0)
            {
                this.rule.InvalidElements = list;
            }
            else if (this.ShowToolBar && this.RuleNameIsRequired && string.IsNullOrWhiteSpace(this.rule.Name))
            {
                this.rule.NameIsInvalid = true;
            }
            else
            {
                this.rule.InvalidElements = new List<InvalidElement>();
                this.rule.NameIsInvalid = false;
            }
            if (string.IsNullOrWhiteSpace(this.rule.Desc))
            {
                this.rule.Desc = null;
            }
        }

        /// <summary>
        /// Empty public c-tor.
        /// </summary>
        public RuleEditor()
        {
            this.rule = new RuleModel();
            this.Mode = RuleType.Evaluation;
            this.ClientOnly = false;
            this.CacheHelp = (this.CacheSource = (this.ShowLineDots = false));
            this.RuleNameIsRequired = true;
            this.ShowToolBar = (this.Mode != RuleType.Filter);
            this.ShowMenuOnRightArrowKey = (this.ShowMenuOnElementClicked = (this.ShowDescriptionsOnMouseHover = (this.ShowHelpString = true)));
            this.Theme = ThemeType.Gray;
            this.ExcludedOperators = new List<Operator>();
        }
    }
}
