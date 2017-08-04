using ESPL.Rule.Common;
using ESPL.Rule.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Client
{
    internal sealed class Labels
    {
        internal enum Scope
        {
            SectionBegin,
            SectionEnd,
            CalculationBegin,
            CalculationEnd,
            CollectionBegin,
            CollectionEnd
        }

        private bool includeToolBarLabels;

        private RuleType mode;

        private XmlDocument help;

        internal string Calculation
        {
            get;
            private set;
        }

        internal string Setter
        {
            get;
            private set;
        }

        internal string StringInput
        {
            get;
            private set;
        }

        internal string NumericInput
        {
            get;
            private set;
        }

        internal string BoolInput
        {
            get;
            private set;
        }

        internal string EnumInput
        {
            get;
            private set;
        }

        internal string DateInput
        {
            get;
            private set;
        }

        internal string TimeInput
        {
            get;
            private set;
        }

        internal string Name
        {
            get;
            private set;
        }

        internal string Description
        {
            get;
            private set;
        }

        internal string EmptyRuleArea
        {
            get;
            private set;
        }

        internal string MenuButton
        {
            get;
            private set;
        }

        internal string DeleteButton
        {
            get;
            private set;
        }

        internal string SaveButton
        {
            get;
            private set;
        }

        internal string NewEvaluationRule
        {
            get;
            private set;
        }

        internal string NewExecutionRule
        {
            get;
            private set;
        }

        internal string True
        {
            get;
            private set;
        }

        internal string False
        {
            get;
            private set;
        }

        internal string EvaluationIf
        {
            get;
            private set;
        }

        internal string ExecutionIf
        {
            get;
            private set;
        }

        internal string Exists
        {
            get;
            private set;
        }

        internal string DoesNotExist
        {
            get;
            private set;
        }

        internal string SectionBegin
        {
            get;
            private set;
        }

        internal string SectionEnd
        {
            get;
            private set;
        }

        internal string CalculationBegin
        {
            get;
            private set;
        }

        internal string CalculationEnd
        {
            get;
            private set;
        }

        internal string CollectionBegin
        {
            get;
            private set;
        }

        internal string CollectionEnd
        {
            get;
            private set;
        }

        public Labels(XmlDocument help, bool includeToolBarLabels, RuleType mode)
        {
            this.includeToolBarLabels = includeToolBarLabels;
            this.mode = mode;
            XmlNode xmlNode = help.SelectSingleNode("/codeeffects/flow");
            if (xmlNode.SelectSingleNode("evaluationIf") == null)
            {
                this.EvaluationIf = (this.ExecutionIf = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("if").InnerText));
            }
            else
            {
                switch (this.mode)
                {
                    case RuleType.Execution:
                        this.EvaluationIf = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("evaluationIf").InnerText);
                        this.ExecutionIf = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("executionIf").InnerText);
                        goto IL_12B;
                    case RuleType.Loop:
                        this.EvaluationIf = string.Empty;
                        this.ExecutionIf = this.CheckForNull(xmlNode.SelectSingleNode("loopIf"), "While");
                        goto IL_12B;
                    case RuleType.Ruleset:
                        this.EvaluationIf = string.Empty;
                        this.ExecutionIf = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("executionIf").InnerText);
                        goto IL_12B;
                }
                this.EvaluationIf = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("evaluationIf").InnerText);
                this.ExecutionIf = string.Empty;
            }
        IL_12B:
            xmlNode = help.SelectSingleNode("/codeeffects/scopes");
            if (xmlNode == null)
            {
                this.CollectionBegin = (this.SectionBegin = "&#40;");
                this.CollectionEnd = (this.SectionEnd = "&#41;");
                this.CalculationBegin = "&#123;";
                this.CalculationEnd = "&#125;";
            }
            else
            {
                this.SectionBegin = this.CheckForNull(xmlNode.SelectSingleNode("sectionBegin"), "&#40;");
                this.SectionEnd = this.CheckForNull(xmlNode.SelectSingleNode("sectionEnd"), "&#41;");
                this.CalculationBegin = this.CheckForNull(xmlNode.SelectSingleNode("calculationBegin"), "&#123;");
                this.CalculationEnd = this.CheckForNull(xmlNode.SelectSingleNode("calculationEnd"), "&#125;");
                this.CollectionBegin = this.CheckForNull(xmlNode.SelectSingleNode("collectionBegin"), "&#40;");
                this.CollectionEnd = this.CheckForNull(xmlNode.SelectSingleNode("collectionEnd"), "&#41;");
            }
            xmlNode = help.SelectSingleNode("/codeeffects/labels");
            this.Calculation = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("calculationMenuItem").InnerText);
            this.StringInput = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("stringInput").InnerText);
            this.BoolInput = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("boolInput").InnerText);
            this.EnumInput = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("enumInput").InnerText);
            this.NumericInput = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("numericInput").InnerText);
            this.DateInput = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("dateInput").InnerText);
            this.TimeInput = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("timeInput").InnerText);
            this.EmptyRuleArea = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("emptyRuleArea").InnerText);
            this.True = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("trueValueName").InnerText);
            this.False = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("falseValueName").InnerText);
            this.Exists = this.CheckForNull(xmlNode.SelectSingleNode("existsMenuItem"), "Exists...");
            this.DoesNotExist = this.CheckForNull(xmlNode.SelectSingleNode("doesNotExistMenuItem"), "Exists...");
            if (this.includeToolBarLabels)
            {
                this.Name = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("ruleDefaultName").InnerText);
                this.Description = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("ruleDefaultDescription").InnerText);
                this.MenuButton = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("menuButton").InnerText);
                this.SaveButton = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("saveButton").InnerText);
                this.DeleteButton = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("deleteButton").InnerText);
                switch (this.mode)
                {
                    case RuleType.Execution:
                        this.NewEvaluationRule = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("newEvaluationRule").InnerText);
                        this.NewExecutionRule = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("newExecutionRule").InnerText);
                        goto IL_4A0;
                    case RuleType.Loop:
                    case RuleType.Ruleset:
                        this.NewExecutionRule = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("newRule").InnerText);
                        goto IL_4A0;
                }
                this.NewEvaluationRule = ESPL.Rule.Core.Encoder.Sanitize(xmlNode.SelectSingleNode("newRule").InnerText);
            }
        IL_4A0:
            switch (this.mode)
            {
                case RuleType.Execution:
                case RuleType.Loop:
                case RuleType.Ruleset:
                    this.Setter = this.CheckForNull(xmlNode.SelectSingleNode("setMenuItem"), "Set...");
                    break;
            }
            this.help = help;
        }

        public List<Pair> GetErrorMessages()
        {
            List<Pair> list = new List<Pair>();
            foreach (XmlNode xmlNode in this.help.SelectSingleNode("/codeeffects/errors").ChildNodes)
            {
                if (xmlNode.NodeType != XmlNodeType.Comment && (xmlNode.Attributes["toolbar"] == null || (this.includeToolBarLabels && xmlNode.Attributes["toolbar"].Value == "true")))
                {
                    list.Add(new Pair(xmlNode.Name, xmlNode.InnerText));
                }
            }
            return list;
        }

        public List<Pair> GetUiMessages()
        {
            List<Pair> list = new List<Pair>();
            foreach (XmlNode xmlNode in this.help.SelectSingleNode("/codeeffects/help").ChildNodes)
            {
                if (xmlNode.NodeType != XmlNodeType.Comment)
                {
                    list.Add(new Pair(xmlNode.Name, xmlNode.InnerText));
                }
            }
            return list;
        }

        internal string GetFlowLabel(string value, bool isEvaluationRule)
        {
            string text = value;
            if (text == "if" || (this.mode == RuleType.Ruleset && text == "elseIf"))
            {
                switch (this.mode)
                {
                    case RuleType.Execution:
                    case RuleType.Ruleset:
                        text = (isEvaluationRule ? "evaluationIf" : "executionIf");
                        goto IL_68;
                    case RuleType.Loop:
                        text = "loopIf";
                        goto IL_68;
                }
                text = "evaluationIf";
            }
        IL_68:
            return this.help.SelectSingleNode(string.Format("{0}/{1}", "/codeeffects/flow", text)).InnerText;
        }

        internal string GetExistsLabel(SelectionType type)
        {
            string arg = (type == SelectionType.DoesNotExist) ? "doesNotExist" : "exists";
            return this.help.SelectSingleNode(string.Format("{0}/{1}", "/codeeffects/collections", arg)).InnerText;
        }

        internal string GetWhereLabel()
        {
            return this.help.SelectSingleNode(string.Format("{0}/{1}", "/codeeffects/collections", "where")).InnerText;
        }

        internal string GetClauseLabel(string value)
        {
            return this.help.SelectSingleNode(string.Format("{0}/{1}", "/codeeffects/clauses", value)).InnerText;
        }

        internal string GetSetterLabel(string value)
        {
            return this.help.SelectSingleNode(string.Format("{0}/{1}", "/codeeffects/setters", value)).InnerText;
        }

        internal string GetScopeLabel(Labels.Scope scope)
        {
            string arg = "collectionBegin";
            switch (scope)
            {
                case Labels.Scope.SectionBegin:
                    arg = "sectionBegin";
                    break;
                case Labels.Scope.SectionEnd:
                    arg = "sectionEnd";
                    break;
                case Labels.Scope.CalculationBegin:
                    arg = "calculationBegin";
                    break;
                case Labels.Scope.CalculationEnd:
                    arg = "calculationEnd";
                    break;
                case Labels.Scope.CollectionEnd:
                    arg = "collectionEnd";
                    break;
            }
            return this.help.SelectSingleNode(string.Format("{0}/{1}", "/codeeffects/scopes", arg)).InnerText;
        }

        internal string GetOperatorLabel(string value, OperatorType type)
        {
            return this.help.SelectSingleNode(string.Format("{0}/{1}/{2}", "/codeeffects/operators", Converter.ClientTypeToClientString(type), value)).InnerText;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("c:\"").Append(this.Calculation);
            stringBuilder.Append("\",s:\"").Append(this.StringInput);
            stringBuilder.Append("\",b:\"").Append(this.BoolInput);
            stringBuilder.Append("\",e:\"").Append(this.EnumInput);
            stringBuilder.Append("\",m:\"").Append(this.NumericInput);
            stringBuilder.Append("\",v:\"").Append(this.DateInput);
            stringBuilder.Append("\",j:\"").Append(this.TimeInput);
            stringBuilder.Append("\",g:\"").Append(this.EmptyRuleArea);
            stringBuilder.Append("\",h:\"").Append(this.Exists);
            stringBuilder.Append("\",l:\"").Append(this.DoesNotExist);
            stringBuilder.Append("\",pb:\"").Append(this.SectionBegin);
            stringBuilder.Append("\",pe:\"").Append(this.SectionEnd);
            stringBuilder.Append("\",cb:\"").Append(this.CalculationBegin);
            stringBuilder.Append("\",ce:\"").Append(this.CalculationEnd);
            stringBuilder.Append("\",ub:\"").Append(this.CollectionBegin);
            stringBuilder.Append("\",ue:\"").Append(this.CollectionEnd);
            if (this.includeToolBarLabels)
            {
                stringBuilder.Append("\",n:\"").Append(this.Name);
                stringBuilder.Append("\",d:\"").Append(this.Description);
                stringBuilder.Append("\",a:\"").Append(this.SaveButton);
                stringBuilder.Append("\",u:\"").Append(this.DeleteButton);
                stringBuilder.Append("\",r:\"").Append(this.MenuButton);
                if (this.mode == RuleType.Loop || this.mode == RuleType.Ruleset)
                {
                    stringBuilder.Append("\",x:\"").Append(this.NewExecutionRule);
                }
                else if (this.mode == RuleType.Execution)
                {
                    stringBuilder.Append("\",y:\"").Append(this.NewEvaluationRule);
                    stringBuilder.Append("\",x:\"").Append(this.NewExecutionRule);
                }
                else
                {
                    stringBuilder.Append("\",y:\"").Append(this.NewEvaluationRule);
                }
            }
            if (this.mode != RuleType.Evaluation && this.mode != RuleType.Filter)
            {
                stringBuilder.Append("\",w:\"").Append(this.Setter);
            }
            stringBuilder.Append("\",t:\"").Append(this.True);
            stringBuilder.Append("\",f:\"").Append(this.False).Append("\"}");
            return stringBuilder.ToString();
        }

        private string CheckForNull(XmlNode node, string defaultValue)
        {
            if (node != null)
            {
                return ESPL.Rule.Core.Encoder.Sanitize(node.InnerText);
            }
            return defaultValue;
        }
    }
}
