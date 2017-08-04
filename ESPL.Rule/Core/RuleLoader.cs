using ESPL.Rule.Attributes;
using ESPL.Rule.Client;
using ESPL.Rule.Common;
using ESPL.Rule.Formats;
using ESPL.Rule.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Core
{
    internal sealed class RuleLoader
    {
        private RuleLoader()
        {
        }

        internal static string GetXml(List<Element> items, string ruleID, string ruleName, string ruleDescription, XmlDocument sourceXml, RuleFormatType format, RuleType mode, bool isEvaluationType)
        {
            if (format == RuleFormatType.CodeEffects)
            {
                return ESPLFormats.GetXml(items, sourceXml, ruleID, ruleName, ruleDescription, mode, isEvaluationType);
            }
            throw new FormatException("Unknown value of CodeEffects.Rule.Common.RuleFormatType enumeration.");
        }

        internal static RuleModel LoadXml(string xmlRule, XmlDocument sourceXml, GetRuleDelegate ruleDelegate)
        {
            if (xmlRule == null || xmlRule.Trim().Length == 0)
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.ParameterIsNull, new string[0]);
            }
            RuleModel ruleModel = new RuleModel();
            ruleModel.SourceXml = sourceXml;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlRule);
            if (!Xml.IsVersion2XmlNamespace(xmlDocument.DocumentElement.NamespaceURI))
            {
                throw new NotSupportedException("Old XML formats are not supported by this version of Code Effects control.");
            }
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("x", xmlDocument.DocumentElement.NamespaceURI);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("/x:codeeffects/x:rule", xmlNamespaceManager);
            if (xmlNode == null)
            {
                xmlNode = xmlDocument.SelectSingleNode("/x:rule", xmlNamespaceManager);
            }
            if (xmlNode == null)
            {
                throw new MalformedXmlException(MalformedXmlException.ErrorIds.CouldNotLoadRuleXML, new string[0]);
            }
            ruleModel.Id = xmlNode.Attributes["id"].Value;
            XmlNode xmlNode2 = xmlNode.SelectSingleNode("x:name", xmlNamespaceManager);
            if (xmlNode2 != null)
            {
                ruleModel.Name = Encoder.Desanitize(xmlNode2.InnerText);
            }
            XmlNode xmlNode3 = xmlNode.SelectSingleNode("x:description", xmlNamespaceManager);
            if (xmlNode3 != null)
            {
                ruleModel.Desc = Encoder.Desanitize(xmlNode3.InnerText);
            }
            ruleModel.Elements = RuleLoader.LoadXml(xmlDocument, sourceXml, RuleFormatType.CodeEffects, ruleDelegate);
            ruleModel.Format = RuleFormatType.CodeEffects;
            return ruleModel;
        }

        internal static List<Element> LoadXml(XmlDocument rule, XmlDocument source, RuleFormatType format, GetRuleDelegate getRuleDelegate)
        {
            if (format == RuleFormatType.CodeEffects)
            {
                return ESPLFormats.LoadXml(rule, source, getRuleDelegate);
            }
            throw new FormatException("Unknown value of CodeEffects.Rule.Common.RuleFormatType enumeration.");
        }

        internal static string GetString(List<Element> items, XmlDocument source, Labels labels, GetRuleDelegate ruleDelegate, Dictionary<string, GetDataSourceDelegate> dataSources)
        {
            int num = 0;
            StringBuilder stringBuilder = new StringBuilder();
            RuleLoader.DoGetString(items, source, (source.DocumentElement.ChildNodes[0].Attributes["name"] == null) ? null : source.DocumentElement.ChildNodes[0].Attributes["name"].Value, labels, ruleDelegate, dataSources, stringBuilder, ref num);
            return stringBuilder.ToString().Trim();
        }

        private static void DoGetString(List<Element> items, XmlDocument sourceXml, string sourceName, Labels labels, GetRuleDelegate ruleDelegate, Dictionary<string, GetDataSourceDelegate> dataSources, StringBuilder sb, ref int i)
        {
            XmlNode xmlNode = SourceLoader.GetSourceNode(sourceXml, sourceName);
            if (xmlNode == null)
            {
                xmlNode = SourceLoader.GetSourceNodeByToken(sourceXml, sourceName);
            }
            if (xmlNode == null)
            {
                throw new SourceException(SourceException.ErrorIds.SourceNodeNotFound, new string[]
				{
					sourceName
				});
            }
            string format = null;
            string propertyName = null;
            string name = null;
            string token = null;
            string text = null;
            string assemblyString = null;
            string globalValue = null;
            Dictionary<string, List<DataSourceItem>> dataSourceItems = new Dictionary<string, List<DataSourceItem>>();
            int num = 0;
            int num2 = -1;
            XmlNode xmlNode2 = null;
            Element element = null;
            while (i < items.Count)
            {
                element = items[i];
                switch (element.Type)
                {
                    case ElementType.Flow:
                        {
                            bool isEvaluationRule = true;
                            foreach (Element current in items)
                            {
                                if (current.Type == ElementType.Clause && current.Value == "then")
                                {
                                    isEvaluationRule = false;
                                    break;
                                }
                            }
                            sb.Append(" ").Append(labels.GetFlowLabel(element.Value, isEvaluationRule)).Append(" ");
                            break;
                        }
                    case ElementType.Field:
                        {
                            name = (format = (token = (text = (assemblyString = (globalValue = null)))));
                            if (element.IsRule)
                            {
                                if (ruleDelegate == null)
                                {
                                    throw new MalformedXmlException(MalformedXmlException.ErrorIds.RuleDelegateIsNull, new string[0]);
                                }
                                propertyName = null;
                                XmlDocument xmlDocument = new XmlDocument();
                                string text2 = ruleDelegate(element.Value);
                                try
                                {
                                    xmlDocument.LoadXml(text2);
                                    XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
                                    xmlNamespaceManager.AddNamespace("x", xmlDocument.DocumentElement.NamespaceURI);
                                    XmlNode xmlNode3 = xmlDocument.SelectSingleNode(string.Format("/x:codeeffects/x:{0}/x:{1}", "rule", "name"), xmlNamespaceManager);
                                    if (xmlNode3 == null)
                                    {
                                        xmlDocument = Xml.GetEmptyRuleDocument();
                                        xmlDocument.DocumentElement.InnerXml = text2;
                                        xmlNode3 = xmlDocument.SelectSingleNode(string.Format("/x:codeeffects/x:{0}/x:{1}", "rule", "name"), xmlNamespaceManager);
                                    }
                                    if (xmlNode3 == null)
                                    {
                                        throw new Exception("The XML of the reusable rule does not contain the name node. Without the name, Code Effects control cannot display this reusable rule in the resulting string. Either use Toolbar when saving the rule or insert its name node into the Rule XML manually before saving it.");
                                    }
                                    sb.Append(xmlNode3.InnerText);
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    throw new MalformedXmlException(MalformedXmlException.ErrorIds.GetRuleDelegateInvokeError, new string[]
							{
								ex.Message
							});
                                }
                            }
                            xmlNode2 = SourceLoader.GetFieldByPropertyName(xmlNode, element.Value);
                            propertyName = element.Value;
                            string value = xmlNode2.Attributes["displayName"].Value;
                            if (xmlNode2.Name == "collection")
                            {
                                xmlNode2 = xmlNode2.ChildNodes[0];
                                if (xmlNode2.Attributes["displayName"] != null)
                                {
                                    value = xmlNode2.Attributes["displayName"].Value;
                                }
                            }
                            if (xmlNode2.Name == "enum" && xmlNode2.Attributes["class"] != null)
                            {
                                text = xmlNode2.Attributes["class"].Value;
                                assemblyString = xmlNode2.Attributes["assembly"].Value;
                            }
                            else if (xmlNode2.Name == "numeric" && xmlNode2.Attributes["dataSourceName"] != null)
                            {
                                globalValue = RuleLoader.HandleDataSources(dataSources, dataSourceItems, xmlNode2.Attributes["dataSourceName"].Value);
                            }
                            else if (xmlNode2.Attributes["format"] != null)
                            {
                                format = Encoder.Desanitize(xmlNode2.Attributes["format"].Value);
                            }
                            sb.Append(value);
                            break;
                        }
                    case ElementType.Function:
                    case ElementType.Action:
                        switch (element.FuncType)
                        {
                            case FunctionType.Name:
                                {
                                    format = (propertyName = (text = (assemblyString = (globalValue = null))));
                                    num = 0;
                                    num2 = -1;
                                    name = element.Value;
                                    token = element.Token;
                                    if (element.Type == ElementType.Function)
                                    {
                                        xmlNode2 = SourceLoader.GetFunctionByToken(xmlNode, token);
                                    }
                                    else
                                    {
                                        xmlNode2 = SourceLoader.GetActionByToken(xmlNode, token);
                                    }
                                    sb.Append(xmlNode2.Attributes["displayName"].Value);
                                    XmlNode xmlNode4 = SourceLoader.GetParamNode(xmlNode2);
                                    num = ((xmlNode4 != null && xmlNode4.ChildNodes.Count > 0) ? RuleLoader.CountInputParams(xmlNode4.ChildNodes) : 0);
                                    if (num > 0)
                                    {
                                        sb.Append(" (");
                                    }
                                    if (element.Type == ElementType.Function)
                                    {
                                        xmlNode4 = SourceLoader.GetReturnNode(xmlNode2);
                                        OperatorType operatorType = Converter.ClientStringToClientType(xmlNode4.Attributes["type"].Value);
                                        if (operatorType == OperatorType.Date || operatorType == OperatorType.Time)
                                        {
                                            format = Encoder.Desanitize(xmlNode4.Attributes["format"].Value);
                                        }
                                        else if (operatorType == OperatorType.Enum)
                                        {
                                            text = xmlNode4.Attributes["class"].Value;
                                            assemblyString = xmlNode4.Attributes["assembly"].Value;
                                        }
                                        else if (operatorType == OperatorType.Numeric && xmlNode4.Attributes["dataSourceName"] != null)
                                        {
                                            globalValue = RuleLoader.HandleDataSources(dataSources, dataSourceItems, xmlNode4.Attributes["dataSourceName"].Value);
                                        }
                                    }
                                    break;
                                }
                            case FunctionType.Param:
                                num2++;
                                if (element.InpType != InputType.Field)
                                {
                                    switch (element.Oper)
                                    {
                                        case OperatorType.String:
                                            sb.Append("\"").Append(element.Value).Append("\"");
                                            goto IL_791;
                                        case OperatorType.Numeric:
                                            {
                                                Parameter paramByIndex = RuleLoader.GetParamByIndex(xmlNode, element.Type, name, token, num2);
                                                if (string.IsNullOrWhiteSpace(paramByIndex.Settings.DataSourceName))
                                                {
                                                    sb.Append(element.Value);
                                                    goto IL_791;
                                                }
                                                string globalValue2 = RuleLoader.HandleDataSources(dataSources, dataSourceItems, paramByIndex.Settings.DataSourceName);
                                                RuleLoader.HandleNumericValue(dataSourceItems, sb, globalValue2, element.Value);
                                                goto IL_791;
                                            }
                                        case OperatorType.Date:
                                            sb.Append(DateTime.Parse(element.Value).ToString(Encoder.Desanitize(RuleLoader.GetParamByIndex(xmlNode, element.Type, name, token, num2).Settings.Format), Thread.CurrentThread.CurrentCulture));
                                            goto IL_791;
                                        case OperatorType.Time:
                                            sb.Append(DateTime.Parse("1/1/2010 " + element.Value).ToString(Encoder.Desanitize(RuleLoader.GetParamByIndex(xmlNode, element.Type, name, token, num2).Settings.Format)));
                                            goto IL_791;
                                        case OperatorType.Bool:
                                            sb.Append((element.Value == "true") ? labels.True : labels.False);
                                            goto IL_791;
                                        case OperatorType.Enum:
                                            {
                                                Parameter paramByIndex2 = RuleLoader.GetParamByIndex(xmlNode, element.Type, name, token, num2);
                                                Assembly assembly = Assembly.Load(paramByIndex2.Settings.Assembly);
                                                Type type = assembly.GetType(paramByIndex2.Settings.TypeFullName);
                                                sb.Append(RuleLoader.GetEnumItemDisplayName(type, element.Value));
                                                goto IL_791;
                                            }
                                    }
                                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.InvalidOrderOfNodes, new string[0]);
                                }
                                xmlNode2 = SourceLoader.GetFieldByPropertyName(xmlNode, element.Value);
                                sb.Append(xmlNode2.Attributes["displayName"].Value);
                            IL_791:
                                sb.Append(", ");
                                break;
                            case FunctionType.End:
                                if (num > 0)
                                {
                                    sb.Remove(sb.Length - 2, 2);
                                    sb.Append(")");
                                }
                                num = 0;
                                num2 = -1;
                                break;
                        }
                        break;
                    case ElementType.Operator:
                        sb.Append(" ").Append(labels.GetOperatorLabel(element.Value, element.Oper)).Append(" ");
                        break;
                    case ElementType.Value:
                        if (element.InpType != InputType.Field)
                        {
                            switch (element.Oper)
                            {
                                case OperatorType.String:
                                    sb.Append("\"").Append(Encoder.Sanitize(element.Value)).Append("\"");
                                    goto IL_D0C;
                                case OperatorType.Numeric:
                                    break;
                                case OperatorType.Date:
                                    sb.Append(DateTime.Parse(element.Value).ToString(format, Thread.CurrentThread.CurrentCulture));
                                    goto IL_D0C;
                                case OperatorType.Time:
                                    sb.Append(DateTime.Parse("1/1/2010 " + element.Value).ToString(format));
                                    goto IL_D0C;
                                case OperatorType.Bool:
                                    sb.Append((element.Value == "true") ? labels.True : labels.False);
                                    goto IL_D0C;
                                case (OperatorType)5:
                                    goto IL_AC6;
                                case OperatorType.Enum:
                                    if (text != null)
                                    {
                                        Assembly assembly2 = Assembly.Load(assemblyString);
                                        Type type2 = assembly2.GetType(text);
                                        sb.Append(RuleLoader.GetEnumItemDisplayName(type2, element.Value));
                                        goto IL_D0C;
                                    }
                                    if (xmlNode2 == null)
                                    {
                                        xmlNode2 = SourceLoader.GetFieldByPropertyName(xmlNode, propertyName);
                                    }
                                    IEnumerator enumerator2 = xmlNode2.ChildNodes.GetEnumerator();
                                        while (enumerator2.MoveNext())
                                        {
                                            XmlNode xmlNode5 = (XmlNode)enumerator2.Current;
                                            if (xmlNode5.Attributes["value"].Value == element.Value)
                                            {
                                                sb.Append(xmlNode5.Attributes["displayName"].Value);
                                                break;
                                            }
                                        }
                                        goto IL_D0C;
                                    break;
                                default:
                                    goto IL_AC6;
                            }
                            RuleLoader.HandleNumericValue(dataSourceItems, sb, globalValue, element.Value);
                            globalValue = null;
                            break;
                        IL_AC6:
                            throw new InvalidRuleException(InvalidRuleException.ErrorIds.InvalidOrderOfNodes, new string[0]);
                        }
                        xmlNode2 = SourceLoader.GetFieldByPropertyName(xmlNode, element.Value);
                        sb.Append(xmlNode2.Attributes["displayName"].Value);
                        break;
                    case ElementType.Clause:
                        sb.Append(" ").Append(labels.GetClauseLabel(element.Value)).Append(" ");
                        break;
                    case ElementType.LeftParenthesis:
                        sb.Append(labels.GetScopeLabel(Labels.Scope.SectionBegin));
                        break;
                    case ElementType.RightParenthesis:
                        sb.Append(labels.GetScopeLabel(Labels.Scope.SectionEnd));
                        break;
                    case ElementType.LeftBracket:
                        sb.Append(labels.GetScopeLabel(Labels.Scope.SectionBegin));
                        break;
                    case ElementType.RightBracket:
                        sb.Append(labels.GetScopeLabel(Labels.Scope.SectionEnd));
                        break;
                    case ElementType.Calculation:
                        if (element.CalType == CalculationType.Function)
                        {
                            FunctionType funcType = element.FuncType;
                            if (funcType != FunctionType.Name)
                            {
                                if (funcType == FunctionType.End)
                                {
                                    if (num > 0)
                                    {
                                        sb.Remove(sb.Length - 2, 2);
                                        sb.Append(")");
                                    }
                                    num = 0;
                                    num2 = -1;
                                }
                            }
                            else
                            {
                                format = (propertyName = (text = (assemblyString = (globalValue = null))));
                                num = 0;
                                num2 = -1;
                                name = element.Value;
                                token = element.Token;
                                xmlNode2 = SourceLoader.GetFunctionByToken(xmlNode, token);
                                sb.Append(xmlNode2.Attributes["displayName"].Value);
                                XmlNode xmlNode4 = SourceLoader.GetParamNode(xmlNode2);
                                num = ((xmlNode4 != null && xmlNode4.ChildNodes.Count > 0) ? RuleLoader.CountInputParams(xmlNode4.ChildNodes) : 0);
                                if (num > 0)
                                {
                                    sb.Append(" (");
                                }
                                xmlNode4 = SourceLoader.GetReturnNode(xmlNode2);
                                OperatorType operatorType = Converter.ClientStringToClientType(xmlNode4.Attributes["type"].Value);
                                if (operatorType == OperatorType.Date || operatorType == OperatorType.Time)
                                {
                                    format = Encoder.Desanitize(xmlNode4.Attributes["format"].Value);
                                }
                                else if (operatorType == OperatorType.Enum)
                                {
                                    text = xmlNode4.Attributes["class"].Value;
                                    assemblyString = xmlNode4.Attributes["assembly"].Value;
                                }
                                else if (operatorType == OperatorType.Numeric && xmlNode4.Attributes["dataSourceName"] != null)
                                {
                                    globalValue = RuleLoader.HandleDataSources(dataSources, dataSourceItems, xmlNode4.Attributes["dataSourceName"].Value);
                                }
                            }
                        }
                        else
                        {
                            RuleLoader.GetCalculationString(element, sb, xmlNode, labels);
                        }
                        break;
                    case ElementType.Setter:
                        sb.Append(" ").Append(labels.GetSetterLabel(element.Value)).Append(" ");
                        break;
                    case ElementType.LeftSource:
                        sb.Append(labels.GetScopeLabel(Labels.Scope.CollectionBegin));
                        i++;
                        RuleLoader.DoGetString(items, sourceXml, element.Value, labels, ruleDelegate, dataSources, sb, ref i);
                        break;
                    case ElementType.RightSource:
                        sb.Append(labels.GetScopeLabel(Labels.Scope.CollectionEnd));
                        return;
                    case ElementType.Where:
                        sb.Append(" ").Append(labels.GetWhereLabel()).Append(" ");
                        break;
                    case ElementType.Exists:
                        sb.Append(" ").Append(labels.GetExistsLabel(element.SelType)).Append(" ");
                        break;
                }
            IL_D0C:
                i++;
            }
        }

        private static string HandleDataSources(Dictionary<string, GetDataSourceDelegate> dataSources, Dictionary<string, List<DataSourceItem>> dataSourceItems, string dataSourceName)
        {
            if (dataSources == null || dataSources.Count <= 0)
            {
                return null;
            }
            if (!dataSources.ContainsKey(dataSourceName))
            {
                return null;
            }
            if (!dataSourceItems.ContainsKey(dataSourceName))
            {
                List<DataSourceItem> value = dataSources[dataSourceName]();
                dataSourceItems.Add(dataSourceName, value);
            }
            return dataSourceName;
        }

        private static void HandleNumericValue(Dictionary<string, List<DataSourceItem>> dataSourceItems, StringBuilder sb, string globalValue, string value)
        {
            if (!string.IsNullOrWhiteSpace(globalValue))
            {
                List<DataSourceItem> list = dataSourceItems[globalValue];
                if (list == null || list.Count <= 0)
                {
                    sb.Append(value);
                    return;
                }
                bool flag = false;
                foreach (DataSourceItem current in list)
                {
                    if (current.ID.ToString() == value)
                    {
                        sb.Append(Encoder.Sanitize(current.Name));
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    sb.Append(value);
                    return;
                }
            }
            else
            {
                sb.Append(value);
            }
        }

        private static Parameter GetParamByIndex(XmlNode source, ElementType type, string name, string token, int index)
        {
            XmlNode function;
            if (type == ElementType.Function)
            {
                function = SourceLoader.GetFunctionByToken(source, token);
            }
            else
            {
                function = SourceLoader.GetActionByToken(source, token);
            }
            int num = 0;
            XmlNode paramNode = SourceLoader.GetParamNode(function);
            for (int i = 0; i < paramNode.ChildNodes.Count; i++)
            {
                if (paramNode.ChildNodes[i].Name == "input" || paramNode.ChildNodes[i].Name == "collection")
                {
                    if (num == index)
                    {
                        Parameter parameter = new Parameter();
                        if (paramNode.ChildNodes[i].Attributes["format"] != null)
                        {
                            parameter.Settings.Format = paramNode.ChildNodes[i].Attributes["format"].Value;
                        }
                        if (paramNode.ChildNodes[i].Attributes["class"] != null)
                        {
                            parameter.Settings.TypeFullName = paramNode.ChildNodes[i].Attributes["class"].Value;
                        }
                        if (paramNode.ChildNodes[i].Attributes["assembly"] != null)
                        {
                            parameter.Settings.Assembly = paramNode.ChildNodes[i].Attributes["assembly"].Value;
                        }
                        if (paramNode.ChildNodes[i].Attributes["dataSourceName"] != null)
                        {
                            parameter.Settings.DataSourceName = paramNode.ChildNodes[i].Attributes["dataSourceName"].Value;
                        }
                        return parameter;
                    }
                    num++;
                }
            }
            return null;
        }

        private static void GetCalculationString(Element el, StringBuilder sb, XmlNode source, Labels labels)
        {
            switch (el.CalType)
            {
                case CalculationType.Field:
                    sb.Append(SourceLoader.GetFieldByPropertyName(source, el.Value).Attributes["displayName"].Value);
                    return;
                case CalculationType.LeftParenthesis:
                    sb.Append(labels.GetScopeLabel(Labels.Scope.SectionBegin));
                    return;
                case CalculationType.RightParenthesis:
                    sb.Append(labels.GetScopeLabel(Labels.Scope.SectionEnd));
                    return;
                case CalculationType.Multiplication:
                    sb.Append(" * ");
                    return;
                case CalculationType.Division:
                    sb.Append(" / ");
                    return;
                case (CalculationType)5:
                    break;
                case CalculationType.Addition:
                    sb.Append(" + ");
                    return;
                case CalculationType.Subtraction:
                    sb.Append(" - ");
                    return;
                case CalculationType.Number:
                    sb.Append(el.Value);
                    break;
                default:
                    return;
            }
        }

        private static string GetEnumItemDisplayName(Type t, string value)
        {
            Array values = Enum.GetValues(t);
            foreach (object current in values)
            {
                if (int.Parse(Enum.Format(t, current, "D")).ToString() == value)
                {
                    string name = Enum.GetName(t, current);
                    string result = name;
                    FieldInfo field = t.GetField(name);
                    object[] customAttributes = field.GetCustomAttributes(typeof(EnumItemAttribute), false);
                    if (customAttributes.Length > 0)
                    {
                        result = Encoder.Sanitize(((EnumItemAttribute)customAttributes[0]).DisplayName);
                    }
                    return result;
                }
            }
            throw new MalformedXmlException(MalformedXmlException.ErrorIds.MissingEnumMember, new string[]
			{
				value
			});
        }

        private static int CountInputParams(XmlNodeList pars)
        {
            int num = 0;
            foreach (XmlNode xmlNode in pars)
            {
                if (xmlNode.Name == "input" || xmlNode.Name == "collection")
                {
                    num++;
                }
            }
            return num;
        }
    }
}
