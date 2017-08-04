using ESPL.Rule.Client;
using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Core
{
    internal sealed class RuleValidator
    {
        private static NumberStyles defaultNumericStyles
        {
            get
            {
                return NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
            }
        }

        private RuleValidator()
        {
        }

        internal static List<InvalidElement> Validate(List<Element> items, XmlDocument source, bool noActionsAllowed)
        {
            return RuleValidator.DoValidate(null, items, source, noActionsAllowed);
        }

        internal static List<InvalidElement> Validate(XmlDocument help, List<Element> items, XmlDocument source, bool noActionsAllowed)
        {
            return RuleValidator.DoValidate(help, items, source, noActionsAllowed);
        }

        internal static void EnsureTokens(List<Element> items, XmlDocument sourceXml)
        {
            XmlNode source = SourceLoader.GetSourceNode(sourceXml, (sourceXml.DocumentElement.ChildNodes[0].Attributes["name"] == null) ? null : sourceXml.DocumentElement.ChildNodes[0].Attributes["name"].Value);
            foreach (Element current in items)
            {
                if (current.Type == ElementType.LeftSource || current.Type == ElementType.RightSource)
                {
                    source = SourceLoader.GetSourceNodeByToken(sourceXml, current.Value);
                }
                else if (current.Type == ElementType.Function || current.Type == ElementType.Action || (current.Type == ElementType.Calculation && current.CalType == CalculationType.Function))
                {
                    if (current.FuncType != FunctionType.Name)
                    {
                        if (current.FuncType != FunctionType.End)
                        {
                            continue;
                        }
                    }
                    try
                    {
                        XmlNode xmlNode;
                        if (current.Type == ElementType.Action)
                        {
                            xmlNode = SourceLoader.GetActionByToken(source, current.Value);
                        }
                        else
                        {
                            xmlNode = SourceLoader.GetFunctionByToken(source, current.Value);
                        }
                        current.Token = current.Value;
                        current.Value = xmlNode.Attributes["methodName"].Value;
                    }
                    catch (SourceException)
                    {
                        if (string.IsNullOrWhiteSpace(current.Value))
                        {
                            current.Value = "Unknown method";
                        }
                        current.NotFound = true;
                    }
                }
            }
        }

        internal static void ForceTokens(List<Element> items)
        {
            foreach (Element current in items)
            {
                if ((current.Type == ElementType.Function || current.Type == ElementType.Action || (current.Type == ElementType.Calculation && current.CalType == CalculationType.Function)) && (current.FuncType == FunctionType.Name || current.FuncType == FunctionType.End) && !string.IsNullOrWhiteSpace(current.Token))
                {
                    current.Value = current.Token;
                }
            }
        }

        internal static void EnsureIgnoredProperties(List<Element> items, XmlDocument source)
        {
            RuleValidator.DoEnsureIgnoredProperties(items, source);
        }

        internal static void EnsureValues(List<Element> items)
        {
            RuleValidator.DoEnsureValues(items);
        }

        internal static void EnsureValuesForClient(List<Element> items)
        {
            int i = 0;
            while (i < items.Count)
            {
                Element element = items[i];
                ElementType type = element.Type;
                switch (type)
                {
                    case ElementType.Function:
                        goto IL_3A;
                    case ElementType.Operator:
                        break;
                    case ElementType.Value:
                        if (element.InpType != InputType.Field)
                        {
                            switch (element.Oper)
                            {
                                case OperatorType.Numeric:
                                    element.Value = RuleValidator.GetNumericDisplayString(element.Value);
                                    break;
                                case OperatorType.Date:
                                    element.Value = RuleValidator.GetDateValueString(element.Value);
                                    break;
                            }
                        }
                        break;
                    default:
                        if (type == ElementType.Action)
                        {
                            goto IL_3A;
                        }
                        if (type == ElementType.Calculation)
                        {
                            if (element.CalType == CalculationType.Number)
                            {
                                element.Value = RuleValidator.GetNumericDisplayString(element.Value);
                            }
                        }
                        break;
                }
            IL_F6:
                i++;
                continue;
            IL_3A:
                if (element.FuncType != FunctionType.Param || element.InpType == InputType.Field)
                {
                    goto IL_F6;
                }
                switch (element.Oper)
                {
                    case OperatorType.Numeric:
                        element.Value = RuleValidator.GetNumericDisplayString(element.Value);
                        goto IL_F6;
                    case OperatorType.Date:
                        element.Value = RuleValidator.GetDateValueString(element.Value);
                        goto IL_F6;
                    default:
                        goto IL_F6;
                }
            }
        }

        internal static List<InvalidElement> CheckRecursion(List<Element> items, string ruleXml, XmlDocument help, GetRuleDelegate getRule)
        {
            List<InvalidElement> list = new List<InvalidElement>();
            if (string.IsNullOrWhiteSpace(ruleXml) || items.Count == 0)
            {
                return list;
            }
            Stack<string> stack = new Stack<string>();
            bool flag = false;
            RecursionVisitor recursionVisitor = new RecursionVisitor(ruleXml, getRule);
            if (recursionVisitor.HasRecursion())
            {
                stack = recursionVisitor.RecursionStack;
                flag = true;
            }
            if (!flag)
            {
                return list;
            }
            foreach (Element current in items)
            {
                if (current.IsRule)
                {
                    foreach (string current2 in stack)
                    {
                        if (current.Value.ToLower() == current2.ToLower())
                        {
                            RuleValidator.AddInvalidElement(list, help, current.Name, "v135");
                            break;
                        }
                    }
                }
            }
            return list;
        }

        public static bool HasActions(List<Element> items)
        {
            foreach (Element current in items)
            {
                if (current.Type == ElementType.Action || current.Type == ElementType.Setter || (current.Type == ElementType.Clause && current.Value == "then") || (current.Type == ElementType.Flow && (current.Value == "else" || current.Value == "elseIf")))
                {
                    return true;
                }
            }
            return false;
        }

        internal static string GetValidationMessage(XmlDocument help, string tag)
        {
            if (help == null)
            {
                return tag;
            }
            foreach (XmlNode xmlNode in help.SelectSingleNode("/codeeffects/validation").ChildNodes)
            {
                if (xmlNode.NodeType != XmlNodeType.Comment && xmlNode.Name == tag)
                {
                    return xmlNode.InnerText;
                }
            }
            return "- unknown help tag (" + tag + ") -";
        }

        public static bool IsNullableOperator(string val)
        {
            return val == "isNotNull" || val == "isNull";
        }

        public static bool IsNegativeNullableOperator(string val)
        {
            return val == "isNotNull";
        }

        public static bool IsNegativeOperator(string val)
        {
            return val == "notEqual" || val == "doesNotContain" || val == "doesNotEndWith" || val == "doesNotStartWith";
        }

        public static string GetNullableOperatorByOperType(OperatorType type, bool negative)
        {
            switch (type)
            {
                case OperatorType.String:
                    if (!negative)
                    {
                        return "isNull";
                    }
                    return "isNotNull";
                case OperatorType.Numeric:
                    if (!negative)
                    {
                        return "isNull";
                    }
                    return "isNotNull";
                case OperatorType.Date:
                    if (!negative)
                    {
                        return "isNull";
                    }
                    return "isNotNull";
                case OperatorType.Time:
                    if (!negative)
                    {
                        return "isNull";
                    }
                    return "isNotNull";
                case OperatorType.Bool:
                    if (!negative)
                    {
                        return "isNull";
                    }
                    return "isNotNull";
                case OperatorType.Collection:
                    if (!negative)
                    {
                        return "isNull";
                    }
                    return "isNotNull";
            }
            throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownConditionType, new string[0]);
        }

        private static List<InvalidElement> DoValidate(XmlDocument help, List<Element> items, XmlDocument source, bool noActionsAllowed)
        {
            List<InvalidElement> list = new List<InvalidElement>();
            if (items.Count == 0)
            {
                return list;
            }
            Element nextElement = RuleValidator.GetNextElement(items, -1);
            if (nextElement == null || nextElement.Type != ElementType.Flow || nextElement.Value != "if")
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.FlowTypeElementExcpected, new string[0]);
            }
            Element nextElement2 = RuleValidator.GetNextElement(items, 0);
            if (nextElement2 == null)
            {
                RuleValidator.AddInvalidElement(list, help, nextElement.Name, "v101");
            }
            else if (nextElement2.Type == ElementType.Flow)
            {
                RuleValidator.AddInvalidElement(list, help, nextElement.Name, "v110");
            }
            else if (nextElement2.Type != ElementType.LeftParenthesis && nextElement2.Type != ElementType.Field && (nextElement2.Type != ElementType.Function || nextElement2.IsFuncValue) && nextElement2.Type != ElementType.Exists)
            {
                RuleValidator.AddInvalidElement(list, help, nextElement.Name, "v105");
            }
            int num = 1;
            RuleValidator.ValidateSourceSection(items, source, (source.DocumentElement.ChildNodes[0].Attributes["name"] == null) ? null : Encoder.GetHashToken(source.DocumentElement.ChildNodes[0].Attributes["name"].Value), help, noActionsAllowed, list, ref num);
            num = 0;
            RuleValidator.ValidateLevelClauses(items, list, help, ref num);
            return list;
        }

        private static void ValidateSourceSection(List<Element> items, XmlDocument sourceXml, string sourceName, XmlDocument help, bool noActionsAllowed, List<InvalidElement> list, ref int i)
        {
            XmlNode sourceNodeByToken = SourceLoader.GetSourceNodeByToken(sourceXml, sourceName);
            if (sourceNodeByToken == null)
            {
                throw new SourceException(SourceException.ErrorIds.SourceNodeNotFound, new string[]
				{
					sourceName
				});
            }
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = -1;
            int num5 = 0;
            Element element = null;
            Element element2 = null;
            Element element3 = null;
            Element element4 = null;
            Element element5 = null;
            decimal d = -79228162514264337593543950335m;
            decimal d2 = -79228162514264337593543950335m;
            decimal d3 = -79228162514264337593543950335m;
            DateTime minValue = DateTime.MinValue;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            XmlNode xmlNode = null;
            string text = null;
            string text2 = null;
            string token = null;
            while (i < items.Count)
            {
                element = items[i];
                if (element.Type != ElementType.HtmlTag && element.Type != ElementType.NewLine && element.Type != ElementType.Tab)
                {
                    element5 = RuleValidator.GetNextElement(items, i);
                    switch (element.Type)
                    {
                        case ElementType.Flow:
                            if (element.Value == "if")
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v123");
                            }
                            else if (element.Value == "else")
                            {
                                if (noActionsAllowed)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v144");
                                }
                                else if (flag)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v124");
                                }
                                else
                                {
                                    flag = true;
                                    if (element5 == null)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                    }
                                    else if (element5.Type == element.Type)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                    }
                                    else if (element5.Type != ElementType.Action && (element5.Type != ElementType.Setter || element5.Value != "set"))
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v106");
                                    }
                                }
                            }
                            else if (noActionsAllowed)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v144");
                            }
                            else if (element5 == null)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                            }
                            else if (element5.Type == element.Type)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                            }
                            else if (element5.Type != ElementType.LeftParenthesis && element5.Type != ElementType.Field && element5.Type != ElementType.Function && element5.Type != ElementType.Exists)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v105");
                            }
                            if (num != num2)
                            {
                                RuleValidator.AddInvalidElement(list, help, (num > num2) ? element2.Name : element3.Name, "v109");
                            }
                            element3 = (element2 = null);
                            num = 0;
                            num2 = 0;
                            goto IL_1B50;
                        case ElementType.Field:
                            token = (text = null);
                            text2 = element.Value;
                            flag3 = element.IsRule;
                            try
                            {
                                if (!element.IsRule)
                                {
                                    SourceLoader.GetFieldByPropertyName(sourceNodeByToken, element.Value);
                                }
                                if (element5 == null)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                }
                                else if (element5.Type == element.Type)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                }
                                else if (RuleValidator.IsInActionSection(items, i))
                                {
                                    if (element5.Type != ElementType.Setter || element5.Value != "to")
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v136");
                                    }
                                }
                                else if (element.Oper == OperatorType.Collection && element.CollType == CollectionType.Reference)
                                {
                                    if (element5.Type != ElementType.Where)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v143");
                                    }
                                }
                                else if (element5.Type != ElementType.Operator)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v102");
                                }
                                else if (element.Oper != element5.Oper)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v107");
                                }
                                goto IL_1B50;
                            }
                            catch (SourceException)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v133");
                                goto IL_1B50;
                            }
                            break;
                        case ElementType.Function:
                            flag3 = false;
                            RuleValidator.ValidateFunction(element, element5, list, sourceNodeByToken, help, ref text2, ref token, ref text, false, RuleValidator.IsInActionSection(items, i), noActionsAllowed, ref num4, ref num3, ref num5);
                            goto IL_1B50;
                        case ElementType.Operator:
                            break;
                        case ElementType.Value:
                            {
                                bool flag4 = RuleValidator.IsInActionSection(items, i);
                                if (element.InpType != InputType.Field)
                                {
                                    switch (element.Oper)
                                    {
                                        case OperatorType.String:
                                            if (string.IsNullOrEmpty(element.Value) && element4 != null && element4.Value != "is" && element4.Value != "isNot" && element4.Value != "equal" && element4.Value != "notEqual")
                                            {
                                                RuleValidator.AddInvalidElement(list, help, element4.Name, "v120");
                                            }
                                            if (element.Value.Length > element.Max)
                                            {
                                                RuleValidator.AddInvalidElement(list, help, element.Name, "v122");
                                            }
                                            break;
                                        case OperatorType.Numeric:
                                            if (!decimal.TryParse(element.Value, RuleValidator.defaultNumericStyles, Thread.CurrentThread.CurrentCulture, out d))
                                            {
                                                RuleValidator.AddInvalidElement(list, help, element.Name, "v119");
                                            }
                                            else if (sourceNodeByToken != null)
                                            {
                                                //TODO:
                                                //if (text != null)
                                            //    {
                                            //        try
                                            //        {
                                            //            xmlNode = SourceLoader.GetReturnNode(SourceLoader.GetFunctionByToken(sourceNodeByToken, token));
                                            //            d2 = decimal.Parse(xmlNode.Attributes["min"].Value);
                                            //            d3 = decimal.Parse(xmlNode.Attributes["max"].Value);
                                            //            goto IL_1179;
                                            //        }
                                            //        catch (SourceException)
                                            //        {
                                            //            RuleValidator.AddInvalidElement(list, help, element.Name, "v134");
                                            //            goto IL_1179;
                                            //        }
                                            //        catch
                                            //        {
                                            //            RuleValidator.AddInvalidElement(list, help, element.Name, "v131");
                                            //            goto IL_1179;
                                            //        }
                                            //        goto Block_103;
                                            //    }
                                            //    goto IL_1064;
                                            //IL_1179:
                                                if (d > d3 || d < d2)
                                                {
                                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v121");
                                                    break;
                                                }
                                                break;
                                            Block_103:
                                                try
                                                {
                                                IL_1064:
                                                    XmlNode xmlNode2 = SourceLoader.GetFieldByPropertyName(sourceNodeByToken, text2);
                                                    if (xmlNode2.Name == "collection")
                                                    {
                                                        if (!(xmlNode2.ChildNodes[0].Name == "value"))
                                                        {
                                                            throw new MalformedXmlException(MalformedXmlException.ErrorIds.ReferenceTypedCollectionNotAllowed, new string[]
												{
													text2
												});
                                                        }
                                                        d2 = decimal.Parse(xmlNode2.ChildNodes[0].Attributes["min"].Value);
                                                        d3 = decimal.Parse(xmlNode2.ChildNodes[0].Attributes["max"].Value);
                                                    }
                                                    else
                                                    {
                                                        d2 = decimal.Parse(xmlNode2.Attributes["min"].Value);
                                                        d3 = decimal.Parse(xmlNode2.Attributes["max"].Value);
                                                    }
                                                }
                                                catch (InvalidRuleException)
                                                {
                                                    throw;
                                                }
                                                catch (SourceException)
                                                {
                                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v134");
                                                }
                                                catch (Exception)
                                                {
                                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v131");
                                                }
                                                //TODO: goto IL_1179;
                                            }
                                            break;
                                        case OperatorType.Date:
                                            if (!DateTime.TryParse(element.Value, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out minValue))
                                            {
                                                RuleValidator.AddInvalidElement(list, help, element.Name, "v117");
                                            }
                                            break;
                                        case OperatorType.Time:
                                            if (!DateTime.TryParse("1/1/2010 " + element.Value, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out minValue))
                                            {
                                                RuleValidator.AddInvalidElement(list, help, element.Name, "v118");
                                            }
                                            break;
                                        case OperatorType.Enum:
                                            {
                                                Type type = null;
                                                try
                                                {
                                                    XmlNode xmlNode3 = SourceLoader.GetFieldByPropertyName(sourceNodeByToken, text2);
                                                    string str = (xmlNode3.Attributes["assembly"] == null) ? string.Empty : (", " + xmlNode3.Attributes["assembly"].Value);
                                                    type = Type.GetType(xmlNode3.Attributes["class"].Value + str, false);
                                                }
                                                catch (Exception ex)
                                                {
                                                    if (!(ex is SourceException) && !(ex is MalformedXmlException))
                                                    {
                                                        throw;
                                                    }
                                                    if (text != null)
                                                    {
                                                        XmlNode xmlNode3 = SourceLoader.GetReturnNode(SourceLoader.GetFunctionByToken(sourceNodeByToken, token));
                                                        string str2 = (xmlNode3.Attributes["assembly"] == null) ? string.Empty : (", " + xmlNode3.Attributes["assembly"].Value);
                                                        type = Type.GetType(xmlNode3.Attributes["class"].Value + str2, false);
                                                    }
                                                    else
                                                    {
                                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v145");
                                                    }
                                                }
                                                if (type != null)
                                                {
                                                    Type underlyingType = SourceLoader.GetUnderlyingType(type, null);
                                                    if (underlyingType != null)
                                                    {
                                                        type = underlyingType;
                                                    }
                                                }
                                                if (type == null || !Enum.IsDefined(type, Enum.Parse(type, element.Value)))
                                                {
                                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v145");
                                                }
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        SourceLoader.GetFieldByPropertyName(sourceNodeByToken, element.Value);
                                    }
                                    catch (SourceException)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v134");
                                    }
                                }
                                if (noActionsAllowed)
                                {
                                    if (flag4)
                                    {
                                        throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedValueSetters, new string[0]);
                                    }
                                    if (element5 == null)
                                    {
                                        goto IL_1B50;
                                    }
                                    if (element5.Type == element.Type)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                        goto IL_1B50;
                                    }
                                    if (element5.Type != ElementType.Clause && element5.Type != ElementType.RightParenthesis && element5.Type != ElementType.RightSource)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v104");
                                        goto IL_1B50;
                                    }
                                    goto IL_1B50;
                                }
                                else if (flag4)
                                {
                                    if (element5 == null)
                                    {
                                        goto IL_1B50;
                                    }
                                    if (element5.Type == element.Type)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                        goto IL_1B50;
                                    }
                                    if (element5.Type != ElementType.Clause && element5.Type != ElementType.Flow)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v137");
                                        goto IL_1B50;
                                    }
                                    if (element5.Type == ElementType.Clause && element5.Value != "and")
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v111");
                                        goto IL_1B50;
                                    }
                                    if (element5.Type == ElementType.Flow && (element5.Value != "elseIf" & element5.Value != "else"))
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v138");
                                        goto IL_1B50;
                                    }
                                    goto IL_1B50;
                                }
                                else
                                {
                                    if (element5 == null)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                        goto IL_1B50;
                                    }
                                    if (element5.Type == element.Type)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                        goto IL_1B50;
                                    }
                                    if (element5.Type != ElementType.Clause && element5.Type != ElementType.RightParenthesis && element5.Type != ElementType.RightSource)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v104");
                                        goto IL_1B50;
                                    }
                                    goto IL_1B50;
                                }
                                break;
                            }
                        case (ElementType)5:
                        case ElementType.Calculation:
                        case ElementType.Tab:
                        case (ElementType)14:
                        case ElementType.NewLine:
                        case ElementType.HtmlTag:
                        case (ElementType)21:
                            goto IL_1B50;
                        case ElementType.Clause:
                            if (element5 == null)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                goto IL_1B50;
                            }
                            if (element5.Type == element.Type)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                goto IL_1B50;
                            }
                            if (element.Value == "then" || RuleValidator.IsInActionSection(items, i))
                            {
                                if (noActionsAllowed)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v144");
                                    goto IL_1B50;
                                }
                                if (element5.Type != ElementType.Action && (element5.Type != ElementType.Setter || element5.Value != "set"))
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v106");
                                    goto IL_1B50;
                                }
                                goto IL_1B50;
                            }
                            else
                            {
                                if (element5.Type != ElementType.LeftParenthesis && element5.Type != ElementType.Field && element5.Type != ElementType.Function && element5.Type != ElementType.Exists)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v105");
                                    goto IL_1B50;
                                }
                                goto IL_1B50;
                            }
                            break;
                        case ElementType.Action:
                            flag3 = false;
                            if (noActionsAllowed)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v144");
                                goto IL_1B50;
                            }
                            switch (element.FuncType)
                            {
                                case FunctionType.Name:
                                    num4 = -1;
                                    num5 = 0;
                                    xmlNode = null;
                                    text2 = element.Value;
                                    token = element.Token;
                                    flag2 = true;
                                    text = (string.IsNullOrEmpty(element.Name) ? element.Value : element.Name);
                                    if (element5 == null)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                        goto IL_1B50;
                                    }
                                    if (element5.FuncType == element.FuncType)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                        goto IL_1B50;
                                    }
                                    if (element5.FuncType != FunctionType.Param && element5.FuncType != FunctionType.End)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v125");
                                        goto IL_1B50;
                                    }
                                    goto IL_1B50;
                                case FunctionType.Param:
                                    num4++;
                                    num5++;
                                    if (element5 == null)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                        goto IL_1B50;
                                    }
                                    if (element5.FuncType == element.FuncType)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                        goto IL_1B50;
                                    }
                                    if (element5.FuncType != FunctionType.Comma && element5.FuncType != FunctionType.End)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v127");
                                    }
                                    if (sourceNodeByToken != null)
                                    {
                                        xmlNode = SourceLoader.GetParamNode(SourceLoader.GetActionByToken(sourceNodeByToken, token));
                                        if (RuleValidator.GetInputParamDataType(xmlNode.ChildNodes, num4) != element.Oper)
                                        {
                                            RuleValidator.AddInvalidElement(list, help, element.Name, "v129");
                                        }
                                    }
                                    if (element.InpType != InputType.Input)
                                    {
                                        goto IL_1B50;
                                    }
                                    switch (element.Oper)
                                    {
                                        case OperatorType.String:
                                            if (element.Value.Length > element.Max)
                                            {
                                                RuleValidator.AddInvalidElement(list, help, element.Name, "v122");
                                                goto IL_1B50;
                                            }
                                            goto IL_1B50;
                                        case OperatorType.Numeric:
                                            if (!decimal.TryParse(element.Value, RuleValidator.defaultNumericStyles, Thread.CurrentThread.CurrentCulture, out d))
                                            {
                                                RuleValidator.AddInvalidElement(list, help, element.Name, "v119");
                                                goto IL_1B50;
                                            }
                                            if (d > element.Max || d < element.Min)
                                            {
                                                RuleValidator.AddInvalidElement(list, help, element.Name, "v121");
                                                goto IL_1B50;
                                            }
                                            goto IL_1B50;
                                        case OperatorType.Date:
                                            if (!DateTime.TryParse(element.Value, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out minValue))
                                            {
                                                RuleValidator.AddInvalidElement(list, help, element.Name, "v117");
                                                goto IL_1B50;
                                            }
                                            goto IL_1B50;
                                        case OperatorType.Time:
                                            if (!DateTime.TryParse("1/1/2010 " + element.Value, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out minValue))
                                            {
                                                RuleValidator.AddInvalidElement(list, help, element.Name, "v118");
                                                goto IL_1B50;
                                            }
                                            goto IL_1B50;
                                        case OperatorType.Bool:
                                        case (OperatorType)5:
                                            goto IL_1B50;
                                        case OperatorType.Enum:
                                            {
                                                Type type2 = null;
                                                if (text != null)
                                                {
                                                    if (xmlNode == null)
                                                    {
                                                        xmlNode = SourceLoader.GetParamNode(SourceLoader.GetActionByToken(sourceNodeByToken, token));
                                                    }
                                                    XmlNode paramByIndex = RuleValidator.GetParamByIndex(xmlNode, num5);
                                                    string str3 = (paramByIndex.Attributes["assembly"] == null) ? string.Empty : (", " + paramByIndex.Attributes["assembly"].Value);
                                                    type2 = Type.GetType(paramByIndex.Attributes["class"].Value + str3, false);
                                                }
                                                else
                                                {
                                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v145");
                                                }
                                                if (type2 == null || !Enum.IsDefined(type2, Enum.Parse(type2, element.Value)))
                                                {
                                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v145");
                                                    goto IL_1B50;
                                                }
                                                goto IL_1B50;
                                            }
                                        default:
                                            goto IL_1B50;
                                    }
                                    break;
                                case FunctionType.Comma:
                                    if (element5 == null)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                        goto IL_1B50;
                                    }
                                    if (element5.FuncType == element.FuncType)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                        goto IL_1B50;
                                    }
                                    if (element5.FuncType != FunctionType.Param)
                                    {
                                        RuleValidator.AddInvalidElement(list, help, element.Name, "v128");
                                        goto IL_1B50;
                                    }
                                    goto IL_1B50;
                                case FunctionType.End:
                                    num5 = (num3 = 0);
                                    xmlNode = null;
                                    if (sourceNodeByToken != null)
                                    {
                                        xmlNode = SourceLoader.GetParamNode(SourceLoader.GetActionByToken(sourceNodeByToken, token));
                                        num3 = ((xmlNode == null) ? 0 : RuleValidator.GetInputParamCount(xmlNode.ChildNodes));
                                        if (num3 != num4 + 1)
                                        {
                                            RuleValidator.AddInvalidElement(list, help, text, "v128");
                                            RuleValidator.AddInvalidElement(list, help, element.Name, "v128");
                                        }
                                    }
                                    if (element5 != null)
                                    {
                                        if (element5.Type == element.Type)
                                        {
                                            RuleValidator.AddInvalidElement(list, help, text, "v110");
                                            RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                        }
                                        else if (RuleValidator.IsInActionSection(items, i))
                                        {
                                            if ((element5.Type != ElementType.Clause || element5.Value != "and") && (element5.Type != ElementType.Flow || element5.Value == "if"))
                                            {
                                                RuleValidator.AddInvalidElement(list, help, text, "v137");
                                                RuleValidator.AddInvalidElement(list, help, element.Name, "v137");
                                            }
                                        }
                                        else if (element5.Type != ElementType.Clause || element5.Value != "and")
                                        {
                                            RuleValidator.AddInvalidElement(list, help, text, "v111");
                                            RuleValidator.AddInvalidElement(list, help, element.Name, "v111");
                                        }
                                        else if (element5.Type == ElementType.Flow && element5.Value == "if")
                                        {
                                            RuleValidator.AddInvalidElement(list, help, text, "v123");
                                            RuleValidator.AddInvalidElement(list, help, element.Name, "v123");
                                        }
                                    }
                                    num4 = -1;
                                    goto IL_1B50;
                                default:
                                    throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownDataType, new string[0]);
                            }
                            break;
                        case ElementType.LeftParenthesis:
                            if (element5 == null)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                            }
                            else if (element5.Type != ElementType.LeftParenthesis && element5.Type != ElementType.Field && element5.Type != ElementType.Function && element5.Type != ElementType.Exists)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v105");
                            }
                            num++;
                            if (element2 == null)
                            {
                                element2 = element;
                                goto IL_1B50;
                            }
                            goto IL_1B50;
                        case ElementType.RightParenthesis:
                        case ElementType.RightSource:
                            if (!noActionsAllowed)
                            {
                                if (element5 == null)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                }
                                else if (element5.Type != ElementType.RightParenthesis && element5.Type != ElementType.Clause && element5.Type != ElementType.RightSource)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v104");
                                }
                            }
                            else if (element5 != null && element5.Type != ElementType.RightParenthesis && element5.Type != ElementType.Clause && element5.Type != ElementType.RightSource)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v104");
                            }
                            if (element.Type == ElementType.RightSource)
                            {
                                return;
                            }
                            num2++;
                            element3 = element;
                            goto IL_1B50;
                        case ElementType.LeftBracket:
                            if (element5 == null || element5.Type == element.Type)
                            {
                                throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
                            }
                            if (element5.Type == ElementType.RightBracket)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v112");
                                goto IL_1B50;
                            }
                            if (element5.Type != ElementType.Calculation)
                            {
                                throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
                            }
                            i++;
                            RuleValidator.ValidateCalculation(items, list, sourceNodeByToken, help, ref i);
                            goto IL_1B50;
                        case ElementType.RightBracket:
                            if (RuleValidator.IsInActionSection(items, i))
                            {
                                if (element5 == null)
                                {
                                    goto IL_1B50;
                                }
                                if (element5.Type == element.Type)
                                {
                                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
                                }
                                if (element5.Type != ElementType.Clause && element5.Type != ElementType.Flow)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v137");
                                    goto IL_1B50;
                                }
                                if (element5.Type == ElementType.Clause && element5.Value != "and")
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v111");
                                    goto IL_1B50;
                                }
                                if (element5.Type == ElementType.Flow && (element5.Value != "elseIf" & element5.Value != "else"))
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v138");
                                    goto IL_1B50;
                                }
                                goto IL_1B50;
                            }
                            else if (!noActionsAllowed)
                            {
                                if (element5 == null)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                    goto IL_1B50;
                                }
                                if (element5.Type == element.Type)
                                {
                                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
                                }
                                if (element5.Type != ElementType.Clause && element5.Type != ElementType.RightParenthesis && element5.Type != ElementType.RightSource)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v104");
                                    goto IL_1B50;
                                }
                                goto IL_1B50;
                            }
                            else
                            {
                                if (element5 == null)
                                {
                                    goto IL_1B50;
                                }
                                if (element5.Type == element.Type)
                                {
                                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
                                }
                                if (element5.Type != ElementType.Clause && element5.Type != ElementType.RightParenthesis && element5.Type != ElementType.RightSource)
                                {
                                    RuleValidator.AddInvalidElement(list, help, element.Name, "v104");
                                    goto IL_1B50;
                                }
                                goto IL_1B50;
                            }
                            break;
                        case ElementType.Setter:
                            flag2 = true;
                            if (noActionsAllowed)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v144");
                                goto IL_1B50;
                            }
                            if (element5 == null)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                goto IL_1B50;
                            }
                            if (element5.Type == element.Type)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                goto IL_1B50;
                            }
                            if (element.Value == "set" && element5.Type != ElementType.Field)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v139");
                                goto IL_1B50;
                            }
                            if (element.Value == "to" && element5.Type != ElementType.Value && element5.Type != ElementType.LeftBracket && (element5.Type != ElementType.Function || element5.FuncType != FunctionType.Name))
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v140");
                                goto IL_1B50;
                            }
                            goto IL_1B50;
                        case ElementType.LeftSource:
                            if (element5 == null)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                            }
                            else if (element5.Type != ElementType.LeftParenthesis && element5.Type != ElementType.Field && element5.Type != ElementType.Function && element5.Type != ElementType.Exists)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v105");
                            }
                            i++;
                            RuleValidator.ValidateSourceSection(items, sourceXml, element.Value, help, true, list, ref i);
                            goto IL_1B50;
                        case ElementType.Where:
                            if (element5 == null)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                goto IL_1B50;
                            }
                            if (element5.Type == element.Type)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                goto IL_1B50;
                            }
                            if (element5.Type != ElementType.LeftSource)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v142");
                                goto IL_1B50;
                            }
                            goto IL_1B50;
                        case ElementType.Exists:
                            if (element5 == null)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                                goto IL_1B50;
                            }
                            if (element5.Type == element.Type)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                                goto IL_1B50;
                            }
                            if (element5.Type != ElementType.Field || element5.Oper != OperatorType.Collection || element5.CollType != CollectionType.Reference)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v141");
                                goto IL_1B50;
                            }
                            goto IL_1B50;
                        default:
                            goto IL_1B50;
                    }
                    element4 = null;
                    if (RuleValidator.IsNullableOperator(element.Value))
                    {
                        if (!noActionsAllowed)
                        {
                            if (element5 == null)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                            }
                            else if (element5.Type == element.Type)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                            }
                            else if (element5.Type != ElementType.Clause && element5.Type != ElementType.RightParenthesis && element5.Type != ElementType.RightSource)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v104");
                            }
                        }
                        else if (element5 != null)
                        {
                            if (element5.Type == element.Type)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                            }
                            else if (element5.Type != ElementType.Clause && element5.Type != ElementType.RightParenthesis && element5.Type != ElementType.RightSource)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v104");
                            }
                        }
                    }
                    else if (element5 == null)
                    {
                        RuleValidator.AddInvalidElement(list, help, element.Name, "v101");
                    }
                    else if (element5.Type == element.Type)
                    {
                        RuleValidator.AddInvalidElement(list, help, element.Name, "v110");
                    }
                    else if (element5.Type != ElementType.Value && element5.Type != ElementType.LeftBracket && (element5.Type != ElementType.Function || element5.FuncType != FunctionType.Name || !element5.IsFuncValue))
                    {
                        RuleValidator.AddInvalidElement(list, help, element.Name, "v103");
                    }
                    else
                    {
                        XmlNode xmlNode2 = (!string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2) || flag3) ? null : SourceLoader.GetFieldByPropertyName(sourceNodeByToken, text2);
                        if (element5.Type != ElementType.LeftBracket && xmlNode2 != null && xmlNode2.Name == "collection")
                        {
                            if (!(xmlNode2.ChildNodes[0].Name == "value"))
                            {
                                throw new MalformedXmlException(MalformedXmlException.ErrorIds.ReferenceTypedCollectionNotAllowed, new string[]
								{
									text2
								});
                            }
                        }
                        else if (element5.Type != ElementType.LeftBracket && element.Oper != element5.Oper)
                        {
                            RuleValidator.AddInvalidElement(list, help, element.Name, "v108");
                        }
                        else
                        {
                            element4 = element;
                        }
                    }
                }
            IL_1B50:
                i++;
            }
            if (!noActionsAllowed && !flag2)
            {
                RuleValidator.AddInvalidElement(list, help, items[items.Count - 1].Name, "v101");
            }
            if (num != num2)
            {
                RuleValidator.AddInvalidElement(list, help, (num > num2) ? element2.Name : element3.Name, "v109");
            }
        }

        private static void ValidateCalculation(List<Element> items, List<InvalidElement> invalids, XmlNode source, XmlDocument help, ref int i)
        {
            bool flag = false;
            int num = 0;
            int num2 = 0;
            int index = i - 1;
            int num3 = -1;
            int num4 = 0;
            int num5 = 0;
            Element nextElement = RuleValidator.GetNextElement(items, index);
            if (nextElement.Type != ElementType.Calculation || (nextElement.CalType != CalculationType.LeftParenthesis && (nextElement.CalType != CalculationType.Function || nextElement.FuncType != FunctionType.Name) && nextElement.CalType != CalculationType.Field && nextElement.CalType != CalculationType.Number))
            {
                RuleValidator.AddInvalidElement(invalids, help, nextElement.Name, "v116");
            }
            Element element = null;
            Element element2 = null;
            Element element3 = null;
            string text = null;
            string text2 = null;
            string text3 = null;
            while (i < items.Count)
            {
                element = items[i];
                if (element.Type == ElementType.RightBracket)
                {
                    flag = true;
                    break;
                }
                if (element.Type == ElementType.HtmlTag || element.Type == ElementType.NewLine || element.Type == ElementType.Tab || element.Type == ElementType.Flow)
                {
                    i++;
                }
                else
                {
                    Element nextElement2 = RuleValidator.GetNextElement(items, i);
                    if (nextElement2 == null)
                    {
                        throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
                    }
                    if (element.FuncType != FunctionType.Param && element.FuncType != FunctionType.Comma)
                    {
                        switch (element.CalType)
                        {
                            case CalculationType.Field:
                                try
                                {
                                    SourceLoader.GetFieldByPropertyName(source, element.Value);
                                    if (nextElement2.Type != ElementType.RightBracket && nextElement2.CalType != CalculationType.RightParenthesis && nextElement2.CalType != CalculationType.Addition && nextElement2.CalType != CalculationType.Multiplication && nextElement2.CalType != CalculationType.Division && nextElement2.CalType != CalculationType.Subtraction)
                                    {
                                        RuleValidator.AddInvalidElement(invalids, help, element.Name, "v114");
                                    }
                                    break;
                                }
                                catch (SourceException)
                                {
                                    RuleValidator.AddInvalidElement(invalids, help, element.Name, "v133");
                                    break;
                                }
                                goto IL_319;
                            case CalculationType.LeftParenthesis:
                                if (nextElement2.CalType != CalculationType.LeftParenthesis && nextElement2.CalType != CalculationType.Field && (nextElement2.CalType != CalculationType.Function || nextElement2.FuncType != FunctionType.Name) && nextElement2.CalType != CalculationType.Number)
                                {
                                    RuleValidator.AddInvalidElement(invalids, help, element.Name, "v115");
                                }
                                num++;
                                if (element2 == null)
                                {
                                    element2 = element;
                                }
                                break;
                            case CalculationType.RightParenthesis:
                                if (nextElement2.Type != ElementType.RightBracket && nextElement2.CalType != CalculationType.RightParenthesis && nextElement2.CalType != CalculationType.Addition && nextElement2.CalType != CalculationType.Multiplication && nextElement2.CalType != CalculationType.Division && nextElement2.CalType != CalculationType.Subtraction)
                                {
                                    RuleValidator.AddInvalidElement(invalids, help, element.Name, "v114");
                                }
                                num2++;
                                element3 = element;
                                break;
                            case CalculationType.Multiplication:
                            case CalculationType.Division:
                            case CalculationType.Addition:
                            case CalculationType.Subtraction:
                                if (nextElement2.CalType != CalculationType.Field && (nextElement2.CalType != CalculationType.Function || nextElement2.FuncType != FunctionType.Name) && nextElement2.CalType != CalculationType.Number && nextElement2.CalType != CalculationType.LeftParenthesis)
                                {
                                    RuleValidator.AddInvalidElement(invalids, help, element.Name, "v113");
                                }
                                break;
                            case (CalculationType)5:
                            case CalculationType.None:
                                goto IL_414;
                            case CalculationType.Number:
                                goto IL_319;
                            case CalculationType.Function:
                                RuleValidator.ValidateFunction(element, nextElement2, invalids, source, help, ref text2, ref text3, ref text, true, false, true, ref num3, ref num5, ref num4);
                                break;
                            default:
                                goto IL_414;
                        }
                    IL_422:
                        i++;
                        continue;
                    IL_319:
                        decimal d = -79228162514264337593543950335m;
                        if (!decimal.TryParse(element.Value, RuleValidator.defaultNumericStyles, Thread.CurrentThread.CurrentCulture, out d))
                        {
                            RuleValidator.AddInvalidElement(invalids, help, element.Name, "v119");
                        }
                        else if (d > element.Max || d < element.Min)
                        {
                            RuleValidator.AddInvalidElement(invalids, help, element.Name, "v121");
                        }
                        if (nextElement2.Type != ElementType.RightBracket && nextElement2.CalType != CalculationType.RightParenthesis && nextElement2.CalType != CalculationType.Addition && nextElement2.CalType != CalculationType.Multiplication && nextElement2.CalType != CalculationType.Division && nextElement2.CalType != CalculationType.Subtraction)
                        {
                            RuleValidator.AddInvalidElement(invalids, help, element.Name, "v114");
                            goto IL_422;
                        }
                        goto IL_422;
                    IL_414:
                        throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
                    }
                    RuleValidator.ValidateFunction(element, nextElement2, invalids, source, help, ref text2, ref text3, ref text, true, false, true, ref num3, ref num5, ref num4);
                    i++;
                }
            }
            if (!flag)
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedOrderOfCalculationNodes, new string[0]);
            }
            if (num != num2)
            {
                RuleValidator.AddInvalidElement(invalids, help, (num > num2) ? element2.Name : element3.Name, "v109");
            }
            i--;
        }

        private static void ValidateLevelClauses(List<Element> items, List<InvalidElement> list, XmlDocument help, ref int index)
        {
            Element element = null;
            while (index < items.Count)
            {
                Element element2 = items[index];
                ElementType type = element2.Type;
                if (type != ElementType.Flow)
                {
                    switch (type)
                    {
                        case ElementType.Clause:
                            if (element2.Value == "then")
                            {
                                return;
                            }
                            if (element == null)
                            {
                                element = element2;
                                goto IL_E1;
                            }
                            if (element2.Value != element.Value)
                            {
                                RuleValidator.AddInvalidElement(list, help, element.Name, "v132");
                                RuleValidator.AddInvalidElement(list, help, element2.Name, "v132");
                                goto IL_E1;
                            }
                            goto IL_E1;
                        case ElementType.Action:
                            goto IL_E1;
                        case ElementType.LeftParenthesis:
                            goto IL_7E;
                        case ElementType.RightParenthesis:
                            break;
                        default:
                            switch (type)
                            {
                                case ElementType.LeftSource:
                                    goto IL_7E;
                                case ElementType.RightSource:
                                    break;
                                default:
                                    goto IL_E1;
                            }
                            break;
                    }
                    return;
                IL_7E:
                    index++;
                    RuleValidator.ValidateLevelClauses(items, list, help, ref index);
                }
                else if (element2.Value == "if" || element2.Value == "elseIf")
                {
                    index++;
                    RuleValidator.ValidateLevelClauses(items, list, help, ref index);
                }
            IL_E1:
                index++;
            }
        }

        private static void ValidateFunction(Element item, Element next, List<InvalidElement> invalids, XmlNode source, XmlDocument help, ref string lastPropertyName, ref string lastToken, ref string lastFunctionId, bool calculation, bool actionSection, bool noActionsAllowed, ref int index, ref int count, ref int paramCount)
        {
            XmlNode xmlNode = null;
            DateTime minValue = DateTime.MinValue;
            decimal d = -79228162514264337593543950335m;
            switch (item.FuncType)
            {
                case FunctionType.Name:
                    if (calculation && item.Type != ElementType.Calculation)
                    {
                        throw new InvalidRuleException(InvalidRuleException.ErrorIds.InvalidOrderOfNodes, new string[0]);
                    }
                    index = -1;
                    paramCount = 0;
                    lastPropertyName = item.Value;
                    lastToken = item.Token;
                    try
                    {
                        SourceLoader.GetFunctionByToken(source, lastToken);
                        lastFunctionId = (string.IsNullOrEmpty(item.Name) ? item.Value : item.Name);
                        if (next == null)
                        {
                            RuleValidator.AddInvalidElement(invalids, help, item.Name, "v101");
                        }
                        else if (next.FuncType == item.FuncType)
                        {
                            RuleValidator.AddInvalidElement(invalids, help, item.Name, "v110");
                        }
                        else if (next.FuncType != FunctionType.Param && next.FuncType != FunctionType.End)
                        {
                            RuleValidator.AddInvalidElement(invalids, help, item.Name, "v125");
                        }
                        return;
                    }
                    catch (SourceException)
                    {
                        item.NotFound = true;
                        RuleValidator.AddInvalidElement(invalids, help, item.Name, "v133");
                        return;
                    }
                    break;
                case FunctionType.Param:
                    break;
                case FunctionType.Comma:
                    if (next == null)
                    {
                        RuleValidator.AddInvalidElement(invalids, help, item.Name, "v101");
                        return;
                    }
                    if (next.FuncType == item.FuncType)
                    {
                        RuleValidator.AddInvalidElement(invalids, help, item.Name, "v110");
                        return;
                    }
                    if (next.FuncType != FunctionType.Param)
                    {
                        RuleValidator.AddInvalidElement(invalids, help, item.Name, "v128");
                        return;
                    }
                    return;
                case FunctionType.End:
                    if (calculation && item.Type != ElementType.Calculation)
                    {
                        throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnecpectedCalculationType, new string[0]);
                    }
                    count = (paramCount = 0);
                    xmlNode = null;
                    if (source != null)
                    {
                        try
                        {
                            xmlNode = SourceLoader.GetParamNode(SourceLoader.GetFunctionByToken(source, lastToken));
                        }
                        catch (SourceException)
                        {
                            xmlNode = null;
                        }
                        count = ((xmlNode == null) ? 0 : RuleValidator.GetInputParamCount(xmlNode.ChildNodes));
                    }
                    if (item.IsFuncValue)
                    {
                        if (actionSection)
                        {
                            if (next != null)
                            {
                                if (next.Type != ElementType.Clause && next.Type != ElementType.Flow)
                                {
                                    RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v137");
                                    RuleValidator.AddInvalidElement(invalids, help, item.Name, "v137");
                                }
                                else if (next.Type == ElementType.Clause && next.Value != "and")
                                {
                                    RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v111");
                                    RuleValidator.AddInvalidElement(invalids, help, item.Name, "v111");
                                }
                                else if (next.Type == ElementType.Flow && (next.Value != "elseIf" & next.Value != "else"))
                                {
                                    RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v138");
                                    RuleValidator.AddInvalidElement(invalids, help, item.Name, "v138");
                                }
                            }
                        }
                        else if (!noActionsAllowed)
                        {
                            if (next == null)
                            {
                                RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v101");
                                RuleValidator.AddInvalidElement(invalids, help, item.Name, "v101");
                            }
                            else if (next.Type == item.Type)
                            {
                                RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v110");
                                RuleValidator.AddInvalidElement(invalids, help, item.Name, "v110");
                            }
                            else if (next.Type != ElementType.Clause && next.Type != ElementType.RightParenthesis && next.Type != ElementType.RightSource)
                            {
                                RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v104");
                                RuleValidator.AddInvalidElement(invalids, help, item.Name, "v104");
                            }
                        }
                        else if (next != null)
                        {
                            if (next.Type == item.Type)
                            {
                                RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v110");
                                RuleValidator.AddInvalidElement(invalids, help, item.Name, "v110");
                            }
                            else if (next.Type != ElementType.Clause && next.Type != ElementType.RightParenthesis && next.Type != ElementType.RightSource)
                            {
                                RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v104");
                                RuleValidator.AddInvalidElement(invalids, help, item.Name, "v104");
                            }
                        }
                    }
                    else if (next == null)
                    {
                        RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v101");
                        RuleValidator.AddInvalidElement(invalids, help, item.Name, "v101");
                    }
                    else if (next.FuncType == item.FuncType)
                    {
                        RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v110");
                        RuleValidator.AddInvalidElement(invalids, help, item.Name, "v110");
                    }
                    else if (calculation)
                    {
                        if (next.Type != ElementType.RightBracket && next.CalType != CalculationType.RightParenthesis && next.CalType != CalculationType.Addition && next.CalType != CalculationType.Multiplication && next.CalType != CalculationType.Division && next.CalType != CalculationType.Subtraction)
                        {
                            RuleValidator.AddInvalidElement(invalids, help, item.Name, "v114");
                        }
                    }
                    else if (next.Type != ElementType.Operator)
                    {
                        RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v102");
                        RuleValidator.AddInvalidElement(invalids, help, item.Name, "v102");
                    }
                    else if (item.Oper != next.Oper)
                    {
                        RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v107");
                        RuleValidator.AddInvalidElement(invalids, help, item.Name, "v107");
                    }
                    if (xmlNode != null && count != index + 1)
                    {
                        RuleValidator.AddInvalidElement(invalids, help, lastFunctionId, "v128");
                        RuleValidator.AddInvalidElement(invalids, help, item.Name, "v128");
                    }
                    index = -1;
                    return;
                default:
                    return;
            }
            index++;
            paramCount++;
            if (next == null)
            {
                RuleValidator.AddInvalidElement(invalids, help, item.Name, "v101");
                return;
            }
            if (next.FuncType == item.FuncType)
            {
                RuleValidator.AddInvalidElement(invalids, help, item.Name, "v110");
                return;
            }
            if (next.FuncType != FunctionType.Comma && next.FuncType != FunctionType.End)
            {
                RuleValidator.AddInvalidElement(invalids, help, item.Name, "v127");
            }
            if (source != null)
            {
                try
                {
                    xmlNode = SourceLoader.GetParamNode(SourceLoader.GetFunctionByToken(source, lastToken));
                    if (RuleValidator.GetInputParamDataType(xmlNode.ChildNodes, index) != item.Oper)
                    {
                        RuleValidator.AddInvalidElement(invalids, help, item.Name, "v129");
                    }
                }
                catch (SourceException)
                {
                    xmlNode = null;
                }
            }
            if (xmlNode != null && item.InpType == InputType.Input)
            {
                switch (item.Oper)
                {
                    case OperatorType.String:
                        if (item.Value.Length > item.Max)
                        {
                            RuleValidator.AddInvalidElement(invalids, help, item.Name, "v122");
                            return;
                        }
                        break;
                    case OperatorType.Numeric:
                        if (!decimal.TryParse(item.Value, RuleValidator.defaultNumericStyles, Thread.CurrentThread.CurrentCulture, out d))
                        {
                            RuleValidator.AddInvalidElement(invalids, help, item.Name, "v119");
                            return;
                        }
                        if (d > item.Max || d < item.Min)
                        {
                            RuleValidator.AddInvalidElement(invalids, help, item.Name, "v121");
                            return;
                        }
                        break;
                    case OperatorType.Date:
                        if (!DateTime.TryParse(item.Value, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out minValue))
                        {
                            RuleValidator.AddInvalidElement(invalids, help, item.Name, "v117");
                            return;
                        }
                        break;
                    case OperatorType.Time:
                        if (!DateTime.TryParse("1/1/2010 " + item.Value, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out minValue))
                        {
                            RuleValidator.AddInvalidElement(invalids, help, item.Name, "v118");
                            return;
                        }
                        break;
                    case OperatorType.Bool:
                    case (OperatorType)5:
                        break;
                    case OperatorType.Enum:
                        {
                            Type type = null;
                            if (lastFunctionId != null)
                            {
                                XmlNode paramByIndex = RuleValidator.GetParamByIndex(xmlNode, paramCount);
                                string str = (paramByIndex.Attributes["assembly"] == null) ? string.Empty : (", " + paramByIndex.Attributes["assembly"].Value);
                                type = Type.GetType(paramByIndex.Attributes["class"].Value + str, false);
                            }
                            else
                            {
                                RuleValidator.AddInvalidElement(invalids, help, item.Name, "v145");
                            }
                            if (type == null || !Enum.IsDefined(type, Enum.Parse(type, item.Value)))
                            {
                                RuleValidator.AddInvalidElement(invalids, help, item.Name, "v145");
                                return;
                            }
                            break;
                        }
                    default:
                        return;
                }
            }
        }

        private static void DoEnsureIgnoredProperties(List<Element> items, XmlDocument sourceXml)
        {
            XmlNode source = SourceLoader.GetSourceNode(sourceXml, (sourceXml.DocumentElement.ChildNodes[0].Attributes["name"] == null) ? null : sourceXml.DocumentElement.ChildNodes[0].Attributes["name"].Value);
            XmlNode xmlNode = null;
            Element element = null;
            int i = 0;
            while (i < items.Count)
            {
                element = items[i];
                ElementType type = element.Type;
                switch (type)
                {
                    case ElementType.Field:
                        if (!element.IsRule)
                        {
                            try
                            {
                                xmlNode = SourceLoader.GetFieldByPropertyName(source, element.Value);
                                OperatorType oper = element.Oper;
                                if (oper != OperatorType.String)
                                {
                                    switch (oper)
                                    {
                                        case OperatorType.Enum:
                                            if (element.En != null)
                                            {
                                                element.Assembly = xmlNode.Attributes["assembly"].Value;
                                            }
                                            break;
                                        case OperatorType.Collection:
                                            if (xmlNode.ChildNodes[0].Name == "value" && Converter.ClientStringToClientType(xmlNode.ChildNodes[0].Attributes["type"].Value) == OperatorType.Enum && element.En != null)
                                            {
                                                element.Assembly = xmlNode.ChildNodes[0].Attributes["assembly"].Value;
                                            }
                                            break;
                                    }
                                }
                                else if (xmlNode.Attributes["stringComparison"] != null)
                                {
                                    element.StringComparison = (StringComparison)Enum.Parse(typeof(StringComparison), xmlNode.Attributes["stringComparison"].Value, true);
                                }
                                break;
                            }
                            catch (SourceException)
                            {
                                break;
                            }
                            goto IL_4EC;
                        }
                        break;
                    case ElementType.Function:
                    case ElementType.Action:
                        goto IL_C4;
                    case ElementType.Operator:
                    case (ElementType)5:
                    case ElementType.Clause:
                        break;
                    case ElementType.Value:
                        goto IL_4EC;
                    default:
                        if (type == ElementType.Calculation)
                        {
                            goto IL_C4;
                        }
                        switch (type)
                        {
                            case ElementType.LeftSource:
                            case ElementType.RightSource:
                                source = SourceLoader.GetSourceNodeByToken(sourceXml, element.Value);
                                break;
                        }
                        break;
                }
            IL_550:
                i++;
                continue;
            IL_C4:
                FunctionType funcType = element.FuncType;
                if (funcType == FunctionType.Name)
                {
                    xmlNode = null;
                    if (element.Type == ElementType.Function)
                    {
                        //TODO:goto IL_E9;
                    }
                    if (element.Type == ElementType.Calculation)
                    {
                        goto Block_7;
                    }
                //TODO:goto IL_FB;
                IL_10D:
                    if (xmlNode != null && xmlNode.Attributes["class"] != null)
                    {
                        element.Class = xmlNode.Attributes["class"].Value;
                        if (xmlNode.Attributes["assembly"] != null)
                        {
                            element.Assembly = xmlNode.Attributes["assembly"].Value;
                        }
                    }
                    if (xmlNode != null && (element.Type == ElementType.Function || element.Type == ElementType.Calculation))
                    {
                        XmlNode returnNode = SourceLoader.GetReturnNode(xmlNode);
                        string value;
                        if ((value = returnNode.Attributes["type"].Value) != null)
                        {
                            if (value == "bool")
                            {
                                element.Oper = OperatorType.Bool;
                                goto IL_550;
                            }
                            if (value == "date")
                            {
                                element.Oper = OperatorType.Date;
                                element.Format = Encoder.Desanitize(returnNode.Attributes["format"].Value);
                                goto IL_550;
                            }
                            if (value == "time")
                            {
                                element.Oper = OperatorType.Time;
                                element.Format = Encoder.Desanitize(returnNode.Attributes["format"].Value);
                                goto IL_550;
                            }
                            if (value == "enum")
                            {
                                element.Oper = OperatorType.Enum;
                                element.ReturnEnumClass = returnNode.Attributes["class"].Value;
                                element.ReturnEnumAssembly = returnNode.Attributes["assembly"].Value;
                                goto IL_550;
                            }
                            if (value == "string")
                            {
                                element.Oper = OperatorType.String;
                                element.Max = new decimal?(long.Parse(returnNode.Attributes["maxLength"].Value));
                                if (returnNode.Attributes["stringComparison"] != null)
                                {
                                    element.StringComparison = (StringComparison)Enum.Parse(typeof(StringComparison), returnNode.Attributes["stringComparison"].Value, true);
                                    goto IL_550;
                                }
                                goto IL_550;
                            }
                        }
                        element.Oper = OperatorType.Numeric;
                        element.Max = new decimal?(long.Parse(returnNode.Attributes["max"].Value));
                        element.Min = new decimal?(long.Parse(returnNode.Attributes["min"].Value));
                        element.Dec = (returnNode.Attributes["allowDecimal"].Value == "true");
                        element.Cal = (returnNode.Attributes["allowCalculation"].Value == "true");
                        goto IL_550;
                    }
                    goto IL_550;
                Block_8:
                    try
                    {
                    IL_FB:
                        xmlNode = SourceLoader.GetActionByToken(source, element.Token);
                    }
                    catch (SourceException)
                    {
                    }
                    goto IL_10D;
                Block_7:
                    try
                    {
                    IL_E9:
                        xmlNode = SourceLoader.GetFunctionByToken(source, element.Token);
                        goto IL_10D;
                    }
                    catch (SourceException)
                    {
                        goto IL_10D;
                    }
                    goto Block_8;
                }
                goto IL_550;
            IL_4EC:
                if (element.InpType == InputType.Field)
                {
                    try
                    {
                        xmlNode = SourceLoader.GetFieldByPropertyName(source, element.Value);
                        if (element.Oper == OperatorType.String && xmlNode.Attributes["stringComparison"] != null)
                        {
                            element.StringComparison = (StringComparison)Enum.Parse(typeof(StringComparison), xmlNode.Attributes["stringComparison"].Value, true);
                        }
                    }
                    catch (SourceException)
                    {
                    }
                    goto IL_550;
                }
                goto IL_550;
            }
            RuleValidator.DoEnsureIgnoredParameters(items, sourceXml);
        }

        private static void DoEnsureIgnoredParameters(List<Element> items, XmlDocument sourceXml)
        {
            XmlNode source = SourceLoader.GetSourceNode(sourceXml, (sourceXml.DocumentElement.ChildNodes[0].Attributes["name"] == null) ? null : sourceXml.DocumentElement.ChildNodes[0].Attributes["name"].Value);
            Dictionary<int, Element> dictionary = new Dictionary<int, Element>();
            Element element = null;
            XmlNode function = null;
            XmlNode xmlNode = null;
            string token = null;
            int num = 0;
            int i = 0;
            while (i < items.Count)
            {
                num = ((num == 0) ? (i + 1) : (++num));
                element = items[i];
                ElementType type = element.Type;
                if (type <= ElementType.Action)
                {
                    if (type == ElementType.Function || type == ElementType.Action)
                    {
                        goto IL_D6;
                    }
                }
                else
                {
                    if (type == ElementType.Calculation)
                    {
                        goto IL_D6;
                    }
                    switch (type)
                    {
                        case ElementType.LeftSource:
                        case ElementType.RightSource:
                            source = SourceLoader.GetSourceNodeByToken(sourceXml, element.Value);
                            break;
                    }
                }
            IL_3D7:
                i++;
                continue;
            IL_D6:
                switch (element.FuncType)
                {
                    case FunctionType.Name:
                        token = ((element.Token == null) ? element.Value : element.Token);
                        if (element.Type == ElementType.Function)
                        {
                            try
                            {
                                function = SourceLoader.GetFunctionByToken(source, token);
                                goto IL_137;
                            }
                            catch (SourceException)
                            {
                                function = null;
                                goto IL_137;
                            }
                            goto Block_11;
                        }
                    //TODO:goto IL_125;
                    IL_137:
                        xmlNode = SourceLoader.GetParamNode(function);
                        if (xmlNode != null)
                        {
                            IEnumerator enumerator = xmlNode.ChildNodes.GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    XmlNode xmlNode2 = (XmlNode)enumerator.Current;
                                    string name;
                                    if ((name = xmlNode2.Name) != null)
                                    {
                                        if (!(name == "source"))
                                        {
                                            if (!(name == "constant"))
                                            {
                                                if (name == "input" || name == "collection")
                                                {
                                                    num++;
                                                }
                                            }
                                            else
                                            {
                                                dictionary.Add(num, new Element
                                                {
                                                    Type = element.Type,
                                                    FuncType = FunctionType.Param,
                                                    ParameterType = ParameterType.Constant,
                                                    Oper = Converter.ClientStringToClientType(xmlNode2.Attributes["type"].Value),
                                                    Value = xmlNode2.Attributes["value"].Value
                                                });
                                                num++;
                                            }
                                        }
                                        else
                                        {
                                            dictionary.Add(num, new Element
                                            {
                                                Type = element.Type,
                                                FuncType = FunctionType.Param,
                                                ParameterType = ParameterType.Source
                                            });
                                            num++;
                                        }
                                    }
                                }
                                goto IL_3D7;
                            break;
                        }
                        goto IL_3D7;
                    Block_11:
                        try
                        {
                        IL_125:
                            function = SourceLoader.GetActionByToken(source, token);
                        }
                        catch (SourceException)
                        {
                            function = null;
                        }
                        goto IL_137;
                    case FunctionType.Param:
                        break;
                    default:
                        goto IL_3D7;
                }
                element.ParameterType = ParameterType.Input;
                OperatorType oper = element.Oper;
                if (oper == OperatorType.Enum)
                {
                    if (element.InpType == InputType.Field)
                    {
                        xmlNode = SourceLoader.GetFieldByPropertyName(source, element.Value);
                    }
                    else
                    {
                        try
                        {
                            if (element.Type == ElementType.Action)
                            {
                                function = SourceLoader.GetActionByToken(source, token);
                            }
                            else
                            {
                                function = SourceLoader.GetFunctionByToken(source, token);
                            }
                            foreach (XmlNode xmlNode3 in SourceLoader.GetParamNode(function).ChildNodes)
                            {
                                if (xmlNode3.NodeType != XmlNodeType.Comment && xmlNode3.Name == "input" && xmlNode3.Attributes["type"].Value == "enum" && xmlNode3.Attributes["class"].Value == element.En)
                                {
                                    xmlNode = xmlNode3;
                                    break;
                                }
                            }
                        }
                        catch (SourceException)
                        {
                        }
                    }
                    element.Class = xmlNode.Attributes["class"].Value;
                    if (xmlNode.Attributes["assembly"] != null)
                    {
                        element.Assembly = xmlNode.Attributes["assembly"].Value;
                    }
                }
                num--;
                goto IL_3D7;
            }
            foreach (int current in dictionary.Keys)
            {
                items.Insert(current, dictionary[current]);
            }
        }

        private static void DoEnsureValues(List<Element> items)
        {
            Element element = null;
            for (int i = 0; i < items.Count; i++)
            {
                Element element2 = items[i];
                ElementType type = element2.Type;
                switch (type)
                {
                    case ElementType.Field:
                        element = element2;
                        break;
                    case ElementType.Function:
                    case ElementType.Action:
                        if (element2.FuncType == FunctionType.Name)
                        {
                            element = element2;
                        }
                        else if (element2.FuncType == FunctionType.Param && element2.InpType == InputType.Input)
                        {
                            switch (element2.Oper)
                            {
                                case OperatorType.Numeric:
                                case OperatorType.Enum:
                                    element2.Value = RuleValidator.TrimNumeric(element2.Value);
                                    break;
                                case OperatorType.Date:
                                    element2.Value = RuleValidator.TrimDate(element2.Value);
                                    break;
                                case OperatorType.Time:
                                    element2.Value = RuleValidator.TrimTime(element2.Value, element2.Format);
                                    break;
                            }
                        }
                        break;
                    case ElementType.Operator:
                    case (ElementType)5:
                    case ElementType.Clause:
                        break;
                    case ElementType.Value:
                        if (element2.InpType != InputType.Field)
                        {
                            switch (element2.Oper)
                            {
                                case OperatorType.Numeric:
                                    element2.Value = RuleValidator.TrimNumeric(element2.Value);
                                    break;
                                case OperatorType.Date:
                                    element2.Value = RuleValidator.TrimDate(element2.Value);
                                    break;
                                case OperatorType.Time:
                                    element2.Value = RuleValidator.TrimTime(element2.Value, element.Format);
                                    break;
                                case OperatorType.Enum:
                                    if (element.Type == ElementType.Function || element.Type == ElementType.Action || (element.Type == ElementType.Field && element.En != null))
                                    {
                                        element2.Value = RuleValidator.TrimNumeric(element2.Value);
                                    }
                                    break;
                            }
                        }
                        element = null;
                        break;
                    default:
                        if (type == ElementType.Calculation)
                        {
                            if (element2.CalType == CalculationType.Number)
                            {
                                element2.Value = RuleValidator.TrimNumeric(element2.Value);
                            }
                        }
                        break;
                }
            }
        }

        private static Element GetNextElement(List<Element> items, int index)
        {
            for (int i = index + 1; i < items.Count; i++)
            {
                Element element = items[i];
                if (element.Type != ElementType.NewLine && element.Type != ElementType.HtmlTag && element.Type != ElementType.Tab)
                {
                    return element;
                }
            }
            return null;
        }

        private static bool IsInActionSection(List<Element> items, int index)
        {
            for (int i = index; i > -1; i--)
            {
                Element element = items[i];
                if (element.Type == ElementType.Operator || element.Type == ElementType.LeftParenthesis || element.Type == ElementType.RightParenthesis || (element.Type == ElementType.Flow && element.Value != "else"))
                {
                    return false;
                }
                if (element.Type == ElementType.Setter || element.Type == ElementType.Action || (element.Type == ElementType.Clause && element.Value == "then") || (element.Type == ElementType.Flow && element.Value == "else"))
                {
                    return true;
                }
            }
            return false;
        }

        private static XmlNode GetParamByIndex(XmlNode pars, int index)
        {
            int num = 0;
            foreach (XmlNode xmlNode in pars.ChildNodes)
            {
                if (xmlNode.Name == "input" || xmlNode.Name == "collection")
                {
                    num++;
                }
                if (num == index)
                {
                    return xmlNode;
                }
            }
            throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnexpectedNumberOfParameters, new string[0]);
        }

        private static int GetInputParamCount(XmlNodeList pars)
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

        private static int GetInputParamCount(List<Parameter> pars)
        {
            int num = 0;
            foreach (Parameter current in pars)
            {
                if (current.Type == ParameterType.Input)
                {
                    num++;
                }
            }
            return num;
        }

        private static OperatorType GetInputParamDataType(XmlNodeList pars, int index)
        {
            int num = -1;
            for (int i = 0; i < pars.Count; i++)
            {
                XmlNode xmlNode = pars[i];
                if (xmlNode.Name == "input" || xmlNode.Name == "collection")
                {
                    num++;
                }
                if (num == index)
                {
                    return Converter.ClientStringToClientType((xmlNode.Name == "collection") ? xmlNode.Name : xmlNode.Attributes["type"].Value);
                }
            }
            return OperatorType.None;
        }

        private static void AddInvalidElement(List<InvalidElement> list, XmlDocument help, string clientID, string tag)
        {
            if (!RuleValidator.ClientIdExists(list, clientID))
            {
                list.Add(new InvalidElement(clientID, RuleValidator.GetValidationMessage(help, tag)));
            }
        }

        private static bool ClientIdExists(List<InvalidElement> list, string clientID)
        {
            return list.Find((InvalidElement el) => el.ClientID == clientID) != null;
        }

        private static string TrimNumeric(string numeric)
        {
            string text = numeric.Replace(",", ".");
            if (text.Length > 1 && text.StartsWith("0"))
            {
                text = text.Substring(1, text.Length - 1);
            }
            if (text.Length > 1 && text.StartsWith("."))
            {
                text = "0" + text;
            }
            if (text == "." || text == "-." || text == "-0")
            {
                text = "0";
            }
            if (text.Length > 1 && text.EndsWith("."))
            {
                text = text.Substring(0, text.Length - 1);
            }
            return text;
        }

        private static string TrimDate(string date)
        {
            return DateTime.Parse(date, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None).ToString("yyyy-MM-ddTHH:mm:ss.ffff");
        }

        private static string TrimTime(string time, string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                format = "HH:mm:ss";
            }
            return DateTime.Parse("1/1/2000 " + time, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None).ToString(format.Contains("s") ? "HH:mm:ss" : "HH:mm");
        }

        private static string GetNumericDisplayString(string value)
        {
            return decimal.Parse(value.Replace(",", "."), CultureInfo.CreateSpecificCulture("en-US")).ToString("G", Thread.CurrentThread.CurrentCulture);
        }

        private static string GetDateValueString(string value)
        {
            return DateTime.Parse(value).ToString("M/d/yyyy H:m:s", CultureInfo.CreateSpecificCulture("en-US"));
        }
    }
}
