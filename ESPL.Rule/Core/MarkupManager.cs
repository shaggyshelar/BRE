using ESPL.Rule.Client;
using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    internal sealed class MarkupManager
    {
        private MarkupManager()
        {
        }

        internal static string RenderInitials()
        {
            List<string> list = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\r\n\r\n/* Code Effects business rules engine. Visit CodeEffects.com for support information. */\r\n\r\n");
            MarkupManager.RegisterNamespace(typeof(Element).Namespace, list, stringBuilder);
            MarkupManager.RegisterNamespace(typeof(RuleModel).Namespace, list, stringBuilder);
            stringBuilder.Append(MarkupManager.Localize(typeof(Element).FullName)).Append("=function(){};");
            stringBuilder.Append(MarkupManager.Localize(typeof(RuleModel).FullName)).Append("=function(){};");
            MarkupManager.RegisterLocalEnum(typeof(ElementType), stringBuilder, list);
            MarkupManager.RegisterLocalEnum(typeof(OperatorType), stringBuilder, list);
            MarkupManager.RegisterLocalEnum(typeof(FunctionType), stringBuilder, list);
            MarkupManager.RegisterLocalEnum(typeof(InputType), stringBuilder, list);
            MarkupManager.RegisterLocalEnum(typeof(CalculationType), stringBuilder, list);
            MarkupManager.RegisterLocalEnum(typeof(ValueInputType), stringBuilder, list);
            MarkupManager.RegisterLocalEnum(typeof(CollectionType), stringBuilder, list);
            MarkupManager.RegisterLocalEnum(typeof(SelectionType), stringBuilder, list);
            return stringBuilder.ToString();
        }

        internal static string RenderControlInstance(string controlServerID, string controlClientID, string hiddenFieldClientID, bool isWinXP, bool isAsp)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("$rule.Context.addControl('").Append(controlServerID).Append("',new $rule.Control(");
            stringBuilder.Append("[\"").Append(controlClientID).Append("\",");
            stringBuilder.Append(isWinXP ? "true," : "false,");
            stringBuilder.Append(isAsp ? "true," : "false,");
            if (string.IsNullOrEmpty(hiddenFieldClientID))
            {
                stringBuilder.Append("null]");
            }
            else
            {
                stringBuilder.Append("\"").Append(hiddenFieldClientID).Append("\"]");
            }
            stringBuilder.Append("));");
            return stringBuilder.ToString();
        }

        internal static string RenderHelp(List<Pair> helpMessages)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("$rule.Help={");
            bool flag = true;
            foreach (Pair current in helpMessages)
            {
                if (!flag)
                {
                    stringBuilder.Append(",");
                }
                else
                {
                    flag = false;
                }
                stringBuilder.Append(current.ID).Append(":'").Append(current.Name.Replace("\"", "&quot;").Replace("'", "&#39;")).Append("'");
            }
            stringBuilder.Append("};");
            return stringBuilder.ToString();
        }

        internal static string RenderErrors(List<Pair> errorMessages)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("$rule.Errors={");
            bool flag = true;
            foreach (Pair current in errorMessages)
            {
                if (!flag)
                {
                    stringBuilder.Append(",");
                }
                else
                {
                    flag = false;
                }
                stringBuilder.Append(current.ID).Append(":'").Append(current.Name.Replace("\"", "&quot;").Replace("'", "&#39;")).Append("'");
            }
            stringBuilder.Append("};");
            return stringBuilder.ToString();
        }

        internal static string RenderSettings(IControl control, MarkupData conditions, bool includeInitialization)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (includeInitialization)
            {
                stringBuilder.Append("$ce('").Append(conditions.ControlServerID).Append("').init('");
            }
            stringBuilder.Append("{ui:[");
            stringBuilder.Append(control.ShowHelpString ? "true" : "false").Append(",");
            stringBuilder.Append(conditions.IsLoadedRuleOfEvalType ? "true" : "false").Append(",\"");
            stringBuilder.Append(conditions.Settings.Labels.EvaluationIf).Append("\",\"");
            stringBuilder.Append(conditions.Settings.Labels.ExecutionIf).Append("\",");
            stringBuilder.Append(control.ShowToolBar ? "true" : "false").Append(",");
            if (!control.ClientOnly)
            {
                switch (conditions.Pattern)
                {
                    case Pattern.Asp:
                        {
                            string text = conditions.PostBackFunctionName;
                            int startIndex = text.IndexOf("(", 0);
                            text = text.Remove(startIndex);
                            stringBuilder.Append("\"").Append(text).Append("\"");
                            break;
                        }
                    case Pattern.Mvc:
                        if (control.ShowToolBar)
                        {
                            stringBuilder.Append(conditions.MvcActions.ToString());
                        }
                        else
                        {
                            stringBuilder.Append("null");
                        }
                        break;
                }
                stringBuilder.Append(",");
            }
            else
            {
                stringBuilder.Append("null,");
            }
            stringBuilder.Append(control.ShowLineDots ? "true" : "false").Append(",");
            stringBuilder.Append(control.ShowMenuOnElementClicked ? "true" : "false").Append(",");
            stringBuilder.Append(control.ShowDescriptionsOnMouseHover ? "true" : "false").Append(",");
            stringBuilder.Append(control.ShowMenuOnRightArrowKey ? "true" : "false").Append(",");
            stringBuilder.Append(Enum.Format(typeof(RuleType), conditions.Mode, "D")).Append(",");
            if (!control.ClientOnly || control.Theme == ThemeType.None)
            {
                stringBuilder.Append("null,null],");
            }
            else
            {
                stringBuilder.Append("\"").Append(conditions.ThemeFactory.StyleTagAttribute).Append("\",\"").Append(conditions.ThemeFactory.GetLinkUrl()).Append("\"],");
            }
            stringBuilder.Append("s:").Append(conditions.Settings.ToString()).Append("}");
            if (includeInitialization)
            {
                stringBuilder.Append("');");
            }
            return stringBuilder.ToString();
        }

        internal static string RenderRuleDataForLoading(RuleModel rule, string controlServerID)
        {
            if (rule.InvalidElements.Count == 0)
            {
                RuleValidator.EnsureValuesForClient(rule.Elements);
            }
            return MarkupManager.SerializeElements<Element>(rule.Elements, ClientDataType.Rule, rule, controlServerID, true);
        }

        internal static string RenderInvalidDataForLoading(RuleModel rule, string controlServerID)
        {
            return MarkupManager.SerializeElements<InvalidElement>(rule.InvalidElements, ClientDataType.Invalids, rule, controlServerID, true);
        }

        internal static string RenderRuleClientData(RuleModel rule)
        {
            if (rule.InvalidElements.Count == 0)
            {
                RuleValidator.EnsureValuesForClient(rule.Elements);
            }
            return MarkupManager.SerializeElements<Element>(rule.Elements, ClientDataType.Rule, rule);
        }

        internal static string RenderRuleInvalidClientData(RuleModel rule)
        {
            return MarkupManager.SerializeElements<InvalidElement>(rule.InvalidElements, ClientDataType.Invalids, rule);
        }

        private static string SerializeElements<T>(List<T> list, ClientDataType type, RuleModel data)
        {
            return MarkupManager.SerializeElements<T>(list, type, data, null, false);
        }

        private static string SerializeElements<T>(List<T> list, ClientDataType type, RuleModel data, string controlServerID, bool includeInitialization)
        {
            StringBuilder stringBuilder = new StringBuilder();
            switch (type)
            {
                case ClientDataType.Rule:
                    if (includeInitialization)
                    {
                        stringBuilder.Append("$ce('").Append(controlServerID).Append("').load('");
                    }
                    stringBuilder.Append("[{g:");
                    if (string.IsNullOrWhiteSpace(data.Id))
                    {
                        stringBuilder.Append("null");
                    }
                    else
                    {
                        stringBuilder.Append("\"").Append(data.Id).Append("\"");
                    }
                    stringBuilder.Append(",");
                    stringBuilder.Append("v:").Append(data.IsLoadedRuleOfEvalType.Value ? "true" : "false").Append(",");
                    stringBuilder.Append("n:");
                    if (string.IsNullOrEmpty(data.Name))
                    {
                        stringBuilder.Append("null");
                    }
                    else
                    {
                        stringBuilder.Append("\"").Append(Encoder.Sanitize(data.Name)).Append("\"");
                    }
                    stringBuilder.Append(",");
                    stringBuilder.Append("d:");
                    if (string.IsNullOrEmpty(data.Desc))
                    {
                        stringBuilder.Append("null");
                    }
                    else
                    {
                        stringBuilder.Append("\"").Append(Encoder.Sanitize(data.Desc)).Append("\"");
                    }
                    stringBuilder.Append("}");
                    break;
                case ClientDataType.Invalids:
                    if (includeInitialization)
                    {
                        stringBuilder.Append("$ce('").Append(controlServerID).Append("').invalid('");
                    }
                    stringBuilder.Append("[{e:");
                    stringBuilder.Append(data.NameIsInvalid ? "true" : "false");
                    stringBuilder.Append("}");
                    break;
                default:
                    throw new Exception("Unexpected value of ClientDataType");
            }
            foreach (T current in list)
            {
                stringBuilder.Append(",").Append(current.ToString());
            }
            stringBuilder.Append("]");
            if (includeInitialization)
            {
                stringBuilder.Append("');");
            }
            return stringBuilder.ToString();
        }

        private static void RegisterLocalEnum(Type t, StringBuilder sb, List<string> registeredNamespaces)
        {
            MarkupManager.RegisterNamespace(t.Namespace, registeredNamespaces, sb);
            sb.Append(MarkupManager.Localize(t.FullName)).Append(" = {");
            bool flag = true;
            foreach (object current in Enum.GetValues(t))
            {
                if (!flag)
                {
                    sb.Append(",");
                }
                else
                {
                    flag = false;
                }
                sb.Append(Enum.GetName(t, current)).Append(":").Append(int.Parse(Enum.Format(t, current, "D")));
            }
            sb.Append("};");
        }

        private static void RegisterNamespace(string spaceName, List<string> namespaces, StringBuilder sb)
        {
            if (!namespaces.Contains(spaceName))
            {
                sb.Append("CodeEffects.register('").Append(spaceName).Append("');");
                namespaces.Add(spaceName);
            }
        }

        private static string Localize(string ns)
        {
            if (!ns.StartsWith("CodeEffects.Rule."))
            {
                return ns;
            }
            return ns.Replace("CodeEffects.Rule.", "$rule.");
        }
    }
}
