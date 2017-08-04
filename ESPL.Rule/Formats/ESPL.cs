using ESPL.Rule.Client;
using ESPL.Rule.Common;
using ESPL.Rule.Core;
using ESPL.Rule.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Formats
{
    internal sealed class ESPLFormats
    {
        private ESPLFormats()
        {
        }

        internal static string GetXml(List<Element> items, XmlDocument source, string ruleID, string ruleName, string ruleDescription, RuleType mode, bool isCurrentRuleOfEvalType)
        {
            XmlDocument emptyRuleDocument = Xml.GetEmptyRuleDocument();
            int num = 0;
            ESPLFormats.FillDefinition(items, emptyRuleDocument, ruleID, ruleName, ruleDescription, source, (source.DocumentElement.ChildNodes[0].Attributes["name"] == null) ? null : ESPL.Rule.Core.Encoder.GetHashToken(source.DocumentElement.ChildNodes[0].Attributes["name"].Value), mode, isCurrentRuleOfEvalType, true, ref num);
            return emptyRuleDocument.OuterXml;
        }

        internal static List<Element> LoadXml(XmlDocument rule, XmlDocument source, GetRuleDelegate ruleDelegate)
        {
            List<Element> list = new List<Element>();
            ESPLFormats.FillRule(list, rule.DocumentElement.ChildNodes[0], source, ruleDelegate);
            return list;
        }

        private static void FillRule(List<Element> list, XmlNode ruleXml, XmlDocument sourceXml, GetRuleDelegate ruleDelegate)
        {
            try
            {
                string uiNamespaceByRuleNamespace = Xml.GetUiNamespaceByRuleNamespace(ruleXml.OwnerDocument.DocumentElement.NamespaceURI);
                XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(ruleXml.OwnerDocument.NameTable);
                xmlNamespaceManager.AddNamespace(Xml.RuleNamespaceTag, ruleXml.OwnerDocument.DocumentElement.NamespaceURI);
                xmlNamespaceManager.AddNamespace(Xml.UiNamespaceTag, uiNamespaceByRuleNamespace);
                XmlNode xmlNode = ruleXml.SelectSingleNode(string.Format("{0}:{1}", Xml.RuleNamespaceTag, "definition"), xmlNamespaceManager);
                XmlNode xmlNode2 = null;
                if (ruleXml.Attributes["type"] == null || string.IsNullOrWhiteSpace(ruleXml.Attributes["type"].Value))
                {
                    xmlNode2 = sourceXml.DocumentElement.ChildNodes[0];
                }
                else
                {
                    Type type = Type.GetType(ruleXml.Attributes["type"].Value, true, false);
                    foreach (XmlNode xmlNode3 in sourceXml.DocumentElement.ChildNodes)
                    {
                        if (xmlNode3.Attributes["name"] == null || xmlNode3.Attributes["name"].Value == type.FullName)
                        {
                            xmlNode2 = xmlNode3;
                            break;
                        }
                    }
                    if (xmlNode2 == null)
                    {
                        throw new SourceException(SourceException.ErrorIds.SourceNodeNotFound, new string[]
						{
							type.FullName
						});
                    }
                }
                int num = list.Count;
                bool flag = false;
                XmlNode xmlNode4 = xmlNode.SelectSingleNode(string.Format("{0}:{1}", Xml.RuleNamespaceTag, "if"), xmlNamespaceManager);
                if (xmlNode4 == null)
                {
                    xmlNode4 = xmlNode.SelectSingleNode(string.Format("{0}:{1}", Xml.RuleNamespaceTag, "while"), xmlNamespaceManager);
                }
                else if (xmlNode.ChildNodes.Count > 1 && xmlNode.ChildNodes[1].Name == "if")
                {
                    flag = true;
                }
                if (xmlNode4 == null)
                {
                    if (list.Count == 0)
                    {
                        list.Add(new Element
                        {
                            Type = ElementType.Flow,
                            Value = "if"
                        });
                        num++;
                    }
                    ESPLFormats.FillEvaluationElements(list, xmlNode2, xmlNode, uiNamespaceByRuleNamespace, ruleDelegate);
                }
                else if (flag)
                {
                    if (list.Count == 0)
                    {
                        num++;
                    }
                    for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
                    {
                        ESPLFormats.FillExecutionElements(list, xmlNode2, xmlNode.ChildNodes[i], uiNamespaceByRuleNamespace, (i == 0) ? "if" : "elseIf", ruleDelegate);
                    }
                }
                else
                {
                    if (list.Count == 0)
                    {
                        num++;
                    }
                    ESPLFormats.FillExecutionElements(list, xmlNode2, xmlNode.ChildNodes[0], uiNamespaceByRuleNamespace, "if", ruleDelegate);
                }
                XmlNode xmlNode5 = ruleXml.SelectSingleNode(string.Format("{0}:{1}/{0}:{2}", Xml.RuleNamespaceTag, "format", "lines"), xmlNamespaceManager);
                foreach (XmlNode xmlNode6 in xmlNode5.ChildNodes)
                {
                    Element element = new Element();
                    element.Type = ElementType.NewLine;
                    int num2 = num + int.Parse(xmlNode6.Attributes["index"].Value);
                    if (num2 >= list.Count)
                    {
                        list.Add(element);
                    }
                    else
                    {
                        list.Insert(num2, element);
                    }
                    int num3 = int.Parse(xmlNode6.Attributes["tabs"].Value);
                    if (num3 > 0)
                    {
                        num2++;
                        int j = 0;
                        while (j < num3)
                        {
                            element = new Element();
                            element.Type = ElementType.Tab;
                            if (num2 >= list.Count)
                            {
                                list.Add(element);
                            }
                            else
                            {
                                list.Insert(num2, element);
                            }
                            j++;
                            num2++;
                        }
                    }
                }
            }
            catch (SourceException)
            {
                throw;
            }
            catch (MalformedXmlException)
            {
                throw;
            }
            catch (InvalidRuleException)
            {
                throw;
            }
            catch (RuleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid2, new string[]
				{
					ex.Message
				});
            }
        }

        private static void FillExecutionElements(List<Element> list, XmlNode source, XmlNode node, string uiNamespace, string ifValue, GetRuleDelegate ruleDelegate)
        {
            if (node.Name == "else")
            {
                node = node.ChildNodes[0];
            }
            string name;
            if ((name = node.Name) != null)
            {
                if (!(name == "if") && !(name == "while"))
                {
                    if (!(name == "method") && !(name == "set"))
                    {
                        goto IL_1AC;
                    }
                }
                else
                {
                    if (node.ChildNodes.Count < 2 || node.ChildNodes.Count > 3)
                    {
                        throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid, new string[0]);
                    }
                    list.Add(new Element
                    {
                        Type = ElementType.Flow,
                        Value = ifValue
                    });
                    IEnumerator enumerator = node.ChildNodes.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            XmlNode xmlNode = (XmlNode)enumerator.Current;
                            string name2;
                            if ((name2 = xmlNode.Name) != null)
                            {
                                if (name2 == "clause")
                                {
                                    ESPLFormats.FillEvaluationElements(list, source, xmlNode, uiNamespace, ruleDelegate);
                                    continue;
                                }
                                if (name2 == "else")
                                {
                                    ESPLFormats.FillExecutionElements(list, source, xmlNode, uiNamespace, "elseIf", ruleDelegate);
                                    continue;
                                }
                                if (name2 == "then")
                                {
                                    list.Add(new Element
                                    {
                                        Type = ElementType.Clause,
                                        Value = "then"
                                    });
                                    ESPLFormats.AppendActionElements(list, source, xmlNode, uiNamespace);
                                    continue;
                                }
                            }
                            throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid, new string[0]);
                        }
                        return;
                }
                list.Add(new Element
                {
                    Type = ElementType.Flow,
                    Value = "else"
                });
                ESPLFormats.AppendActionElements(list, source, node.ParentNode, uiNamespace);
                return;
            }
        IL_1AC:
            throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid, new string[0]);
        }

        private static void FillEvaluationElements(List<Element> list, XmlNode source, XmlNode parent, string uiNamespace, GetRuleDelegate ruleDelegate)
        {
            foreach (XmlNode xmlNode in parent.ChildNodes)
            {
                string name;
                if ((name = xmlNode.Name) != null)
                {
                    if (!(name == "and") && !(name == "or"))
                    {
                        if (!(name == "condition"))
                        {
                            if (name == "rule")
                            {
                                if (xmlNode.ChildNodes.Count != 1)
                                {
                                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.InvalidNumberOfChildNodes, new string[0]);
                                }
                                XmlNode xmlNode2 = null;
                                foreach (XmlNode xmlNode3 in parent.OwnerDocument.DocumentElement.ChildNodes)
                                {
                                    if (xmlNode3.Attributes["id"].Value.ToLower() == xmlNode.Attributes["id"].Value.ToLower())
                                    {
                                        xmlNode2 = xmlNode3;
                                        break;
                                    }
                                }
                                if (xmlNode2 == null && ruleDelegate != null)
                                {
                                    string text = ruleDelegate(xmlNode.Attributes["id"].Value);
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        XmlDocument xmlDocument = new XmlDocument();
                                        xmlDocument.LoadXml(text);
                                        if (xmlDocument.DocumentElement.ChildNodes.Count != 0)
                                        {
                                            xmlNode2 = xmlDocument.DocumentElement.ChildNodes[0];
                                        }
                                    }
                                }
                                if (xmlNode2 == null)
                                {
                                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLNotFound, new string[]
									{
										xmlNode.Attributes["id"].Value
									});
                                }
                                SelectionType selType = Converter.StringToSelectionType(xmlNode.Attributes["operator"].Value);
                                ESPLFormats.AppendExistsElement(list, ElementType.Exists, selType);
                                Element element = new Element();
                                element.Value = xmlNode.ChildNodes[0].Attributes["name"].Value;
                                try
                                {
                                    SourceLoader.GetFieldByPropertyName(source, element.Value);
                                }
                                catch (SourceException)
                                {
                                    element.NotFound = true;
                                }
                                element.Type = ElementType.Field;
                                element.Oper = OperatorType.Collection;
                                element.CollType = CollectionType.Reference;
                                list.Add(element);
                                ESPLFormats.AppendExistsElement(list, ElementType.Where, selType);
                                Type type = Type.GetType(xmlNode2.Attributes["type"].Value, true, false);
                                ESPLFormats.AppendClauseElement(list, ElementType.LeftSource, ESPL.Rule.Core.Encoder.GetHashToken(type.FullName));
                                ESPLFormats.FillRule(list, xmlNode2, source.OwnerDocument, ruleDelegate);
                                type = Type.GetType(source.Attributes["type"].Value, true, false);
                                ESPLFormats.AppendClauseElement(list, ElementType.RightSource, ESPL.Rule.Core.Encoder.GetHashToken(type.FullName));
                                if (xmlNode.NextSibling != null && parent.Name != "definition" && parent.Name != "clause")
                                {
                                    ESPLFormats.AppendClauseElement(list, ElementType.Clause, parent.Name);
                                    continue;
                                }
                                continue;
                            }
                        }
                        else
                        {
                            if (xmlNode.ChildNodes.Count < 1 || xmlNode.ChildNodes.Count > 2)
                            {
                                throw new InvalidRuleException(InvalidRuleException.ErrorIds.InvalidNumberOfConditionChildNodes, new string[0]);
                            }
                            ESPLFormats.AppendConditionElements(list, source, new Element
                            {
                                Type = ElementType.Operator,
                                Value = xmlNode.Attributes["type"].Value
                            }, xmlNode.ChildNodes[0], (xmlNode.ChildNodes.Count > 1) ? xmlNode.ChildNodes[1] : null, uiNamespace);
                            if (xmlNode.NextSibling != null && parent.Name != "definition" && parent.Name != "clause")
                            {
                                ESPLFormats.AppendClauseElement(list, ElementType.Clause, parent.Name);
                                continue;
                            }
                            continue;
                        }
                    }
                    else
                    {
                        bool flag = xmlNode.Attributes["block", uiNamespace] != null && xmlNode.Attributes["block", uiNamespace].Value == "true";
                        if (flag)
                        {
                            ESPLFormats.AppendClauseElement(list, ElementType.LeftParenthesis, null);
                        }
                        ESPLFormats.FillEvaluationElements(list, source, xmlNode, uiNamespace, ruleDelegate);
                        if (flag)
                        {
                            ESPLFormats.AppendClauseElement(list, ElementType.RightParenthesis, null);
                        }
                        if (xmlNode.NextSibling != null && parent.Name != "definition" && parent.Name != "clause")
                        {
                            ESPLFormats.AppendClauseElement(list, ElementType.Clause, parent.Name);
                            continue;
                        }
                        continue;
                    }
                }
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid, new string[0]);
            }
        }

        private static void FillPropertyElement(Element field, XmlNode fieldNode, XmlNode source)
        {
            XmlNode xmlNode = null;
            try
            {
                xmlNode = SourceLoader.GetFieldByPropertyName(source, fieldNode.Attributes["name"].Value);
            }
            catch (SourceException)
            {
                field.NotFound = true;
            }
            field.Type = ElementType.Field;
            field.Value = fieldNode.Attributes["name"].Value;
            field.Oper = (field.NotFound ? OperatorType.String : Converter.ClientStringToClientType(xmlNode.Name));
            if (!field.NotFound)
            {
                ESPLFormats.SetElementValues(field, xmlNode, false);
            }
        }

        private static void FillParamOperatorType(Element par, XmlNode paramNode)
        {
            if (paramNode.Name == "collection")
            {
                par.Oper = OperatorType.Collection;
                par.CollType = Converter.StringToCollectionType(paramNode.ChildNodes[0].Name);
                return;
            }
            par.Oper = Converter.ClientStringToClientType(paramNode.Attributes["type"].Value);
        }

        private static void FillFunctionElements(out Element field, out List<Element> parameters, out Element end, XmlNode fieldNode, XmlNode source, bool isFunctionValue, bool isCalculation)
        {
            field = new Element();
            if (isCalculation)
            {
                field.Type = ElementType.Calculation;
                field.CalType = CalculationType.Function;
            }
            else
            {
                field.Type = ElementType.Function;
            }
            field.FuncType = FunctionType.Name;
            field.IsFuncValue = isFunctionValue;
            XmlNode xmlNode = null;
            try
            {
                field.Token = (field.Value = SourceLoader.GetTokenByRuleMethodNode(source, fieldNode, true));
                if (fieldNode.Attributes["type"] != null)
                {
                    ESPLFormats.SetElementType(field, fieldNode.Attributes["type"].Value);
                }
                xmlNode = SourceLoader.GetFunctionByToken(source, field.Token);
            }
            catch (SourceException)
            {
                field.NotFound = true;
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    field.Token = (field.Value = fieldNode.Attributes["name"].Value);
                }
            }
            parameters = new List<Element>();
            if (!field.NotFound)
            {
                if (xmlNode.ChildNodes.Count != 2)
                {
                    throw new SourceException(SourceException.ErrorIds.InvalidSourceXML, new string[0]);
                }
                XmlNode xmlNode2 = xmlNode.ChildNodes[0];
                if (xmlNode2.ChildNodes.Count != fieldNode.ChildNodes.Count)
                {
                    throw new EvaluationException(EvaluationException.ErrorIds.ParameterCountMismatch, new string[]
					{
						fieldNode.Attributes["name"].Value
					});
                }
                if (xmlNode2.ChildNodes.Count > 0)
                {
                    for (int i = 0; i < xmlNode2.ChildNodes.Count; i++)
                    {
                        XmlNode xmlNode3 = xmlNode2.ChildNodes[i];
                        if (!(xmlNode3.Name == "constant") && !(xmlNode3.Name == "source"))
                        {
                            Element element = new Element();
                            element.Type = ElementType.Function;
                            element.FuncType = FunctionType.Param;
                            element.ParameterType = ParameterType.Input;
                            ESPLFormats.FillParamOperatorType(element, xmlNode3);
                            element.InpType = ((fieldNode.ChildNodes[i].Name == "property") ? InputType.Field : InputType.Input);
                            if (element.InpType == InputType.Field)
                            {
                                element.Value = fieldNode.ChildNodes[i].Attributes["name"].Value;
                            }
                            else
                            {
                                element.Value = ESPL.Rule.Core.Encoder.Sanitize(fieldNode.ChildNodes[i].InnerText);
                                ESPLFormats.SetElementValues(element, xmlNode3, true);
                            }
                            parameters.Add(element);
                        }
                    }
                }
                XmlNode xmlNode4 = xmlNode.ChildNodes[1];
                field.Oper = Converter.ClientStringToClientType(xmlNode4.Attributes["type"].Value);
                switch (field.Oper)
                {
                    case OperatorType.String:
                        field.Max = new decimal?(new int?(int.Parse(xmlNode4.Attributes["maxLength"].Value)).Value);
                        break;
                    case OperatorType.Numeric:
                        field.Dec = (xmlNode4.Attributes["allowDecimal"].Value == "true");
                        field.Min = new decimal?(decimal.Parse(xmlNode4.Attributes["min"].Value));
                        field.Max = new decimal?(decimal.Parse(xmlNode4.Attributes["max"].Value));
                        field.Cal = (xmlNode4.Attributes["allowCalculation"].Value == "true");
                        break;
                    case OperatorType.Date:
                    case OperatorType.Time:
                        field.Format = xmlNode4.Attributes["format"].Value;
                        break;
                    case OperatorType.Enum:
                        if (xmlNode4.Attributes["class"] == null)
                        {
                            throw new MalformedXmlException(MalformedXmlException.ErrorIds.MissingReturnClassAttribute, new string[0]);
                        }
                        ESPLFormats.SetElementType(field, xmlNode4.Attributes["class"].Value, true);
                        break;
                }
            }
            else
            {
                field.Oper = OperatorType.String;
                parameters = null;
            }
            end = new Element();
            if (isCalculation)
            {
                end.Type = ElementType.Calculation;
                end.CalType = CalculationType.Function;
            }
            else
            {
                end.Type = ElementType.Function;
            }
            end.FuncType = FunctionType.End;
            end.NotFound = field.NotFound;
            end.Value = (end.Token = field.Token);
            end.IsFuncValue = isFunctionValue;
            end.Oper = field.Oper;
        }

        private static void AppendActionElements(List<Element> list, XmlNode source, XmlNode parent, string uiNamespace)
        {
            bool flag = true;
            foreach (XmlNode xmlNode in parent.ChildNodes)
            {
                if (!flag)
                {
                    ESPLFormats.AppendClauseElement(list, ElementType.Clause, "and");
                }
                else
                {
                    flag = false;
                }
                string name;
                if ((name = xmlNode.Name) != null)
                {
                    if (name == "method")
                    {
                        ESPLFormats.AppendActionElement(list, source, xmlNode);
                        continue;
                    }
                    if (name == "set")
                    {
                        ESPLFormats.AppendSetterElement(list, source, xmlNode, uiNamespace);
                        continue;
                    }
                }
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid, new string[0]);
            }
        }

        private static void AppendClauseElement(List<Element> list, ElementType type, string value)
        {
            Element element = new Element();
            element.Type = type;
            if (value != null)
            {
                element.Value = value;
            }
            list.Add(element);
        }

        private static void AppendExistsElement(List<Element> list, ElementType type, SelectionType selType)
        {
            list.Add(new Element
            {
                Type = type,
                SelType = selType
            });
        }

        private static void AppendSetterElement(List<Element> list, XmlNode source, XmlNode node, string uiNamespace)
        {
            if (node.ChildNodes.Count != 2)
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid, new string[0]);
            }
            ESPLFormats.AppendClauseElement(list, ElementType.Setter, "set");
            Element element = null;
            Element element2 = null;
            Element element3 = null;
            Element element4 = null;
            List<Element> list2 = null;
            List<Element> list3 = null;
            string name;
            if ((name = node.ChildNodes[0].Name) != null && name == "property")
            {
                Element element5 = new Element();
                ESPLFormats.FillPropertyElement(element5, node.ChildNodes[0], source);
                list.Add(element5);
                list.Add(new Element
                {
                    Type = ElementType.Setter,
                    Value = "to",
                    Oper = element5.Oper
                });
                XmlNode xmlNode = node.ChildNodes[1];
                string name2;
                if ((name2 = xmlNode.Name) != null)
                {
                    if (!(name2 == "expression"))
                    {
                        if (!(name2 == "property"))
                        {
                            if (!(name2 == "method"))
                            {
                                if (!(name2 == "value"))
                                {
                                    goto IL_1E7;
                                }
                                element = new Element();
                                element.Type = ElementType.Value;
                                element.InpType = InputType.Input;
                                element.Oper = element5.Oper;
                                element.Value = ((element.Oper == OperatorType.String) ? ESPL.Rule.Core.Encoder.Sanitize(xmlNode.InnerText) : xmlNode.InnerText);
                            }
                            else
                            {
                                ESPLFormats.FillFunctionElements(out element, out list2, out element4, xmlNode, source, true, false);
                            }
                        }
                        else
                        {
                            element = new Element();
                            element.Type = ElementType.Value;
                            element.InpType = InputType.Field;
                            element.Value = xmlNode.Attributes["name"].Value;
                            element.Oper = element5.Oper;
                        }
                    }
                    else
                    {
                        element2 = new Element();
                        element2.Type = ElementType.LeftBracket;
                        element3 = new Element();
                        element3.Type = ElementType.RightBracket;
                        list3 = new List<Element>();
                        ESPLFormats.AppendExpressionElements(list3, source, xmlNode, uiNamespace);
                    }
                    if (element != null)
                    {
                        list.Add(element);
                    }
                    if (list2 != null && list2.Count > 0)
                    {
                        ESPLFormats.AppendParameterElements(list, list2);
                    }
                    if (element4 != null)
                    {
                        list.Add(element4);
                        return;
                    }
                    if (element2 != null)
                    {
                        list.Add(element2);
                        foreach (Element current in list3)
                        {
                            list.Add(current);
                        }
                        list.Add(element3);
                    }
                    return;
                }
            IL_1E7:
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid, new string[0]);
            }
            throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid, new string[0]);
        }

        private static void AppendActionElement(List<Element> list, XmlNode source, XmlNode node)
        {
            List<Element> list2 = null;
            Element element = new Element();
            element.Type = ElementType.Action;
            element.FuncType = FunctionType.Name;
            XmlNode xmlNode = null;
            string value = null;
            try
            {
                value = SourceLoader.GetTokenByRuleMethodNode(source, node, false);
                element.Token = (element.Value = value);
                if (node.Attributes["type"] != null)
                {
                    ESPLFormats.SetElementType(element, node.Attributes["type"].Value);
                }
                xmlNode = SourceLoader.GetActionByToken(source, element.Token);
            }
            catch (SourceException)
            {
                element.NotFound = true;
                if (string.IsNullOrWhiteSpace(element.Value))
                {
                    element.Token = (element.Value = node.Attributes["name"].Value);
                }
            }
            list.Add(element);
            if (!element.NotFound)
            {
                if (xmlNode.ChildNodes.Count > 0)
                {
                    XmlNode xmlNode2 = xmlNode.ChildNodes[0];
                    if (node.ChildNodes.Count != xmlNode2.ChildNodes.Count)
                    {
                        throw new EvaluationException(EvaluationException.ErrorIds.ParameterCountMismatch, new string[]
						{
							node.Attributes["name"].Value
						});
                    }
                    list2 = new List<Element>();
                    for (int i = 0; i < xmlNode2.ChildNodes.Count; i++)
                    {
                        XmlNode xmlNode3 = xmlNode2.ChildNodes[i];
                        if (!(xmlNode3.Name == "constant") && !(xmlNode3.Name == "source"))
                        {
                            Element element2 = new Element();
                            element2.Type = ElementType.Action;
                            element2.FuncType = FunctionType.Param;
                            element2.ParameterType = ParameterType.Input;
                            ESPLFormats.FillParamOperatorType(element2, xmlNode3);
                            element2.InpType = ((node.ChildNodes[i].Name == "property") ? InputType.Field : InputType.Input);
                            if (element2.InpType == InputType.Field)
                            {
                                element2.Value = node.ChildNodes[i].Attributes["name"].Value;
                            }
                            else
                            {
                                element2.Value = ESPL.Rule.Core.Encoder.Sanitize(node.ChildNodes[i].InnerText);
                                ESPLFormats.SetElementValues(element2, xmlNode3, true);
                            }
                            list2.Add(element2);
                        }
                    }
                }
            }
            else
            {
                list2 = null;
            }
            if (list2 != null && list2.Count > 0)
            {
                for (int j = 0; j < list2.Count; j++)
                {
                    if (j > 0)
                    {
                        list.Add(new Element
                        {
                            Type = ElementType.Action,
                            FuncType = FunctionType.Comma
                        });
                    }
                    list.Add(list2[j]);
                }
            }
            bool notFound = element.NotFound;
            element = new Element();
            element.NotFound = notFound;
            element.Type = ElementType.Action;
            element.FuncType = FunctionType.End;
            element.Token = (element.Value = value);
            list.Add(element);
        }

        private static void AppendConditionElements(List<Element> list, XmlNode source, Element oper, XmlNode fieldNode, XmlNode valueNode, string uiNamespace)
        {
            Element element = null;
            Element element2 = null;
            Element element3 = null;
            Element element4 = null;
            Element element5 = null;
            Element element6 = null;
            List<Element> list2 = null;
            List<Element> list3 = null;
            List<Element> list4 = null;
            string name;
            if ((name = fieldNode.Name) != null)
            {
                if (!(name == "rule"))
                {
                    if (!(name == "property"))
                    {
                        if (!(name == "method"))
                        {
                            goto IL_CD;
                        }
                        ESPLFormats.FillFunctionElements(out element, out list2, out element2, fieldNode, source, false, false);
                        oper.Oper = element.Oper;
                    }
                    else
                    {
                        element = new Element();
                        ESPLFormats.FillPropertyElement(element, fieldNode, source);
                        oper.Oper = element.Oper;
                    }
                }
                else
                {
                    element = new Element();
                    element.IsRule = true;
                    element.Type = ElementType.Field;
                    element.Value = fieldNode.Attributes["id"].Value;
                    element.Oper = (oper.Oper = OperatorType.Bool);
                }
                if (valueNode != null)
                {
                    string name2;
                    if ((name2 = valueNode.Name) != null)
                    {
                        if (name2 == "expression")
                        {
                            element6 = new Element();
                            element6.Type = ElementType.LeftBracket;
                            element5 = new Element();
                            element5.Type = ElementType.RightBracket;
                            list3 = new List<Element>();
                            ESPLFormats.AppendExpressionElements(list3, source, valueNode, uiNamespace);
                            goto IL_294;
                        }
                        if (name2 == "property")
                        {
                            element3 = new Element();
                            element3.Type = ElementType.Value;
                            element3.InpType = InputType.Field;
                            element3.Value = valueNode.Attributes["name"].Value;
                            element3.Oper = element.Oper;
                            goto IL_294;
                        }
                        if (name2 == "method")
                        {
                            ESPLFormats.FillFunctionElements(out element3, out list4, out element4, valueNode, source, true, false);
                            goto IL_294;
                        }
                        if (name2 == "value")
                        {
                            if (((XmlElement)valueNode).IsEmpty)
                            {
                                oper.Value = RuleValidator.GetNullableOperatorByOperType(element.Oper, RuleValidator.IsNegativeOperator(oper.Value));
                                goto IL_294;
                            }
                            element3 = new Element();
                            element3.Type = ElementType.Value;
                            element3.InpType = InputType.Input;
                            if (element.Oper == OperatorType.Collection)
                            {
                                XmlNode fieldByPropertyName = SourceLoader.GetFieldByPropertyName(source, element.Value);
                                element3.Oper = Converter.ClientStringToClientType(fieldByPropertyName.ChildNodes[0].Attributes["type"].Value);
                            }
                            else
                            {
                                element3.Oper = element.Oper;
                            }
                            OperatorType oper2 = element3.Oper;
                            if (oper2 == OperatorType.String)
                            {
                                element3.Value = ESPL.Rule.Core.Encoder.Sanitize(valueNode.InnerText);
                                goto IL_294;
                            }
                            element3.Value = valueNode.InnerText;
                            goto IL_294;
                        }
                    }
                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedChildNodeInCondition, new string[0]);
                }
            IL_294:
                list.Add(element);
                if (list2 != null && list2.Count > 0)
                {
                    ESPLFormats.AppendParameterElements(list, list2);
                }
                if (element2 != null)
                {
                    list.Add(element2);
                }
                list.Add(oper);
                if (element3 != null)
                {
                    list.Add(element3);
                }
                if (list4 != null && list4.Count > 0)
                {
                    ESPLFormats.AppendParameterElements(list, list4);
                }
                if (element4 != null)
                {
                    list.Add(element4);
                }
                if (element6 != null)
                {
                    list.Add(element6);
                    foreach (Element current in list3)
                    {
                        list.Add(current);
                    }
                    list.Add(element5);
                }
                return;
            }
        IL_CD:
            throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedChildNodeInCondition, new string[0]);
        }

        private static void AppendParameterElements(List<Element> list, List<Element> parameters)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                if (i > 0)
                {
                    list.Add(new Element
                    {
                        Type = ElementType.Function,
                        FuncType = FunctionType.Comma
                    });
                }
                list.Add(parameters[i]);
            }
        }

        private static void AppendExpressionElements(List<Element> list, XmlNode source, XmlNode parent, string uiNamespace)
        {
            foreach (XmlNode xmlNode in parent.ChildNodes)
            {
                string name;
                switch (name = xmlNode.Name)
                {
                    case "divide":
                    case "add":
                    case "multiply":
                    case "subtract":
                        {
                            bool flag = xmlNode.Attributes["block", uiNamespace] != null && xmlNode.Attributes["block", uiNamespace].Value == "true";
                            if (flag)
                            {
                                ESPLFormats.AppendExpressionElement(list, CalculationType.LeftParenthesis, null);
                            }
                            ESPLFormats.AppendExpressionElements(list, source, xmlNode, uiNamespace);
                            if (flag)
                            {
                                ESPLFormats.AppendExpressionElement(list, CalculationType.RightParenthesis, null);
                            }
                            if (xmlNode.NextSibling != null && parent.Name != "expression")
                            {
                                ESPLFormats.AppendExpressionElement(list, Converter.StringToCalculationType(parent.Name), null);
                            }
                            break;
                        }
                    case "method":
                        {
                            Element item = null;
                            Element item2 = null;
                            List<Element> list2 = null;
                            ESPLFormats.FillFunctionElements(out item, out list2, out item2, xmlNode, source, false, true);
                            list.Add(item);
                            if (list2 != null && list2.Count > 0)
                            {
                                ESPLFormats.AppendParameterElements(list, list2);
                            }
                            list.Add(item2);
                            if (xmlNode.NextSibling != null && parent.Name != "expression")
                            {
                                ESPLFormats.AppendExpressionElement(list, Converter.StringToCalculationType(parent.Name), null);
                            }
                            break;
                        }
                    case "property":
                        ESPLFormats.AppendExpressionElement(list, CalculationType.Field, xmlNode.Attributes["name"].Value);
                        if (xmlNode.NextSibling != null && parent.Name != "expression")
                        {
                            ESPLFormats.AppendExpressionElement(list, Converter.StringToCalculationType(parent.Name), null);
                        }
                        break;
                    case "value":
                        ESPLFormats.AppendExpressionElement(list, CalculationType.Number, xmlNode.InnerText);
                        if (xmlNode.NextSibling != null && parent.Name != "expression")
                        {
                            ESPLFormats.AppendExpressionElement(list, Converter.StringToCalculationType(parent.Name), null);
                        }
                        break;
                }
            }
        }

        private static void AppendExpressionElement(List<Element> list, CalculationType type, string value)
        {
            Element element = new Element();
            element.Type = ElementType.Calculation;
            element.CalType = type;
            element.Oper = OperatorType.Numeric;
            if (value != null)
            {
                element.Value = value;
            }
            list.Add(element);
        }

        private static void SetElementType(Element el, string typeAttribute)
        {
            ESPLFormats.SetElementType(el, typeAttribute, false);
        }

        private static void SetElementType(Element el, string typeAttribute, bool setReturnProperties)
        {
            string[] array = typeAttribute.Split(new string[]
			{
				","
			}, StringSplitOptions.RemoveEmptyEntries);
            if (setReturnProperties)
            {
                el.ReturnEnumClass = array[0].Trim();
            }
            else
            {
                el.Class = (el.En = array[0].Trim());
            }
            if (array.Length > 1)
            {
                for (int i = 1; i < array.Length; i++)
                {
                    if (setReturnProperties)
                    {
                        el.ReturnEnumAssembly = el.ReturnEnumAssembly + array[i].Trim() + ",";
                    }
                    else
                    {
                        el.Assembly = el.Assembly + array[i].Trim() + ",";
                    }
                }
                if (setReturnProperties)
                {
                    el.ReturnEnumAssembly.TrimEnd(new char[]
					{
						','
					});
                    return;
                }
                el.Assembly.TrimEnd(new char[]
				{
					','
				});
            }
        }

        private static void SetElementValues(Element el, XmlNode node, bool isParam)
        {
            switch (el.Oper)
            {
                case OperatorType.String:
                    if (!isParam || el.ParameterType == ParameterType.Input)
                    {
                        el.Max = new decimal?(new int?(int.Parse(node.Attributes["maxLength"].Value)).Value);
                        return;
                    }
                    break;
                case OperatorType.Numeric:
                    if (!isParam || el.ParameterType == ParameterType.Input)
                    {
                        el.Dec = (node.Attributes["allowDecimal"].Value == "true");
                        el.Min = new decimal?(decimal.Parse(node.Attributes["min"].Value));
                        el.Max = new decimal?(decimal.Parse(node.Attributes["max"].Value));
                    }
                    if (!isParam)
                    {
                        el.Cal = (node.Attributes["allowCalculation"].Value == "true");
                        return;
                    }
                    break;
                case OperatorType.Date:
                case OperatorType.Time:
                    if (!isParam || el.ParameterType == ParameterType.Input)
                    {
                        el.Format = node.Attributes["format"].Value;
                        return;
                    }
                    break;
                case OperatorType.Bool:
                case (OperatorType)5:
                    break;
                case OperatorType.Enum:
                    if (isParam || node.Attributes["class"] != null)
                    {
                        ESPLFormats.SetElementType(el, node.Attributes["class"].Value);
                    }
                    break;
                default:
                    return;
            }
        }

        private static void FillDefinition(List<Element> items, XmlDocument doc, string ruleID, string ruleName, string ruleDescription, XmlDocument sourceXml, string sourceName, RuleType mode, bool isCurrentRuleOfEvalType, bool isInitialRule, ref int index)
        {
            XmlElement xmlElement = doc.CreateElement("rule", Xml.RuleNamespace);
            xmlElement.SetAttribute("id", ruleID);
            xmlElement.SetAttribute("webrule", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            xmlElement.SetAttribute("utc", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffff"));
            XmlNode sourceNodeByToken = SourceLoader.GetSourceNodeByToken(sourceXml, sourceName);
            if (sourceNodeByToken == null)
            {
                throw new SourceException(SourceException.ErrorIds.SourceNodeNotFound, new string[]
				{
					sourceName
				});
            }
            if (sourceNodeByToken.Attributes["persisted"] == null || sourceNodeByToken.Attributes["persisted"].Value == "true")
            {
                xmlElement.SetAttribute("type", sourceNodeByToken.Attributes["type"].Value);
            }
            xmlElement.SetAttribute("eval", isCurrentRuleOfEvalType ? "true" : "false");
            if (!string.IsNullOrEmpty(ruleName))
            {
                XmlElement xmlElement2 = doc.CreateElement("name", Xml.RuleNamespace);
                xmlElement2.InnerText = ESPL.Rule.Core.Encoder.ClearXml(ruleName);
                xmlElement.AppendChild(xmlElement2);
            }
            if (!string.IsNullOrEmpty(ruleDescription))
            {
                XmlElement xmlElement3 = doc.CreateElement("description", Xml.RuleNamespace);
                xmlElement3.InnerText = ESPL.Rule.Core.Encoder.ClearXml(ruleDescription);
                xmlElement.AppendChild(xmlElement3);
            }
            XmlElement xmlElement4 = doc.CreateElement("definition", Xml.RuleNamespace);
            xmlElement.AppendChild(xmlElement4);
            XmlElement xmlElement5 = doc.CreateElement("format", Xml.RuleNamespace);
            XmlElement xmlElement6 = doc.CreateElement("lines", Xml.RuleNamespace);
            xmlElement5.AppendChild(xmlElement6);
            xmlElement.AppendChild(xmlElement5);
            doc.DocumentElement.AppendChild(xmlElement);
            ESPLFormats.FillFormatNodes(doc, xmlElement6, items, index);
            if (isInitialRule)
            {
                ESPLFormats.EnsureCalcOperatorPriorities(items);
            }
            if (isCurrentRuleOfEvalType)
            {
                ESPLFormats.FillEvaluationNodes(doc, xmlElement4, items, sourceNodeByToken, ref index);
                return;
            }
            ESPLFormats.FillExecutionNodes(doc, xmlElement4, items, sourceNodeByToken, mode, ref index);
        }

        private static void FillExecutionNodes(XmlDocument doc, XmlElement parent, List<Element> items, XmlNode source, RuleType mode, ref int index)
        {
            XmlElement xmlElement = null;
            XmlElement xmlElement2 = doc.CreateElement((mode == RuleType.Loop) ? "while" : "if", Xml.RuleNamespace);
            XmlElement xmlElement3 = doc.CreateElement("clause", Xml.RuleNamespace);
            xmlElement2.AppendChild(xmlElement3);
            parent.AppendChild(xmlElement2);
            ESPLFormats.FillEvaluationNodes(doc, xmlElement3, items, source, ref index);
            RuleAction ruleAction = null;
            while (index < items.Count)
            {
                Element element = items[index];
                ElementType type = element.Type;
                if (type != ElementType.Flow)
                {
                    if (type != ElementType.Action)
                    {
                        if (type == ElementType.Setter)
                        {
                            if (element.Value == "set")
                            {
                                RuleSetter setter = new RuleSetter();
                                if (xmlElement == null)
                                {
                                    xmlElement = doc.CreateElement("then", Xml.RuleNamespace);
                                    xmlElement2.AppendChild(xmlElement);
                                }
                                XmlElement xmlElement4 = doc.CreateElement("set", Xml.RuleNamespace);
                                ESPLFormats.FillSetterNode(doc, xmlElement4, setter, items, source, ref index);
                                xmlElement.AppendChild(xmlElement4);
                            }
                        }
                    }
                    else
                    {
                        switch (element.FuncType)
                        {
                            case FunctionType.Name:
                                ruleAction = new RuleAction
                                {
                                    Action = element
                                };
                                break;
                            case FunctionType.Param:
                                if (ruleAction != null)
                                {
                                    ruleAction.Parameters.Add(element);
                                }
                                break;
                            case FunctionType.End:
                                {
                                    if (xmlElement == null)
                                    {
                                        xmlElement = doc.CreateElement("then", Xml.RuleNamespace);
                                        xmlElement2.AppendChild(xmlElement);
                                    }
                                    XmlElement xmlElement4 = doc.CreateElement("method", Xml.RuleNamespace);
                                    ESPLFormats.FillMethodNode(doc, xmlElement4, ruleAction.Action, ruleAction.Parameters, source);
                                    xmlElement.AppendChild(xmlElement4);
                                    break;
                                }
                        }
                    }
                }
                else if (element.Value == "elseIf")
                {
                    if (mode == RuleType.Ruleset)
                    {
                        ESPLFormats.FillExecutionNodes(doc, parent, items, source, mode, ref index);
                    }
                    else
                    {
                        XmlElement xmlElement4 = doc.CreateElement("else", Xml.RuleNamespace);
                        xmlElement2.AppendChild(xmlElement4);
                        ESPLFormats.FillExecutionNodes(doc, xmlElement4, items, source, mode, ref index);
                    }
                }
                else
                {
                    xmlElement = doc.CreateElement("else", Xml.RuleNamespace);
                    xmlElement2.AppendChild(xmlElement);
                }
                index++;
            }
        }

        private static void FillEvaluationNodes(XmlDocument doc, XmlElement parent, List<Element> items, XmlNode source, ref int index)
        {
            Element element = ESPLFormats.PeekLevelClause(items, index);
            XmlElement xmlElement = null;
            if (element != null && element.Value != "then")
            {
                xmlElement = doc.CreateElement((element.Value == "and") ? "and" : "or", Xml.RuleNamespace);
            }
            if (xmlElement != null)
            {
                if (index > 0)
                {
                    Element element2 = items[index - 1];
                    if (element2.Type == ElementType.LeftParenthesis && element2.IsOrganicParenthesis)
                    {
                        xmlElement.SetAttribute("block", "http://codeeffects.com/schemas/ui/4", "true");
                    }
                }
                parent.AppendChild(xmlElement);
            }
            RuleCondition ruleCondition = null;
            bool flag = false;
            SelectionType type = SelectionType.None;
            while (index < items.Count)
            {
                Element element3 = items[index];
                ElementType type2 = element3.Type;
                switch (type2)
                {
                    case ElementType.Field:
                        if (ruleCondition == null)
                        {
                            ruleCondition = new RuleCondition();
                        }
                        ruleCondition.Field = element3;
                        if (element3.Oper == OperatorType.Collection && element3.CollType == CollectionType.Value)
                        {
                            SourceLoader.GetFieldByPropertyName(source, element3.Value);
                        }
                        break;
                    case ElementType.Function:
                        if (ruleCondition == null)
                        {
                            ruleCondition = new RuleCondition();
                        }
                        switch (element3.FuncType)
                        {
                            case FunctionType.Name:
                                if (element3.IsFuncValue)
                                {
                                    flag = true;
                                    ruleCondition.ValueParametersCount = SourceLoader.GetParamNode(SourceLoader.GetFunctionByToken(source, element3.Token)).ChildNodes.Count;
                                    ruleCondition.Value = element3;
                                }
                                else
                                {
                                    ruleCondition.Field = element3;
                                }
                                break;
                            case FunctionType.Param:
                                if (flag)
                                {
                                    ruleCondition.ValueParameters.Add(element3);
                                }
                                else
                                {
                                    ruleCondition.Parameters.Add(element3);
                                }
                                break;
                            case FunctionType.End:
                                flag = false;
                                break;
                        }
                        break;
                    case ElementType.Operator:
                        if (ruleCondition == null)
                        {
                            ruleCondition = new RuleCondition();
                        }
                        ruleCondition.Operator = element3;
                        break;
                    case ElementType.Value:
                        if (ruleCondition == null)
                        {
                            ruleCondition = new RuleCondition();
                        }
                        ruleCondition.Value = element3;
                        break;
                    case (ElementType)5:
                    case ElementType.Action:
                    case ElementType.RightBracket:
                    case ElementType.Calculation:
                    case ElementType.Tab:
                    case (ElementType)14:
                    case ElementType.NewLine:
                    case ElementType.HtmlTag:
                    case ElementType.Setter:
                        break;
                    case ElementType.Clause:
                        if (element3.Value == "then")
                        {
                            return;
                        }
                        break;
                    case ElementType.LeftParenthesis:
                        index++;
                        ESPLFormats.FillEvaluationNodes(doc, (xmlElement == null) ? parent : xmlElement, items, source, ref index);
                        break;
                    case ElementType.RightParenthesis:
                    case ElementType.RightSource:
                        return;
                    case ElementType.LeftBracket:
                        {
                            index++;
                            XmlElement xmlElement2 = doc.CreateElement("expression", Xml.RuleNamespace);
                            string numericTypeName = "numeric";
                            if (ruleCondition != null && ruleCondition.Field.Oper == OperatorType.Collection && ruleCondition.Field.CollType == CollectionType.Value)
                            {
                                XmlNode fieldByPropertyName = SourceLoader.GetFieldByPropertyName(source, ruleCondition.Field.Value);
                                if (fieldByPropertyName.Attributes["generic"].Value == "false" && fieldByPropertyName.Attributes["array"].Value == "false")
                                {
                                    numericTypeName = fieldByPropertyName.ChildNodes[0].Attributes["class"].Value;
                                }
                            }
                            ESPLFormats.FillCalculationNode(doc, xmlElement2, items, source, null, numericTypeName, ref index);
                            if (ruleCondition == null)
                            {
                                ruleCondition = new RuleCondition();
                            }
                            ruleCondition.Calculation = xmlElement2;
                            break;
                        }
                    case ElementType.LeftSource:
                        {
                            index++;
                            string text = Guid.NewGuid().ToString();
                            XmlElement xmlElement3 = doc.CreateElement("property", Xml.RuleNamespace);
                            XmlElement xmlElement4 = doc.CreateElement("rule", Xml.RuleNamespace);
                            xmlElement4.SetAttribute("id", text);
                            xmlElement4.SetAttribute("operator", Converter.SelectionTypeToString(type));
                            xmlElement3.SetAttribute("name", ruleCondition.Field.Value);
                            xmlElement4.AppendChild(xmlElement3);
                            XmlElement xmlElement5 = (xmlElement == null) ? parent : xmlElement;
                            xmlElement5.AppendChild(xmlElement4);
                            ESPLFormats.FillDefinition(items, doc, text, null, null, source.OwnerDocument, element3.Value, RuleType.Evaluation, true, false, ref index);
                            break;
                        }
                    default:
                        if (type2 == ElementType.Exists)
                        {
                            type = element3.SelType;
                        }
                        break;
                }
                if (ruleCondition != null && ruleCondition.Valid)
                {
                    ESPLFormats.AppendConditionNode(doc, (xmlElement == null) ? parent : xmlElement, ruleCondition, source);
                    ruleCondition = null;
                }
                index++;
            }
        }

        private static void FillSetterNode(XmlDocument doc, XmlElement el, RuleSetter setter, List<Element> items, XmlNode source, ref int index)
        {
            bool flag = false;
            while (index < items.Count)
            {
                Element element = items[index];
                switch (element.Type)
                {
                    case ElementType.Flow:
                    case ElementType.Clause:
                        if (element.Type == ElementType.Flow)
                        {
                            index--;
                        }
                        if (setter.Valid)
                        {
                            ESPLFormats.AppendSetterNode(doc, el, setter, source);
                        }
                        return;
                    case ElementType.Field:
                        setter.Field = element;
                        break;
                    case ElementType.Function:
                        switch (element.FuncType)
                        {
                            case FunctionType.Name:
                                if (element.IsFuncValue)
                                {
                                    flag = true;
                                    setter.ValueParametersCount = SourceLoader.GetParamNode(SourceLoader.GetFunctionByToken(source, element.Token)).ChildNodes.Count;
                                    setter.Value = element;
                                }
                                break;
                            case FunctionType.Param:
                                if (flag)
                                {
                                    setter.ValueParameters.Add(element);
                                }
                                break;
                            case FunctionType.End:
                                flag = false;
                                break;
                        }
                        break;
                    case ElementType.Value:
                        setter.Value = element;
                        break;
                    case ElementType.LeftBracket:
                        {
                            index++;
                            XmlElement xmlElement = doc.CreateElement("expression", Xml.RuleNamespace);
                            ESPLFormats.FillCalculationNode(doc, xmlElement, items, source, null, "numeric", ref index);
                            setter.Calculation = xmlElement;
                            break;
                        }
                }
                index++;
            }
            if (setter.Valid)
            {
                ESPLFormats.AppendSetterNode(doc, el, setter, source);
            }
        }

        private static void FillCalculationNode(XmlDocument doc, XmlElement parent, List<Element> items, XmlNode source, RuleCondition condition, string numericTypeName, ref int index)
        {
            Element element = ESPLFormats.PeekLevelMathOperator(items, index);
            XmlElement xmlElement = null;
            if (element != null)
            {
                xmlElement = doc.CreateElement(ESPLFormats.GetCalculationNodeName(element), Xml.RuleNamespace);
                Element element2 = items[index - 1];
                if (element2.CalType == CalculationType.LeftParenthesis && element2.IsOrganicParenthesis)
                {
                    xmlElement.SetAttribute("block", "http://codeeffects.com/schemas/ui/4", "true");
                }
                parent.AppendChild(xmlElement);
            }
            while (index < items.Count)
            {
                Element element3 = items[index];
                ElementType type = element3.Type;
                if (type != ElementType.Function)
                {
                    switch (type)
                    {
                        case ElementType.RightBracket:
                            return;
                        case ElementType.Calculation:
                            {
                                CalculationType calType = element3.CalType;
                                switch (calType)
                                {
                                    case CalculationType.Field:
                                        {
                                            XmlElement xmlElement2 = doc.CreateElement("property", Xml.RuleNamespace);
                                            xmlElement2.SetAttribute("name", element3.Value);
                                            if (xmlElement == null)
                                            {
                                                parent.AppendChild(xmlElement2);
                                            }
                                            else
                                            {
                                                xmlElement.AppendChild(xmlElement2);
                                            }
                                            break;
                                        }
                                    case CalculationType.LeftParenthesis:
                                        index++;
                                        ESPLFormats.FillCalculationNode(doc, (xmlElement == null) ? parent : xmlElement, items, source, condition, numericTypeName, ref index);
                                        break;
                                    case CalculationType.RightParenthesis:
                                        return;
                                    default:
                                        switch (calType)
                                        {
                                            case CalculationType.Number:
                                                {
                                                    XmlElement xmlElement2 = doc.CreateElement("value", Xml.RuleNamespace);
                                                    xmlElement2.SetAttribute("type", numericTypeName);
                                                    xmlElement2.InnerText = element3.Value;
                                                    if (xmlElement == null)
                                                    {
                                                        parent.AppendChild(xmlElement2);
                                                    }
                                                    else
                                                    {
                                                        xmlElement.AppendChild(xmlElement2);
                                                    }
                                                    break;
                                                }
                                            case CalculationType.Function:
                                                {
                                                    FunctionType funcType = element3.FuncType;
                                                    if (funcType != FunctionType.Name)
                                                    {
                                                        if (funcType == FunctionType.End)
                                                        {
                                                            if (condition == null)
                                                            {
                                                                throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
                                                            }
                                                            XmlElement xmlElement2 = doc.CreateElement("method", Xml.RuleNamespace);
                                                            ESPLFormats.FillMethodNode(doc, xmlElement2, condition.Field, condition.Parameters, source);
                                                            if (xmlElement == null)
                                                            {
                                                                parent.AppendChild(xmlElement2);
                                                            }
                                                            else
                                                            {
                                                                xmlElement.AppendChild(xmlElement2);
                                                            }
                                                            condition = null;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (condition == null)
                                                        {
                                                            condition = new RuleCondition();
                                                        }
                                                        condition.Field = element3;
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                }
                                break;
                            }
                    }
                }
                else
                {
                    if (condition == null)
                    {
                        throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
                    }
                    FunctionType funcType2 = element3.FuncType;
                    if (funcType2 == FunctionType.Param)
                    {
                        condition.Parameters.Add(element3);
                    }
                }
                index++;
            }
        }

        private static void FillMethodNode(XmlDocument doc, XmlElement el, Element method, List<Element> parameters, XmlNode source)
        {
            el.SetAttribute("name", method.Value);
            if (!string.IsNullOrEmpty(method.Class))
            {
                string text = method.Class;
                if (!string.IsNullOrEmpty(method.Assembly))
                {
                    text = text + ", " + method.Assembly;
                }
                el.SetAttribute("type", text);
            }
            if (parameters != null && parameters.Count > 0)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    Element element = parameters[i];
                    XmlElement xmlElement = null;
                    switch (element.ParameterType)
                    {
                        case ParameterType.Source:
                            xmlElement = doc.CreateElement("self", Xml.RuleNamespace);
                            break;
                        case ParameterType.Input:
                            if (element.InpType == InputType.Field)
                            {
                                xmlElement = doc.CreateElement("property", Xml.RuleNamespace);
                                xmlElement.SetAttribute("name", element.Value);
                            }
                            else
                            {
                                xmlElement = doc.CreateElement("value", Xml.RuleNamespace);
                                ESPLFormats.SetParamType(element, xmlElement, method, source, i);
                                ESPLFormats.FillNodeUiAttributes(xmlElement, element);
                                xmlElement.InnerText = ((element.Oper == OperatorType.String) ? ESPL.Rule.Core.Encoder.ClearXml(element.Value) : element.Value);
                            }
                            break;
                        case ParameterType.Constant:
                            xmlElement = doc.CreateElement("value", Xml.RuleNamespace);
                            ESPLFormats.SetParamType(element, xmlElement, method, source, i);
                            ESPLFormats.FillNodeUiAttributes(xmlElement, element);
                            xmlElement.InnerText = ((element.Oper == OperatorType.String) ? ESPL.Rule.Core.Encoder.ClearXml(element.Value) : element.Value);
                            break;
                    }
                    if (xmlElement != null)
                    {
                        el.AppendChild(xmlElement);
                    }
                }
            }
        }

        private static void SetParamType(Element parameter, XmlElement property, Element method, XmlNode source, int index)
        {
            if (parameter.Oper != OperatorType.String)
            {
                property.SetAttribute("type", ESPLFormats.GetParamType(method, source, index));
            }
        }

        private static void FillNodeUiAttributes(XmlElement el, Element field)
        {
            OperatorType oper = field.Oper;
            if (oper != OperatorType.Enum)
            {
                return;
            }
            string text;
            string text2;
            if ((field.Type == ElementType.Function || field.Type == ElementType.Calculation) && field.FuncType == FunctionType.Name)
            {
                text = field.ReturnEnumClass;
                text2 = field.ReturnEnumAssembly;
            }
            else
            {
                text = field.En;
                text2 = field.Assembly;
            }
            if (!string.IsNullOrWhiteSpace(text2))
            {
                text = text + ", " + text2;
            }
            el.SetAttribute("type", text);
        }

        private static void AppendConditionNode(XmlDocument doc, XmlElement parent, RuleCondition condition, XmlNode source)
        {
            XmlElement xmlElement = doc.CreateElement("condition", Xml.RuleNamespace);
            string value;
            if (RuleValidator.IsNullableOperator(condition.Operator.Value))
            {
                value = RuleValidator.GetNullableOperatorByOperType(condition.Field.Oper, RuleValidator.IsNegativeNullableOperator(condition.Operator.Value));
            }
            else
            {
                value = condition.Operator.Value;
            }
            xmlElement.SetAttribute("type", value);
            if (condition.Operator.Oper == OperatorType.String && condition.Value != null)
            {
                StringComparison stringComparison = (condition.Value.InpType == InputType.Input || condition.Value.InpType == InputType.None) ? condition.Field.StringComparison : condition.Value.StringComparison;
                stringComparison = ESPLFormats.GetComparison(condition.Field.StringComparison, stringComparison);
                xmlElement.SetAttribute("stringComparison", stringComparison.ToString());
            }
            switch (condition.Field.Type)
            {
                case ElementType.Field:
                    if (condition.Field.IsRule)
                    {
                        XmlElement xmlElement2 = doc.CreateElement("rule", Xml.RuleNamespace);
                        xmlElement2.SetAttribute("id", condition.Field.Value);
                        xmlElement.AppendChild(xmlElement2);
                    }
                    else
                    {
                        XmlElement xmlElement2 = doc.CreateElement("property", Xml.RuleNamespace);
                        xmlElement2.SetAttribute("name", condition.Field.Value);
                        if (condition.Field.Oper == OperatorType.Collection && condition.Field.CollType == CollectionType.Value)
                        {
                            XmlNode fieldByPropertyName = SourceLoader.GetFieldByPropertyName(source, condition.Field.Value);
                            if (fieldByPropertyName.Attributes["generic"].Value == "false" && fieldByPropertyName.Attributes["array"].Value == "false")
                            {
                                xmlElement2.SetAttribute("itemType", fieldByPropertyName.ChildNodes[0].Attributes["class"].Value);
                            }
                        }
                        xmlElement.AppendChild(xmlElement2);
                    }
                    break;
                case ElementType.Function:
                    {
                        XmlElement xmlElement2 = doc.CreateElement("method", Xml.RuleNamespace);
                        if (condition.Field.IsInstance)
                        {
                            xmlElement2.SetAttribute("instance", "true");
                        }
                        ESPLFormats.FillMethodNode(doc, xmlElement2, condition.Field, condition.Parameters, source);
                        xmlElement.AppendChild(xmlElement2);
                        break;
                    }
                default:
                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.InvalidOrderOfNodes, new string[0]);
            }
            if ((condition.Value != null || condition.Calculation != null) && !RuleValidator.IsNullableOperator(condition.Operator.Value))
            {
                if (condition.Calculation != null)
                {
                    xmlElement.AppendChild(condition.Calculation);
                }
                else if (condition.Value.Type == ElementType.Function)
                {
                    XmlElement xmlElement2 = doc.CreateElement("method", Xml.RuleNamespace);
                    if (condition.Value.IsInstance)
                    {
                        xmlElement2.SetAttribute("instance", "true");
                    }
                    ESPLFormats.FillMethodNode(doc, xmlElement2, condition.Value, condition.ValueParameters, source);
                    xmlElement.AppendChild(xmlElement2);
                }
                else
                {
                    XmlElement xmlElement2;
                    if (condition.Value.InpType == InputType.Field)
                    {
                        xmlElement2 = doc.CreateElement("property", Xml.RuleNamespace);
                        xmlElement2.SetAttribute("name", condition.Value.Value);
                    }
                    else
                    {
                        xmlElement2 = doc.CreateElement("value", Xml.RuleNamespace);
                        if (condition.Value.Oper != OperatorType.String)
                        {
                            string value2 = (condition.Operator.Oper == OperatorType.Numeric) ? "numeric" : ESPLFormats.GetTypeByFieldName(source, condition.Field);
                            xmlElement2.SetAttribute("type", value2);
                        }
                        ESPLFormats.FillNodeUiAttributes(xmlElement2, condition.Field);
                        xmlElement2.InnerText = ((condition.Field.Oper == OperatorType.String) ? ESPL.Rule.Core.Encoder.ClearXml(condition.Value.Value) : condition.Value.Value);
                    }
                    xmlElement.AppendChild(xmlElement2);
                }
            }
            parent.AppendChild(xmlElement);
        }

        private static void AppendSetterNode(XmlDocument doc, XmlElement parent, RuleSetter setter, XmlNode source)
        {
            XmlElement xmlElement = doc.CreateElement("property", Xml.RuleNamespace);
            xmlElement.SetAttribute("name", setter.Field.Value);
            parent.AppendChild(xmlElement);
            if (setter.Calculation != null)
            {
                parent.AppendChild(setter.Calculation);
                return;
            }
            if (setter.Value.Type == ElementType.Function)
            {
                xmlElement = doc.CreateElement("method", Xml.RuleNamespace);
                if (setter.Value.IsInstance)
                {
                    xmlElement.SetAttribute("instance", "true");
                }
                ESPLFormats.FillMethodNode(doc, xmlElement, setter.Value, setter.ValueParameters, source);
                parent.AppendChild(xmlElement);
                return;
            }
            if (setter.Value.InpType == InputType.Field)
            {
                xmlElement = doc.CreateElement("property", Xml.RuleNamespace);
                xmlElement.SetAttribute("name", setter.Value.Value);
            }
            else
            {
                xmlElement = doc.CreateElement("value", Xml.RuleNamespace);
                if (setter.Value.Oper != OperatorType.String)
                {
                    string value = (setter.Field.Oper == OperatorType.Numeric) ? "numeric" : ESPLFormats.GetTypeByFieldName(source, setter.Field);
                    xmlElement.SetAttribute("type", value);
                }
                ESPLFormats.FillNodeUiAttributes(xmlElement, setter.Field);
                xmlElement.InnerText = ((setter.Field.Oper == OperatorType.String) ? ESPL.Rule.Core.Encoder.ClearXml(setter.Value.Value) : setter.Value.Value);
            }
            parent.AppendChild(xmlElement);
        }

        private static void AppendLineNode(XmlDocument doc, XmlElement format, RuleLine line)
        {
            XmlElement xmlElement = doc.CreateElement("line", Xml.RuleNamespace);
            xmlElement.SetAttribute("index", line.Index.ToString());
            xmlElement.SetAttribute("tabs", line.Tabs.ToString());
            format.AppendChild(xmlElement);
        }

        private static string GetParamType(Element method, XmlNode source, int index)
        {
            if (method.Type == ElementType.Action)
            {
                return SourceLoader.GetParamNode(SourceLoader.GetActionByToken(source, method.Token)).ChildNodes[index].Attributes["class"].Value;
            }
            XmlNode functionByToken = SourceLoader.GetFunctionByToken(source, method.Token);
            XmlNode paramNode = SourceLoader.GetParamNode(functionByToken);
            XmlNode xmlNode = paramNode.ChildNodes[index];
            XmlAttribute xmlAttribute = xmlNode.Attributes["class"];
            return xmlAttribute.Value;
        }

        private static string GetTypeByFieldName(XmlNode source, Element field)
        {
            if (field.IsRule)
            {
                return typeof(bool).FullName;
            }
            if (field.Type == ElementType.Function)
            {
                return SourceLoader.GetReturnNode(SourceLoader.GetFunctionByToken(source, field.Token)).Attributes["class"].Value;
            }
            if (field.Oper == OperatorType.Collection && field.CollType == CollectionType.Value)
            {
                return SourceLoader.GetFieldByPropertyName(source, field.Value).ChildNodes[0].Attributes["class"].Value;
            }
            return SourceLoader.GetFieldByPropertyName(source, field.Value).Attributes["class"].Value;
        }

        private static Element PeekLevelClause(List<Element> items, int index)
        {
            int num = 0;
            for (int i = index; i < items.Count; i++)
            {
                Element element = items[i];
                if (element.Type == ElementType.LeftParenthesis || element.Type == ElementType.LeftSource)
                {
                    num++;
                }
                else if ((element.Type == ElementType.RightParenthesis || element.Type == ElementType.RightSource) && num > 0)
                {
                    num--;
                }
                else
                {
                    if ((element.Type == ElementType.RightParenthesis || element.Type == ElementType.RightSource) && num == 0)
                    {
                        break;
                    }
                    if (element.Type == ElementType.Clause && num == 0)
                    {
                        return element;
                    }
                }
            }
            return null;
        }

        private static Element PeekLevelMathOperator(List<Element> items, int index)
        {
            int num = 0;
            for (int i = index; i < items.Count; i++)
            {
                Element element = items[i];
                if (element.Type == ElementType.RightBracket)
                {
                    return null;
                }
                if (element.Type == ElementType.Calculation)
                {
                    if (element.CalType == CalculationType.LeftParenthesis)
                    {
                        num++;
                    }
                    else if (element.CalType == CalculationType.RightParenthesis && num > 0)
                    {
                        num--;
                    }
                    else
                    {
                        if (element.CalType == CalculationType.RightParenthesis && num == 0)
                        {
                            break;
                        }
                        if ((element.CalType == CalculationType.Addition || element.CalType == CalculationType.Division || element.CalType == CalculationType.Multiplication || element.CalType == CalculationType.Subtraction) && num == 0)
                        {
                            return element;
                        }
                    }
                }
            }
            return null;
        }

        private static string GetCalculationNodeName(Element el)
        {
            switch (el.CalType)
            {
                case CalculationType.Multiplication:
                    return "multiply";
                case CalculationType.Division:
                    return "divide";
                case CalculationType.Addition:
                    return "add";
                case CalculationType.Subtraction:
                    return "subtract";
            }
            throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid, new string[0]);
        }

        private static void EnsureCalcOperatorPriorities(List<Element> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                switch (items[i].CalType)
                {
                    case CalculationType.Multiplication:
                    case CalculationType.Division:
                        ESPLFormats.EnsureCalcParentheses(items, i);
                        break;
                }
            }
            bool flag = true;
            bool flag2 = false;
            for (int j = 0; j < items.Count; j++)
            {
                Element element = items[j];
                if (element.Type == ElementType.RightBracket)
                {
                    flag = true;
                }
                else
                {
                    CalculationType calType = element.CalType;
                    switch (calType)
                    {
                        case CalculationType.Field:
                            break;
                        case CalculationType.LeftParenthesis:
                            flag2 = true;
                            goto IL_11B;
                        default:
                            switch (calType)
                            {
                                case CalculationType.Number:
                                    break;
                                case CalculationType.None:
                                    goto IL_11B;
                                case CalculationType.Function:
                                    if (element.FuncType == FunctionType.Name && !flag && !flag2)
                                    {
                                        int indexForEndFunction = ESPLFormats.GetIndexForEndFunction(items, j);
                                        if (!ESPLFormats.RightParenthesisExists(items, indexForEndFunction + 1))
                                        {
                                            ESPLFormats.InsertParenthesis(items, indexForEndFunction + 1, CalculationType.RightParenthesis);
                                            ESPLFormats.InsertParenthesis(items, ESPLFormats.GetIndexForLeftParenthesis(items, j - 1), CalculationType.LeftParenthesis);
                                            j = indexForEndFunction + 1;
                                        }
                                    }
                                    else
                                    {
                                        flag = false;
                                    }
                                    flag2 = false;
                                    goto IL_11B;
                                default:
                                    goto IL_11B;
                            }
                            break;
                    }
                    if (!flag && !flag2)
                    {
                        if (!ESPLFormats.RightParenthesisExists(items, j + 1))
                        {
                            ESPLFormats.InsertParenthesis(items, j + 1, CalculationType.RightParenthesis);
                            ESPLFormats.InsertParenthesis(items, ESPLFormats.GetIndexForLeftParenthesis(items, j - 1), CalculationType.LeftParenthesis);
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                    flag2 = false;
                }
            IL_11B: ;
            }
            for (int k = 0; k < items.Count; k++)
            {
                switch (items[k].Type)
                {
                    case ElementType.LeftBracket:
                        ESPLFormats.InsertParenthesis(items, k + 1, CalculationType.LeftParenthesis);
                        break;
                    case ElementType.RightBracket:
                        ESPLFormats.InsertParenthesis(items, k - 1, CalculationType.RightParenthesis);
                        k++;
                        break;
                }
            }
        }

        private static void EnsureCalcParentheses(List<Element> items, int index)
        {
            int? prevCalcOperator = ESPLFormats.GetPrevCalcOperator(items, index - 1);
            int? nextCalcOperator = ESPLFormats.GetNextCalcOperator(items, index + 1);
            if (prevCalcOperator.HasValue && nextCalcOperator.HasValue)
            {
                ESPLFormats.InsertParenthesis(items, prevCalcOperator.Value + 1, CalculationType.LeftParenthesis);
                ESPLFormats.InsertParenthesis(items, nextCalcOperator.Value + 1, CalculationType.RightParenthesis);
            }
        }

        private static int? GetPrevCalcOperator(List<Element> items, int index)
        {
            for (int i = index; i > -1; i--)
            {
                Element element = items[i];
                if (element.Type == ElementType.LeftBracket)
                {
                    return new int?(i);
                }
                switch (element.CalType)
                {
                    case CalculationType.LeftParenthesis:
                    case CalculationType.RightParenthesis:
                        return null;
                    case CalculationType.Multiplication:
                    case CalculationType.Division:
                    case CalculationType.Addition:
                    case CalculationType.Subtraction:
                        return new int?(i);
                }
            }
            return null;
        }

        private static int? GetNextCalcOperator(List<Element> items, int index)
        {
            for (int i = index; i < items.Count; i++)
            {
                Element element = items[i];
                if (element.Type == ElementType.RightBracket)
                {
                    return new int?(i);
                }
                switch (element.CalType)
                {
                    case CalculationType.LeftParenthesis:
                    case CalculationType.RightParenthesis:
                        return null;
                    case CalculationType.Multiplication:
                    case CalculationType.Division:
                    case CalculationType.Addition:
                    case CalculationType.Subtraction:
                        return new int?(i);
                }
            }
            return new int?(items.Count);
        }

        private static void InsertParenthesis(List<Element> items, int index, CalculationType type)
        {
            items.Insert(index, new Element
            {
                Type = ElementType.Calculation,
                CalType = type,
                IsOrganicParenthesis = false
            });
        }

        private static bool RightParenthesisExists(List<Element> items, int index)
        {
            for (int i = index; i < items.Count; i++)
            {
                Element element = items[i];
                if (element.Type == ElementType.RightBracket)
                {
                    return false;
                }
                if (element.Type == ElementType.Calculation)
                {
                    return element.CalType == CalculationType.RightParenthesis;
                }
            }
            return false;
        }

        private static int GetIndexForEndFunction(List<Element> items, int index)
        {
            for (int i = index; i < items.Count; i++)
            {
                Element element = items[i];
                if (element.Type == ElementType.RightBracket)
                {
                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
                }
                if (element.Type == ElementType.Calculation && element.FuncType == FunctionType.End)
                {
                    return i;
                }
            }
            throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
        }

        private static int GetIndexForLeftParenthesis(List<Element> items, int index)
        {
            int num = 0;
            int i;
            for (i = index; i > -1; i--)
            {
                Element element = items[i];
                if (element.Type == ElementType.LeftBracket)
                {
                    return i + 1;
                }
                if (element.CalType == CalculationType.RightParenthesis)
                {
                    num++;
                }
                else if (element.CalType == CalculationType.LeftParenthesis)
                {
                    if (num > 0)
                    {
                        num--;
                    }
                    if (num == 0)
                    {
                        return i;
                    }
                }
                else if ((element.CalType == CalculationType.Number || element.CalType == CalculationType.Field || (element.CalType == CalculationType.Function && element.FuncType == FunctionType.Name)) && num == 0)
                {
                    return i;
                }
            }
            return i;
        }

        private static StringComparison GetComparison(StringComparison fieldOption, StringComparison valueOption)
        {
            switch (valueOption)
            {
                case StringComparison.CurrentCulture:
                    switch (fieldOption)
                    {
                        case StringComparison.CurrentCulture:
                        case StringComparison.CurrentCultureIgnoreCase:
                        case StringComparison.InvariantCulture:
                        case StringComparison.InvariantCultureIgnoreCase:
                            return StringComparison.CurrentCulture;
                        default:
                            return StringComparison.Ordinal;
                    }
                    break;
                case StringComparison.CurrentCultureIgnoreCase:
                    switch (fieldOption)
                    {
                        case StringComparison.CurrentCulture:
                        case StringComparison.InvariantCulture:
                            return StringComparison.CurrentCulture;
                        case StringComparison.CurrentCultureIgnoreCase:
                        case StringComparison.InvariantCultureIgnoreCase:
                            return StringComparison.CurrentCultureIgnoreCase;
                        case StringComparison.OrdinalIgnoreCase:
                            return StringComparison.OrdinalIgnoreCase;
                    }
                    return StringComparison.Ordinal;
                case StringComparison.InvariantCulture:
                    switch (fieldOption)
                    {
                        case StringComparison.CurrentCultureIgnoreCase:
                            return StringComparison.CurrentCulture;
                        case StringComparison.InvariantCultureIgnoreCase:
                            return StringComparison.InvariantCulture;
                        case StringComparison.OrdinalIgnoreCase:
                            return StringComparison.Ordinal;
                    }
                    return fieldOption;
                case StringComparison.Ordinal:
                    return StringComparison.Ordinal;
                case StringComparison.OrdinalIgnoreCase:
                    switch (fieldOption)
                    {
                        case StringComparison.CurrentCulture:
                        case StringComparison.InvariantCulture:
                        case StringComparison.Ordinal:
                            return StringComparison.Ordinal;
                    }
                    return StringComparison.OrdinalIgnoreCase;
            }
            return fieldOption;
        }

        private static void FillFormatNodes(XmlDocument doc, XmlElement parent, List<Element> items, int index)
        {
            List<Element> list = new List<Element>();
            int num = 0;
            int num2 = 0;
            int i = index;
            while (i < items.Count)
            {
                Element element = items[i];
                if (element.Type == ElementType.Function || element.Type == ElementType.Action || element.Type == ElementType.Calculation)
                {
                    if (element.FuncType != FunctionType.Param)
                    {
                        goto IL_79;
                    }
                    if (element.ParameterType != ParameterType.Source)
                    {
                        if (element.ParameterType != ParameterType.Constant)
                        {
                            goto IL_79;
                        }
                    }
                }
                else
                {
                    if (element.Type == ElementType.LeftSource)
                    {
                        num++;
                        goto IL_79;
                    }
                    if (element.Type != ElementType.RightSource)
                    {
                        goto IL_79;
                    }
                    num2++;
                    if (num2 <= num)
                    {
                        goto IL_79;
                    }
                    break;
                }
            IL_86:
                i++;
                continue;
            IL_79:
                list.Add(element.Clone());
                goto IL_86;
            }
            RuleLine ruleLine = null;
            bool flag = false;
            num2 = (num = 0);
            int num3 = (index == 0) ? -1 : 0;
            int j = 0;
            while (j < list.Count)
            {
                Element element2 = list[j];
                ElementType type = element2.Type;
                if (type == ElementType.Function || type == ElementType.Action)
                {
                    goto IL_140;
                }
                switch (type)
                {
                    case ElementType.Calculation:
                        goto IL_140;
                    case ElementType.Tab:
                        if (!flag && ruleLine != null)
                        {
                            ruleLine.Tabs++;
                            goto IL_1DA;
                        }
                        goto IL_1DA;
                    case ElementType.NewLine:
                        if (!flag)
                        {
                            if (ruleLine != null)
                            {
                                ESPLFormats.AppendLineNode(doc, parent, ruleLine);
                            }
                            ruleLine = new RuleLine(num3);
                            goto IL_1DA;
                        }
                        goto IL_1DA;
                    case ElementType.LeftSource:
                        if (ruleLine != null)
                        {
                            ESPLFormats.AppendLineNode(doc, parent, ruleLine);
                            ruleLine = null;
                        }
                        num++;
                        flag = true;
                        goto IL_1DA;
                    case ElementType.RightSource:
                        if (ruleLine != null)
                        {
                            ESPLFormats.AppendLineNode(doc, parent, ruleLine);
                            ruleLine = null;
                        }
                        num2++;
                        if (num2 >= num)
                        {
                            num2 = (num = 0);
                            flag = false;
                            goto IL_1DA;
                        }
                        goto IL_1DA;
                }
                if (!flag)
                {
                    if (ruleLine != null)
                    {
                        ESPLFormats.AppendLineNode(doc, parent, ruleLine);
                    }
                    ruleLine = null;
                }
            IL_1DA:
                j++;
                num3++;
                continue;
            IL_140:
                if (element2.Type == ElementType.Calculation && element2.CalType != CalculationType.Function)
                {
                    goto IL_1DA;
                }
                if (!flag)
                {
                    if (ruleLine != null)
                    {
                        ESPLFormats.AppendLineNode(doc, parent, ruleLine);
                    }
                    ruleLine = null;
                }
                if (element2.FuncType == FunctionType.Param && list[j + 1].FuncType != FunctionType.End)
                {
                    num3++;
                    goto IL_1DA;
                }
                goto IL_1DA;
            }
            if (ruleLine != null)
            {
                ESPLFormats.AppendLineNode(doc, parent, ruleLine);
            }
        }
    }
}
