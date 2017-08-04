using ESPL.Rule.Attributes;
using ESPL.Rule.Common;
using ESPL.Rule.Core;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Client
{
    internal sealed class Settings
    {
        private bool includeToolBarSettings
        {
            get;
            set;
        }

        private XmlDocument help
        {
            get;
            set;
        }

        public Labels Labels
        {
            get;
            set;
        }

        public List<EnumHolder> Enums
        {
            get;
            set;
        }

        public List<DataSourceDescriber> DataSources
        {
            get;
            set;
        }

        public List<Item> Clauses
        {
            get;
            set;
        }

        public List<Item> Collections
        {
            get;
            set;
        }

        public List<Item> Setters
        {
            get;
            set;
        }

        public List<Field> Operators
        {
            get;
            set;
        }

        public List<Item> Flows
        {
            get;
            set;
        }

        public ICollection<MenuItem> ToolBarRules
        {
            get;
            set;
        }

        public List<SourceHolder> Sources
        {
            get;
            set;
        }

        public Settings(XmlDocument help, ICollection<MenuItem> toolBarRules, bool includeToolBarSettings, RuleType mode)
        {
            this.help = help;
            this.includeToolBarSettings = includeToolBarSettings;
            this.Flows = new List<Item>();
            this.Clauses = new List<Item>();
            this.Collections = new List<Item>();
            this.Setters = new List<Item>();
            this.Sources = new List<SourceHolder>();
            this.Operators = new List<Field>();
            this.Labels = new Labels(help, includeToolBarSettings, mode);
            this.Enums = new List<EnumHolder>();
            this.DataSources = new List<DataSourceDescriber>();
            this.ToolBarRules = toolBarRules;
        }

        internal void Load(IControl control, ICollection<MenuItem> contextMenuRules, XmlDocument sourceXml, ICollection<DataSourceHolder> dataSourceHolders)
        {
            SourceValidator.Validate(sourceXml);
            List<string> registeredNamespaces = new List<string>();
            foreach (DataSource current in SourceLoader.GetDataSources(sourceXml))
            {
                this.DataSources.Add(this.LoadDataSource(current));
            }
            if (dataSourceHolders != null && dataSourceHolders.Count > 0)
            {
                using (IEnumerator<DataSourceHolder> enumerator2 = dataSourceHolders.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        DataSourceHolder dh = enumerator2.Current;
                        if (this.DataSources.Any((DataSourceDescriber s) => s.Name == dh.Name))
                        {
                            throw new SourceException(SourceException.ErrorIds.MultipleDynamicMenuDataSources, new string[0]);
                        }
                        this.DataSources.Add(this.LoadDataSource(dh));
                    }
                }
            }
            this.Flows.Add(new Item
            {
                Value = "if"
            });
            if (control.Mode == RuleType.Execution)
            {
                this.Flows.Add(new Item
                {
                    Name = this.GetHelpValue("/codeeffects/flow/elseIf"),
                    Value = "elseIf"
                });
                this.Flows.Add(new Item
                {
                    Name = this.GetHelpValue("/codeeffects/flow/else"),
                    Value = "else"
                });
            }
            else if (control.Mode == RuleType.Ruleset)
            {
                this.Flows.Add(new Item
                {
                    Name = this.GetHelpValue("/codeeffects/flow/executionIf"),
                    Value = "elseIf"
                });
            }
            this.Clauses.Add(new Item
            {
                Name = this.GetHelpValue("/codeeffects/clauses/and"),
                Value = "and",
                Type = ElementType.Clause
            });
            this.Clauses.Add(new Item
            {
                Name = this.GetHelpValue("/codeeffects/clauses/or"),
                Value = "or",
                Type = ElementType.Clause
            });
            if (control.Mode != RuleType.Evaluation && control.Mode != RuleType.Filter)
            {
                this.Clauses.Add(new Item
                {
                    Name = this.GetHelpValue("/codeeffects/clauses/" + ((control.Mode == RuleType.Loop) ? "do" : "then"), "do"),
                    Value = "then",
                    Type = ElementType.Clause
                });
                this.Setters.Add(new Item
                {
                    Name = this.GetHelpValue("/codeeffects/setters/set", "set"),
                    Value = "set",
                    Type = ElementType.Setter
                });
                this.Setters.Add(new Item
                {
                    Name = this.GetHelpValue("/codeeffects/setters/to", "to"),
                    Value = "to",
                    Type = ElementType.Setter
                });
            }
            string text = "s";
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(sourceXml.NameTable);
            xmlNamespaceManager.AddNamespace(text, sourceXml.DocumentElement.NamespaceURI);
            for (int i = 0; i < sourceXml.DocumentElement.ChildNodes.Count; i++)
            {
                XmlNode xmlNode = sourceXml.DocumentElement.ChildNodes[i];
                SourceHolder sourceHolder = new SourceHolder();
                sourceHolder.Name = ((xmlNode.Attributes["name"] == null) ? "Default.Source.Name" : xmlNode.Attributes["name"].Value);
                XmlNodeList childNodes = xmlNode.SelectSingleNode(string.Format("{0}:fields", text), xmlNamespaceManager).ChildNodes;
                List<Item> list = new List<Item>();
                this.AutodetectOperators(control, childNodes);
                foreach (XmlNode xmlNode2 in childNodes)
                {
                    if (xmlNode2.NodeType != XmlNodeType.Comment)
                    {
                        SourceValidator.ValidateField(xmlNode2);
                        string name;
                        if ((name = xmlNode2.Name) != null)
                        {
                            if (name == "collection")
                            {
                                Field field = new Field
                                {
                                    Name = xmlNode2.Attributes["displayName"].Value,
                                    Value = xmlNode2.Attributes["propertyName"].Value,
                                    Nullable = true,
                                    Description = ((xmlNode2.Attributes["description"] == null) ? null : xmlNode2.Attributes["description"].Value),
                                    DataType = OperatorType.Collection,
                                    Type = ElementType.Field,
                                    IncludeNullableInJson = true
                                };
                                if (xmlNode2.Attributes["gettable"] != null && xmlNode2.Attributes["gettable"].Value == "false")
                                {
                                    field.Gettable = false;
                                }
                                field.Collection = this.GetCollection(xmlNode2, SettingType.Field);
                                if (field.Collection.Type == CollectionType.Value)
                                {
                                    OperatorType operatorType = Converter.ClientStringToClientType(xmlNode2.ChildNodes[0].Attributes["type"].Value);
                                    field.ValueInputType = Converter.StringToValueInputType(xmlNode2.Attributes["valueInputType"].Value);
                                    field.Settings = this.GetSettings(xmlNode2.ChildNodes[0], operatorType, SettingType.Field, registeredNamespaces);
                                    if (operatorType == OperatorType.Numeric)
                                    {
                                        field.ValueInputType = ValueInputType.User;
                                        field.Settings.AllowCalculations = (field.Settings.IncludeInCalculations = false);
                                    }
                                }
                                else
                                {
                                    field.ValueInputType = ValueInputType.Fields;
                                }
                                list.Add(field);
                                continue;
                            }
                            if (name == "function")
                            {
                                Function function = new Function
                                {
                                    Name = xmlNode2.Attributes["displayName"].Value,
                                    Value = SourceLoader.GetTokenBySourceMethodNode(xmlNode, xmlNode2),
                                    Type = ElementType.Function,
                                    IncludeReturnInJson = true
                                };
                                if (xmlNode2.Attributes["description"] != null)
                                {
                                    function.Description = xmlNode2.Attributes["description"].Value;
                                }
                                if (xmlNode2.Attributes["includeInCalculations"] != null)
                                {
                                    function.IncludeInCalculations = (xmlNode2.Attributes["includeInCalculations"].Value == "true");
                                }
                                if (xmlNode2.Attributes["gettable"] != null && xmlNode2.Attributes["gettable"].Value == "false")
                                {
                                    function.Gettable = false;
                                }
                                foreach (XmlNode xmlNode3 in xmlNode2.ChildNodes)
                                {
                                    if (xmlNode3.NodeType != XmlNodeType.Comment)
                                    {
                                        if (xmlNode3.Name == "returns")
                                        {
                                            function.Returns.DataType = Converter.ClientStringToClientType(xmlNode3.Attributes["type"].Value);
                                            function.Returns.Nullable = (xmlNode3.Attributes["nullable"].Value == "true");
                                            function.Returns.ValueInputType = Converter.StringToValueInputType(xmlNode3.Attributes["valueInputType"].Value);
                                            function.Returns.Settings = this.GetSettings(xmlNode3, function.Returns.DataType, SettingType.Return, registeredNamespaces);
                                        }
                                        else
                                        {
                                            foreach (XmlNode xmlNode4 in xmlNode3.ChildNodes)
                                            {
                                                if (xmlNode4.NodeType != XmlNodeType.Comment)
                                                {
                                                    ParameterType parameterType = Converter.StringToParameterType(xmlNode4.Name);
                                                    if (parameterType == ParameterType.Input || parameterType == ParameterType.Collection)
                                                    {
                                                        function.Parameters.Add(this.GetParameter(xmlNode4, registeredNamespaces));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                list.Add(function);
                                continue;
                            }
                        }
                        Field field2 = new Field
                        {
                            Name = xmlNode2.Attributes["displayName"].Value,
                            Value = xmlNode2.Attributes["propertyName"].Value,
                            Nullable = (xmlNode2.Attributes["nullable"].Value == "true"),
                            Description = ((xmlNode2.Attributes["description"] == null) ? null : xmlNode2.Attributes["description"].Value),
                            DataType = Converter.ClientStringToClientType(xmlNode2.Name),
                            Type = ElementType.Field,
                            ValueInputType = Converter.StringToValueInputType(xmlNode2.Attributes["valueInputType"].Value),
                            IncludeNullableInJson = true
                        };
                        if (xmlNode2.Attributes["settable"] != null && xmlNode2.Attributes["settable"].Value == "false")
                        {
                            field2.Settable = false;
                        }
                        if (xmlNode2.Attributes["gettable"] != null && xmlNode2.Attributes["gettable"].Value == "false")
                        {
                            field2.Gettable = false;
                        }
                        field2.Settings = this.GetSettings(xmlNode2, field2.DataType, SettingType.Field, registeredNamespaces);
                        list.Add(field2);
                    }
                }
                XmlNode xmlNode5 = xmlNode.SelectSingleNode(string.Format("{0}:actions", text), xmlNamespaceManager);
                SourceValidator.ValidateActions(xmlNode5);
                List<Function> list2 = new List<Function>();
                if (xmlNode5 != null && control.Mode != RuleType.Evaluation && control.Mode != RuleType.Filter)
                {
                    Function function2 = null;
                    foreach (XmlNode xmlNode6 in xmlNode5.ChildNodes)
                    {
                        if (xmlNode6.NodeType != XmlNodeType.Comment)
                        {
                            function2 = new Function
                            {
                                Name = xmlNode6.Attributes["displayName"].Value,
                                Value = SourceLoader.GetTokenBySourceMethodNode(xmlNode, xmlNode6),
                                Type = ElementType.Action
                            };
                            if (xmlNode6.Attributes["description"] != null)
                            {
                                function2.Description = xmlNode6.Attributes["description"].Value;
                            }
                            if (xmlNode6.HasChildNodes)
                            {
                                foreach (XmlNode xmlNode7 in xmlNode6.ChildNodes[0].ChildNodes)
                                {
                                    if (xmlNode7.NodeType != XmlNodeType.Comment)
                                    {
                                        ParameterType parameterType2 = Converter.StringToParameterType(xmlNode7.Name);
                                        if (parameterType2 == ParameterType.Input || parameterType2 == ParameterType.Collection)
                                        {
                                            function2.Parameters.Add(this.GetParameter(xmlNode7, registeredNamespaces));
                                        }
                                    }
                                }
                            }
                            list2.Add(function2);
                        }
                    }
                }
                bool flag = false;
                List<CollectionHolder> list3 = new List<CollectionHolder>();
                foreach (Item current2 in list)
                {
                    if (current2.Type == ElementType.Field)
                    {
                        Field field3 = (Field)current2;
                        if (field3.DataType == OperatorType.Collection)
                        {
                            if (field3.Collection.Type == CollectionType.Reference)
                            {
                                flag = true;
                            }
                            list3.Add(field3.Collection);
                        }
                    }
                }
                foreach (Item current3 in list)
                {
                    if (current3.Type == ElementType.Function)
                    {
                        Function function3 = (Function)current3;
                        if (function3.Parameters.Count == 0)
                        {
                            sourceHolder.Fields.Add(current3);
                        }
                        else if (this.ValidateParameters(function3.Parameters, list3, flag))
                        {
                            sourceHolder.Fields.Add(current3);
                        }
                    }
                    else
                    {
                        sourceHolder.Fields.Add(current3);
                    }
                }
                foreach (Function current4 in list2)
                {
                    if (current4.Parameters.Count == 0)
                    {
                        sourceHolder.Actions.Add(current4);
                    }
                    else if (this.ValidateParameters(current4.Parameters, list3, flag))
                    {
                        sourceHolder.Actions.Add(current4);
                    }
                }
                if (contextMenuRules != null && contextMenuRules.Count > 0)
                {
                    foreach (MenuItem current5 in contextMenuRules)
                    {
                        if ((string.IsNullOrWhiteSpace(current5.SourceName) && i == 0) || current5.SourceName == sourceHolder.Name)
                        {
                            Field item = new Field
                            {
                                Name = current5.DisplayName,
                                Value = current5.ID.ToString(),
                                Description = (string.IsNullOrWhiteSpace(current5.Description) ? null : current5.Description),
                                Nullable = false,
                                DataType = OperatorType.Bool,
                                Type = ElementType.Field,
                                IsRule = true,
                                ValueInputType = ValueInputType.User,
                                IncludeNullableInJson = true
                            };
                            sourceHolder.Fields.Add(item);
                        }
                    }
                }
                if (flag)
                {
                    this.Collections.Add(new Item
                    {
                        Name = this.GetHelpValue("/codeeffects/collections/exists", "exists"),
                        Value = "exists",
                        Type = ElementType.Exists
                    });
                    this.Collections.Add(new Item
                    {
                        Name = this.GetHelpValue("/codeeffects/collections/doesNotExist", "doesNotExist"),
                        Value = "doesNotExist",
                        Type = ElementType.Exists
                    });
                    this.Collections.Add(new Item
                    {
                        Name = this.GetHelpValue("/codeeffects/collections/where", "where"),
                        Value = "where",
                        Type = ElementType.Where
                    });
                }
                if (sourceHolder.Fields.Count > 0 && !control.KeepDeclaredOrder)
                {
                    sourceHolder.Fields.Sort((Item f1, Item f2) => f1.Name.CompareTo(f2.Name));
                }
                if (sourceHolder.Actions.Count > 0 && !control.KeepDeclaredOrder)
                {
                    sourceHolder.Actions.Sort((Function f1, Function f2) => f1.Name.CompareTo(f2.Name));
                }
                this.Sources.Add(sourceHolder);
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{lbl:");
            stringBuilder.Append(this.Labels.ToString());
            stringBuilder.Append(",");
            this.GetJson<Item>("fls", this.Flows, stringBuilder);
            this.GetJson<Item>("cls", this.Clauses, stringBuilder);
            this.GetJson<Item>("dsc", this.Collections, stringBuilder);
            this.GetJson<Item>("str", this.Setters, stringBuilder);
            if (this.includeToolBarSettings && this.ToolBarRules != null && this.ToolBarRules.Count > 0)
            {
                this.GetJson<MenuItem>("rls", this.ToolBarRules, stringBuilder);
            }
            if (this.Enums.Count > 0)
            {
                this.GetJson<EnumHolder>("ens", this.Enums, stringBuilder);
            }
            if (this.DataSources.Count > 0)
            {
                this.GetJson<DataSourceDescriber>("mds", this.DataSources, stringBuilder);
            }
            this.GetJson<Field>("ops", this.Operators, stringBuilder);
            this.GetJson<SourceHolder>("src", this.Sources, stringBuilder);
            stringBuilder.Remove(stringBuilder.Length - 1, 1).Append("}");
            return stringBuilder.ToString();
        }

        private bool IsOperatorExcluded(IControl control, OperatorType type, ExcludedOperator ex)
        {
            return control.ExcludedOperators.Any((Operator o) => o.Type == type && (o.ExcludedOperator & ex) == ex);
        }

        private void AutodetectOperators(IControl control, XmlNodeList fields)
        {
            bool flag = false;
            if (SourceValidator.IsOperatorTypeUsed(fields, OperatorType.Bool, out flag))
            {
                if (!this.IsOperatorExcluded(control, OperatorType.Bool, ExcludedOperator.Equal))
                {
                    this.AddOperator(OperatorType.Bool, "equal");
                }
                if (flag)
                {
                    this.AddOperator(OperatorType.Bool, "isNull");
                    this.AddOperator(OperatorType.Bool, "isNotNull");
                }
            }
            flag = false;
            if (SourceValidator.IsOperatorTypeUsed(fields, OperatorType.Date, out flag))
            {
                if (!this.IsOperatorExcluded(control, OperatorType.Date, ExcludedOperator.Equal))
                {
                    this.AddOperator(OperatorType.Date, "equal");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Date, ExcludedOperator.NotEqual))
                {
                    this.AddOperator(OperatorType.Date, "notEqual");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Date, ExcludedOperator.Greater))
                {
                    this.AddOperator(OperatorType.Date, "greater");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Date, ExcludedOperator.GreaterOrEqual))
                {
                    this.AddOperator(OperatorType.Date, "greaterOrEqual");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Date, ExcludedOperator.Less))
                {
                    this.AddOperator(OperatorType.Date, "less");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Date, ExcludedOperator.LessOrEqual))
                {
                    this.AddOperator(OperatorType.Date, "lessOrEqual");
                }
                if (flag)
                {
                    this.AddOperator(OperatorType.Date, "isNull");
                    this.AddOperator(OperatorType.Date, "isNotNull");
                }
            }
            flag = false;
            if (SourceValidator.IsOperatorTypeUsed(fields, OperatorType.Time, out flag))
            {
                if (!this.IsOperatorExcluded(control, OperatorType.Time, ExcludedOperator.Equal))
                {
                    this.AddOperator(OperatorType.Time, "equal");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Time, ExcludedOperator.NotEqual))
                {
                    this.AddOperator(OperatorType.Time, "notEqual");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Time, ExcludedOperator.Greater))
                {
                    this.AddOperator(OperatorType.Time, "greater");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Time, ExcludedOperator.GreaterOrEqual))
                {
                    this.AddOperator(OperatorType.Time, "greaterOrEqual");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Time, ExcludedOperator.Less))
                {
                    this.AddOperator(OperatorType.Time, "less");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Time, ExcludedOperator.LessOrEqual))
                {
                    this.AddOperator(OperatorType.Time, "lessOrEqual");
                }
                if (flag)
                {
                    this.AddOperator(OperatorType.Time, "isNull");
                    this.AddOperator(OperatorType.Time, "isNotNull");
                }
            }
            flag = false;
            if (SourceValidator.IsOperatorTypeUsed(fields, OperatorType.Numeric, out flag))
            {
                if (!this.IsOperatorExcluded(control, OperatorType.Numeric, ExcludedOperator.Equal))
                {
                    this.AddOperator(OperatorType.Numeric, "equal");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Numeric, ExcludedOperator.NotEqual))
                {
                    this.AddOperator(OperatorType.Numeric, "notEqual");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Numeric, ExcludedOperator.Greater))
                {
                    this.AddOperator(OperatorType.Numeric, "greater");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Numeric, ExcludedOperator.GreaterOrEqual))
                {
                    this.AddOperator(OperatorType.Numeric, "greaterOrEqual");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Numeric, ExcludedOperator.Less))
                {
                    this.AddOperator(OperatorType.Numeric, "less");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Numeric, ExcludedOperator.LessOrEqual))
                {
                    this.AddOperator(OperatorType.Numeric, "lessOrEqual");
                }
                if (flag)
                {
                    this.AddOperator(OperatorType.Numeric, "isNull");
                    this.AddOperator(OperatorType.Numeric, "isNotNull");
                }
            }
            if (SourceValidator.IsOperatorTypeUsed(fields, OperatorType.Enum, out flag))
            {
                if (!this.IsOperatorExcluded(control, OperatorType.Enum, ExcludedOperator.Equal))
                {
                    this.AddOperator(OperatorType.Enum, "equal");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Enum, ExcludedOperator.NotEqual))
                {
                    this.AddOperator(OperatorType.Enum, "notEqual");
                }
            }
            if (SourceValidator.IsOperatorTypeUsed(fields, OperatorType.String, out flag))
            {
                if (!this.IsOperatorExcluded(control, OperatorType.String, ExcludedOperator.Equal))
                {
                    this.AddOperator(OperatorType.String, "equal");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.String, ExcludedOperator.NotEqual))
                {
                    this.AddOperator(OperatorType.String, "notEqual");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.String, ExcludedOperator.Contains))
                {
                    this.AddOperator(OperatorType.String, "contains");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.String, ExcludedOperator.DoesNotContain))
                {
                    this.AddOperator(OperatorType.String, "doesNotContain");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.String, ExcludedOperator.StartsWith))
                {
                    this.AddOperator(OperatorType.String, "startsWith");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.String, ExcludedOperator.DoesNotStartWith))
                {
                    this.AddOperator(OperatorType.String, "doesNotStartWith");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.String, ExcludedOperator.EndsWith))
                {
                    this.AddOperator(OperatorType.String, "endsWith");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.String, ExcludedOperator.DoesNotEndWith))
                {
                    this.AddOperator(OperatorType.String, "doesNotEndWith");
                }
                this.AddOperator(OperatorType.String, "isNull");
                this.AddOperator(OperatorType.String, "isNotNull");
            }
            if (SourceValidator.IsOperatorTypeUsed(fields, OperatorType.Collection, out flag))
            {
                if (!this.IsOperatorExcluded(control, OperatorType.Collection, ExcludedOperator.Contains))
                {
                    this.AddOperator(OperatorType.Collection, "contains", "contains");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Collection, ExcludedOperator.DoesNotContain))
                {
                    this.AddOperator(OperatorType.Collection, "doesNotContain", "doesNotContain");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Collection, ExcludedOperator.StartsWith))
                {
                    this.AddOperator(OperatorType.Collection, "startsWith", "startsWith");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Collection, ExcludedOperator.DoesNotStartWith))
                {
                    this.AddOperator(OperatorType.Collection, "doesNotStartWith", "doesNotStartWith");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Collection, ExcludedOperator.EndsWith))
                {
                    this.AddOperator(OperatorType.Collection, "endsWith", "endsWith");
                }
                if (!this.IsOperatorExcluded(control, OperatorType.Collection, ExcludedOperator.DoesNotEndWith))
                {
                    this.AddOperator(OperatorType.Collection, "doesNotEndWith", "doesNotEndWith");
                }
                this.AddOperator(OperatorType.Collection, "isNull", "isNull");
                this.AddOperator(OperatorType.Collection, "isNotNull", "isNotNull");
            }
        }

        private void AddOperator(OperatorType type, string value)
        {
            this.AddOperator(type, value, "-default-");
        }

        private void AddOperator(OperatorType type, string value, string defaultValue)
        {
            if (!this.Operators.Any((Field o) => o.Value == value && o.DataType == type))
            {
                this.Operators.Add(new Field
                {
                    Name = this.GetHelpValue(string.Format("{0}/{1}/{2}", "/codeeffects/operators", Converter.ClientTypeToClientString(type), value), defaultValue),
                    Value = value,
                    DataType = type,
                    Type = ElementType.Operator
                });
            }
        }

        private void GetJson<T>(string name, ICollection<T> list, StringBuilder sb)
        {
            bool flag = true;
            sb.Append(name).Append(":[");
            foreach (T current in list)
            {
                if (!flag)
                {
                    sb.Append(",");
                }
                else
                {
                    flag = false;
                }
                sb.Append(current.ToString());
            }
            sb.Append("],");
        }

        private EnumHolder LoadEnum(XmlNode node, List<string> registered)
        {
            return this.LoadEnum(node.Attributes["assembly"].Value, node.Attributes["class"].Value, registered);
        }

        private EnumHolder LoadEnum(string assembly, string type, List<string> registered)
        {
            if (registered.Contains(type))
            {
                return null;
            }
            registered.Add(type);
            Type type2 = null;
            try
            {
                Assembly assembly2 = Assembly.Load(assembly);
                type2 = assembly2.GetType(type);
            }
            catch (Exception ex)
            {
                throw new SourceException(SourceException.ErrorIds.FailedToLoadAssemblyOrType, new string[]
				{
					assembly,
					type,
					ex.Message
				});
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            EnumHolder enumHolder = new EnumHolder();
            if (type2.MemberType == MemberTypes.NestedType)
            {
                string[] array = type.Split(new char[]
				{
					'.'
				});
                enumHolder.Name = array[array.Length - 1];
            }
            else
            {
                enumHolder.Name = type2.Name;
            }
            enumHolder.Ns = type2.Namespace;
            Array values = Enum.GetValues(type2);
            bool flag = true;
            foreach (object current in values)
            {
                string name = Enum.GetName(type2, current);
                string value = name;
                FieldInfo field = type2.GetField(name);
                if (field.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), false).Length == 0)
                {
                    object[] customAttributes = field.GetCustomAttributes(typeof(EnumItemAttribute), false);
                    if (customAttributes.Length > 0)
                    {
                        value = ESPL.Rule.Core.Encoder.Sanitize(((EnumItemAttribute)customAttributes[0]).DisplayName);
                    }
                    if (!flag)
                    {
                        stringBuilder.Append(",");
                    }
                    else
                    {
                        flag = false;
                    }
                    stringBuilder.Append("{ID:").Append(int.Parse(Enum.Format(type2, current, "D"))).Append(",Name:\\\"").Append(value).Append("\\\"}");
                }
            }
            stringBuilder.Append("]");
            enumHolder.Data = stringBuilder.ToString();
            return enumHolder;
        }

        private CollectionHolder GetCollection(XmlNode fieldNode, SettingType type)
        {
            CollectionHolder collectionHolder = new CollectionHolder();
            collectionHolder.Type = Converter.StringToCollectionType(fieldNode.ChildNodes[0].Name);
            collectionHolder.IsArray = (fieldNode.Attributes["array"].Value == "true");
            collectionHolder.IsGeneric = (fieldNode.Attributes["generic"].Value == "true");
            collectionHolder.ComparisonName = fieldNode.Attributes["comparisonName"].Value;
            switch (collectionHolder.Type)
            {
                case CollectionType.Value:
                    collectionHolder.UnderlyingTypeFullName = fieldNode.ChildNodes[0].Attributes["class"].Value;
                    collectionHolder.DataType = Converter.ClientStringToClientType(fieldNode.ChildNodes[0].Attributes["type"].Value);
                    collectionHolder.IsUnderlyingTypeNullable = (fieldNode.ChildNodes[0].Attributes["nullable"].Value == "true");
                    return collectionHolder;
                case CollectionType.Reference:
                    if (type == SettingType.Field)
                    {
                        collectionHolder.DisplayName = fieldNode.ChildNodes[0].Attributes["displayName"].Value;
                    }
                    collectionHolder.UnderlyingTypeFullName = fieldNode.ChildNodes[0].Attributes["class"].Value;
                    return collectionHolder;
            }
            if (type == SettingType.Field)
            {
                throw new SourceException(SourceException.ErrorIds.GenericCollectionsAndPropertiesNotSupported, new string[0]);
            }
            return collectionHolder;
        }

        private SettingHolder GetSettings(XmlNode node, OperatorType dataType, SettingType elementType, List<string> registeredNamespaces)
        {
            SettingHolder settingHolder = new SettingHolder();
            if (elementType != SettingType.Field)
            {
                settingHolder.Nullable = (node.Attributes["nullable"].Value == "true");
            }
            switch (dataType)
            {
                case OperatorType.String:
                    if (node.Attributes["maxLength"] != null)
                    {
                        settingHolder.Max = new decimal?(new int?(int.Parse(node.Attributes["maxLength"].Value)).Value);
                    }
                    break;
                case OperatorType.Numeric:
                    switch (elementType)
                    {
                        case SettingType.Field:
                            settingHolder.IncludeInCalculations = (node.Attributes["includeInCalculations"].Value == "true");
                            break;
                        case SettingType.Parameter:
                            goto IL_BD;
                        case SettingType.Return:
                            break;
                        default:
                            goto IL_BD;
                    }
                    settingHolder.AllowCalculations = (node.Attributes["allowCalculation"].Value == "true");
                IL_BD:
                    settingHolder.AllowDecimals = (node.Attributes["allowDecimal"].Value == "true");
                    settingHolder.Min = new decimal?(decimal.Parse(node.Attributes["min"].Value));
                    settingHolder.Max = new decimal?(decimal.Parse(node.Attributes["max"].Value));
                    if (node.Attributes["dataSourceName"] != null)
                    {
                        settingHolder.DataSourceName = node.Attributes["dataSourceName"].Value;
                    }
                    break;
                case OperatorType.Date:
                case OperatorType.Time:
                    settingHolder.Format = node.Attributes["format"].Value;
                    break;
                case OperatorType.Enum:
                    {
                        settingHolder.TypeFullName = node.Attributes["class"].Value;
                        EnumHolder holder = this.LoadEnum(node, registeredNamespaces);
                        if (holder != null && !this.Enums.Any((EnumHolder e) => e.Name == holder.Name && e.Ns == holder.Ns))
                        {
                            this.Enums.Add(holder);
                        }
                        break;
                    }
            }
            return settingHolder;
        }

        private DataSourceDescriber LoadDataSource(DataSourceHolder source)
        {
            DataSourceDescriber dataSourceDescriber = new DataSourceDescriber();
            dataSourceDescriber.Client = false;
            dataSourceDescriber.Name = source.Name;
            try
            {
                List<DataSourceItem> items = source.Delegate();
                this.HandleDataSourceItems(dataSourceDescriber, items);
            }
            catch (Exception ex)
            {
                throw new SourceException(SourceException.ErrorIds.DelegateInvokeOrConversionError, new string[]
				{
					source.Name,
					ex.Message
				});
            }
            return dataSourceDescriber;
        }

        private DataSourceDescriber LoadDataSource(DataSource source)
        {
            DataSourceDescriber dataSourceDescriber = new DataSourceDescriber();
            dataSourceDescriber.Client = (source.Location == FeatureLocation.Client);
            dataSourceDescriber.Name = source.Name;
            if (dataSourceDescriber.Client)
            {
                dataSourceDescriber.Data = "[\\\"" + source.Method + "\\\"]";
            }
            else
            {
                Type type = null;
                ICollection<DataSourceItem> items = null;
                try
                {
                    Assembly assembly = Assembly.Load(source.Assembly);
                    type = assembly.GetType(source.Class);
                }
                catch (Exception ex)
                {
                    throw new SourceException(SourceException.ErrorIds.FailedToLoadAssemblyOrType, new string[]
					{
						source.Assembly,
						source.Class,
						ex.Message
					});
                }
                MethodInfo methodInfo = null;
                try
                {
                    methodInfo = type.GetMethod(source.Method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, new Type[0], new ParameterModifier[0]);
                    if (methodInfo == null)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    throw new SourceException(SourceException.ErrorIds.MissingOrInaccessibleMethod, new string[]
					{
						source.Method
					});
                }
                if (!SourceValidator.IsDataSourceMethodValid(methodInfo))
                {
                    throw new SourceException(SourceException.ErrorIds.MethodIsNotDataSource, new string[]
					{
						source.Method
					});
                }
                try
                {
                    object obj = methodInfo.Invoke(methodInfo.IsStatic ? type : Activator.CreateInstance(type), null);
                    items = (ICollection<DataSourceItem>)obj;
                }
                catch (Exception ex2)
                {
                    throw new SourceException(SourceException.ErrorIds.MethodInvokeOrConversionError, new string[]
					{
						source.Method,
						ex2.Message
					});
                }
                this.HandleDataSourceItems(dataSourceDescriber, items);
            }
            return dataSourceDescriber;
        }

        private void HandleDataSourceItems(DataSourceDescriber h, ICollection<DataSourceItem> items)
        {
            if (items != null && items.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("[");
                bool flag = true;
                foreach (DataSourceItem current in items)
                {
                    if (!flag)
                    {
                        stringBuilder.Append(",");
                    }
                    else
                    {
                        flag = false;
                    }
                    stringBuilder.Append("{ID:").Append(current.ID).Append(",Name:\\\"").Append(ESPL.Rule.Core.Encoder.Sanitize(current.Name)).Append("\\\"}");
                }
                stringBuilder.Append("]");
                h.Data = stringBuilder.ToString();
                return;
            }
            h.Data = "[null]";
        }

        private string GetHelpValue(string path)
        {
            return this.GetHelpValue(path, "-default-");
        }

        private string GetHelpValue(string path, string defaultValue)
        {
            XmlNode xmlNode = this.help.SelectSingleNode(path);
            if (xmlNode != null)
            {
                return ESPL.Rule.Core.Encoder.Desanitize(xmlNode.InnerText);
            }
            return defaultValue;
        }

        private Parameter GetParameter(XmlNode p, List<string> registeredNamespaces)
        {
            Parameter parameter = new Parameter();
            parameter.Type = ParameterType.Input;
            if (p.Attributes["description"] != null)
            {
                parameter.Description = p.Attributes["description"].Value;
            }
            if (p.Name == "collection")
            {
                parameter.ValueInputType = ValueInputType.Fields;
                parameter.DataType = OperatorType.Collection;
                parameter.Collection = this.GetCollection(p, SettingType.Parameter);
                if (parameter.Collection.Type == CollectionType.Value)
                {
                    parameter.Nullable = (p.ChildNodes[0].Attributes["nullable"].Value == "true");
                    parameter.Settings = this.GetSettings(p.ChildNodes[0], Converter.ClientStringToClientType(p.ChildNodes[0].Attributes["type"].Value), SettingType.Parameter, registeredNamespaces);
                }
            }
            else
            {
                parameter.DataType = Converter.ClientStringToClientType(p.Attributes["type"].Value);
                parameter.ValueInputType = Converter.StringToValueInputType(p.Attributes["valueInputType"].Value);
                parameter.Nullable = (p.Attributes["nullable"].Value == "true");
                parameter.Settings = this.GetSettings(p, parameter.DataType, SettingType.Parameter, registeredNamespaces);
            }
            return parameter;
        }

        private bool ValidateParameters(List<Parameter> parameters, List<CollectionHolder> collections, bool references)
        {
            using (List<Parameter>.Enumerator enumerator = parameters.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Parameter param = enumerator.Current;
                    if (param.DataType == OperatorType.Collection)
                    {
                        if (collections.Count == 0 || (param.Collection.Type == CollectionType.Reference && !references))
                        {
                            bool result = false;
                            return result;
                        }
                        if (param.Collection.Type == CollectionType.Generic)
                        {
                            if (!collections.Any((CollectionHolder c) => c.IsGeneric))
                            {
                                bool result = false;
                                return result;
                            }
                        }
                        else if (!collections.Any((CollectionHolder c) => c.Type == param.Collection.Type && c.IsArray == param.Collection.IsArray && c.IsGeneric == param.Collection.IsGeneric && c.UnderlyingTypeFullName == param.Collection.UnderlyingTypeFullName && c.IsUnderlyingTypeNullable == param.Collection.IsUnderlyingTypeNullable))
                        {
                            bool result = false;
                            return result;
                        }
                    }
                }
            }
            return true;
        }
    }
}
