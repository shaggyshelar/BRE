using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;

namespace ESPL.Rule.MVC
{
    /// <summary>
    /// Provides a wrapper for the RuleEditor class
    /// </summary>
    public class RuleEditorBuilder
    {
        private ViewContext viewContext;

        /// <summary>
        /// Gets or sets an instance of the RuleEditor class
        /// </summary>
        public RuleEditor Editor
        {
            get;
            set;
        }

        /// <summary>
        /// Public constructor of the RuleEditorBuilder class
        /// </summary>
        public RuleEditorBuilder(ViewContext viewContext)
        {
            this.viewContext = viewContext;
            this.Editor = new RuleEditor(viewContext);
        }

        /// <summary>
        /// Sets the value of the server ID of the RuleEditor class. Used to distinguish this instance of
        /// the RuleEditor from all other instances that may be declared on the same view.
        /// </summary>
        /// <param name="id">Server ID of this instance of the RuleEditor</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder Id(string id)
        {
            this.Editor.Id = id;
            return this;
        }

        /// <summary>
        /// Sets the value of the SaveAction property of the underlying instance of RuleEditor class. This property is
        /// ignored if ShowToolBar is set to False. 
        /// </summary>
        /// <param name="saveAction">Name of the Save action</param>
        /// <param name="saveController">Name of the controller</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder SaveAction(string saveAction, string saveController)
        {
            this.Editor.SaveController = saveController;
            this.Editor.SaveAction = saveAction;
            return this;
        }

        /// <summary>
        /// Sets the value of the DeleteAction property of the underlying instance of RuleEditor class. This property is
        /// ignored if ShowToolBar is set to False. 
        /// </summary>
        /// <param name="deleteAction">Name of the Delete action</param>
        /// <param name="deleteController">Name of the controller</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder DeleteAction(string deleteAction, string deleteController)
        {
            this.Editor.DeleteController = deleteController;
            this.Editor.DeleteAction = deleteAction;
            return this;
        }

        /// <summary>
        /// Sets the value of the LoadAction property of the underlying instance of RuleEditor class. This property is
        /// ignored if ShowToolBar is set to False. 
        /// </summary>
        /// <param name="loadAction">Name of the Load action</param>
        /// <param name="loadController">Name of the controller</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder LoadAction(string loadAction, string loadController)
        {
            this.Editor.LoadController = loadController;
            this.Editor.LoadAction = loadAction;
            return this;
        }

        /// <summary>
        /// Sets the value of the ToolBarRules property of the underlying instance of RuleEditor class. The rules
        /// from this collection appear in the Rules menu of the Tool Bar. The collection can contain
        /// rules of all types. This property is ignored if ShowToolBar is set to False. 
        /// </summary>
        /// <param name="toolBarRules">List of rules</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder ToolBarRules(List<MenuItem> toolBarRules)
        {
            this.Editor.ToolBarRules = toolBarRules;
            return this;
        }

        /// <summary>
        /// Sets the value of the ContextMenuRules property of the underlying instance of RuleEditor class. The rules
        /// from this collection appear in the context menus of Code Effects component as regular fields. Make sure
        /// that only rules of evaluation type are added to this list. This property is ignored if
        /// ShowToolBar is set to False. 
        /// </summary>
        /// <param name="contextMenuRules">List of rules</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder ContextMenuRules(List<MenuItem> contextMenuRules)
        {
            this.Editor.ContextMenuRules = contextMenuRules;
            return this;
        }

        /// <summary>
        /// Sets the value of the HelpXmlFile property of the underlying instance of RuleEditor class. HelpXmlFile
        /// property allows you to specify a Help XML file that contains different values for Help String messages, Tool Bar
        /// labels and validation error messages. This could be helpful for multilingual applications.
        /// </summary>
        /// <param name="helpXmlFilePath">Virtual path to the custom Help XML file</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder Help(string helpXmlFilePath)
        {
            this.Editor.HelpXmlFile = helpXmlFilePath;
            return this;
        }

        /// <summary>
        /// Sets the value of the HelpXml property of the underlying instance of RuleEditor class. HelpXml
        /// property allows you to specify a Help XML that contains different values for Help String messages, Tool Bar
        /// labels and validation error messages. This could be helpful for multilingual applications.
        /// </summary>
        /// <param name="helpXml">Xml document object of the custom help</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder Help(XmlDocument helpXml)
        {
            this.Editor.HelpXml = helpXml;
            return this;
        }

        /// <summary>
        /// Sets the value of the Rule property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="rule">Instance of the rule model</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder Rule(RuleModel rule)
        {
            this.Editor.Rule = rule;
            return this;
        }

        /// <summary>
        /// Sets the collection of excluded operators
        /// </summary>
        /// <param name="excludedOperators">Excluded operators</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder ExcludedOperators(ICollection<Operator> excludedOperators)
        {
            this.Editor.ExcludedOperators = excludedOperators;
            return this;
        }

        /// <summary>
        /// Sets the collection of named Dynamic Menu Data Sources
        /// </summary>
        /// <param name="dataSources">Data sources</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder DataSources(ICollection<DataSourceHolder> dataSources)
        {
            this.Editor.DataSources = dataSources;
            return this;
        }

        /// <summary>
        /// Sets the value of the Theme property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="theme">One of the members of ThemeType enumerator</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder Theme(ThemeType theme)
        {
            this.Editor.Theme = theme;
            return this;
        }

        /// <summary>
        /// Sets the value of the Mode property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="mode">One of the members of RuleType enumerator. Value of RuleType.Evaluation
        /// limits Code Effects' UI to evaluation type rules only. Value of RuleType.Execution permits both
        /// execution and evaluation types.</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder Mode(RuleType mode)
        {
            this.Editor.Mode = mode;
            return this;
        }

        /// <summary>
        /// Sets the value of the ShowHelpString property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="showHelpString">Value of False instructs Code Effects component not to display the Help String.</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder ShowHelpString(bool showHelpString)
        {
            this.Editor.ShowHelpString = showHelpString;
            return this;
        }

        /// <summary>
        /// Sets the value of the ShowLineDots property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="showLineDots">Value of True instructs Code Effects component to display "line dots" at the beginning of
        /// each new line in the Rule Area</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder ShowLineDots(bool showLineDots)
        {
            this.Editor.ShowLineDots = showLineDots;
            return this;
        }

        /// <summary>
        /// Sets the value of the ShowMenuOnRightArrowKey property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="showMenuOnRightArrowKey">Value of False instructs Code Effects component not to display context menus when user presses the
        /// "right arrow" key while navigating the rule</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder ShowMenuOnRightArrowKey(bool showMenuOnRightArrowKey)
        {
            this.Editor.ShowMenuOnRightArrowKey = showMenuOnRightArrowKey;
            return this;
        }

        /// <summary>
        /// Sets the value of the ShowMenuOnElementClicked property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="showMenuOnRightArrowKey">Value of False instructs Code Effects component not to display context menus when user clicks rule elements</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder ShowMenuOnElementClicked(bool showMenuOnElementClicked)
        {
            this.Editor.ShowMenuOnElementClicked = showMenuOnElementClicked;
            return this;
        }

        /// <summary>
        /// Sets the value of the ShowDescriptionsOnMouseHover property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="showMenuOnRightArrowKey">Value of False instructs Code Effects component not to display element descriptions</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder ShowDescriptionsOnMouseHover(bool showDescriptionsOnMouseHover)
        {
            this.Editor.ShowDescriptionsOnMouseHover = showDescriptionsOnMouseHover;
            return this;
        }

        /// <summary>
        /// Sets the value of the ShowToolBar property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="showToolBar">Value of False instructs Code Effects component not to display the Tool Bar.</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder ShowToolBar(bool showToolBar)
        {
            this.Editor.ShowToolBar = showToolBar;
            return this;
        }

        /// <summary>
        /// Sets the value of the ClientOnly property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="clientOnly">Value of True sets Code Effects' UI to "client-only" mode</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder ClientOnly(bool clientOnly)
        {
            this.Editor.ClientOnly = clientOnly;
            return this;
        }

        /// <summary>
        /// Sets the value of the KeepDeclaredOrder property of the underlying instance of RuleEditor class.
        /// </summary>
        /// <param name="clientOnly">Value of True instructs Code Effects component to keep the order of fields in the menu the way they are declared in the source object</param>
        /// <returns>Instance of the RuleEditorBuilder type</returns>
        public RuleEditorBuilder KeepDeclaredOrder(bool keepDeclaredOrder)
        {
            this.Editor.KeepDeclaredOrder = keepDeclaredOrder;
            return this;
        }

        /// <summary>
        /// Renders Code Effects component
        /// </summary>
        public void Render()
        {
            this.Editor.Render();
        }
    }
}
