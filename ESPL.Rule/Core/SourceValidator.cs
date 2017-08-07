using ESPL.Rule.Client;
using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Core
{
    internal sealed class SourceValidator
    {
        private SourceValidator()
        {
        }

        public static void Validate(XmlDocument source)
        {
            Xml.ValidateSchema(source, Xml.GetSourceNameByNamespace(source.DocumentElement.NamespaceURI));
        }

        internal static void ValidateField(XmlNode field)
        {
            throw new NotImplementedException();//TODO: 
            //string name;
            //if ((name = field.Name) != null)
            //{
            //    if (<PrivateImplementationDetails>{C6EA5F6E-C064-4F46-8F27-151D6168C23D}.$$method0x6000311-1 == null)
            //    {
            //        <PrivateImplementationDetails>{C6EA5F6E-C064-4F46-8F27-151D6168C23D}.$$method0x6000311-1 = new Dictionary<string, int>(8)
            //        {
            //            {
            //                "function",
            //                0
            //            },
            //            {
            //                "collection",
            //                1
            //            },
            //            {
            //                "bool",
            //                2
            //            },
            //            {
            //                "date",
            //                3
            //            },
            //            {
            //                "enum",
            //                4
            //            },
            //            {
            //                "numeric",
            //                5
            //            },
            //            {
            //                "string",
            //                6
            //            },
            //            {
            //                "time",
            //                7
            //            }
            //        };
            //    }
            //    int num;
            //    if (<PrivateImplementationDetails>{C6EA5F6E-C064-4F46-8F27-151D6168C23D}.$$method0x6000311-1.TryGetValue(name, out num))
            //    {
            //        switch (num)
            //        {
            //        case 0:
            //            using (IEnumerator enumerator = field.ChildNodes.GetEnumerator())
            //            {
            //                while (enumerator.MoveNext())
            //                {
            //                    XmlNode xmlNode = (XmlNode)enumerator.Current;
            //                    if (xmlNode.Name == "parameters")
            //                    {
            //                        using (IEnumerator enumerator2 = xmlNode.ChildNodes.GetEnumerator())
            //                        {
            //                            while (enumerator2.MoveNext())
            //                            {
            //                                XmlNode xmlNode2 = (XmlNode)enumerator2.Current;
            //                                string name2;
            //                                if ((name2 = xmlNode2.Name) != null)
            //                                {
            //                                    if (!(name2 == "constant"))
            //                                    {
            //                                        if (!(name2 == "input"))
            //                                        {
            //                                            if (name2 == "collection")
            //                                            {
            //                                                if (xmlNode2.ChildNodes.Count != 1)
            //                                                {
            //                                                    throw new SourceException(SourceException.ErrorIds.InvalidCollectionParameterXML, new string[0]);
            //                                                }
            //                                                if (Converter.StringToCollectionType(xmlNode2.ChildNodes[0].Name) == CollectionType.Value)
            //                                                {
            //                                                    SourceValidator.ValidateFieldDataType(xmlNode2.ChildNodes[0], Converter.ClientStringToClientType(xmlNode2.ChildNodes[0].Attributes["type"].Value));
            //                                                }
            //                                            }
            //                                        }
            //                                        else
            //                                        {
            //                                            switch (Converter.ClientStringToClientType(xmlNode2.Attributes["type"].Value))
            //                                            {
            //                                            case OperatorType.String:
            //                                            case OperatorType.Numeric:
            //                                                SourceValidator.ValidateFieldDataType(xmlNode2, Converter.ClientStringToClientType(xmlNode2.Attributes["type"].Value));
            //                                                break;
            //                                            }
            //                                        }
            //                                    }
            //                                    else
            //                                    {
            //                                        SourceValidator.ValidateConstantParam(xmlNode2);
            //                                    }
            //                                }
            //                            }
            //                            continue;
            //                        }
            //                    }
            //                    if (xmlNode.Name == "returns")
            //                    {
            //                        SourceValidator.ValidateFieldDataType(xmlNode, Converter.ClientStringToClientType(xmlNode.Attributes["type"].Value));
            //                    }
            //                }
            //                return;
            //            }
            //            break;
            //        case 1:
            //            break;
            //        case 2:
            //        case 3:
            //        case 4:
            //        case 5:
            //        case 6:
            //        case 7:
            //            SourceValidator.ValidateFieldDataType(field, Converter.ClientStringToClientType(field.Name));
            //            return;
            //        default:
            //            goto IL_311;
            //        }
            //        if (field.ChildNodes.Count != 1)
            //        {
            //            throw new SourceException(SourceException.ErrorIds.InvalidCollectionParameterXML, new string[0]);
            //        }
            //        if (Converter.StringToCollectionType(field.ChildNodes[0].Name) == CollectionType.Value)
            //        {
            //            SourceValidator.ValidateFieldDataType(field.ChildNodes[0], Converter.ClientStringToClientType(field.ChildNodes[0].Attributes["type"].Value));
            //            return;
            //        }
            //        return;
            //    }
            //}
            //IL_311:
            //throw new MalformedXmlException(MalformedXmlException.ErrorIds.InvalidFieldName, new string[]
            //{
            //    field.Name
            //});
        }

        internal static void ValidateActions(XmlNode actions)
        {
            if (actions != null)
            {
                foreach (XmlNode action in actions.ChildNodes)
                {
                    SourceValidator.ValidateAction(action);
                }
            }
        }

        internal static bool IsMethodValid(MethodInfo m, bool checkReturn)
        {
            if (m == null)
            {
                return false;
            }
            if (m.IsPrivate)
            {
                return false;
            }
            if (m.IsSpecialName)
            {
                return false;
            }
            if (m.IsConstructor)
            {
                return false;
            }
            if (checkReturn)
            {
                if (m.ReturnType == typeof(void))
                {
                    return false;
                }
                if (!SourceValidator.IsAcceptedValueType(m.ReturnType))
                {
                    return false;
                }
            }
            else if (m.ReturnType != typeof(void))
            {
                return false;
            }
            return true;
        }

        internal static bool IsDataSourceMethodValid(MethodInfo m)
        {
            return !(m == null) && !m.IsPrivate && !m.IsSpecialName && m.GetParameters().Length <= 0 && !m.ContainsGenericParameters && !m.IsConstructor && !m.IsGenericMethod && !m.IsGenericMethodDefinition && TypeUtils.AreReferenceAssignable(typeof(ICollection<DataSourceItem>), m.ReturnType);
        }

        internal static bool IsParameterValid(ParameterInfo pi)
        {
            return SourceValidator.IsParameterValid(pi, null);
        }

        internal static bool IsParameterValid(ParameterInfo pi, Type sourceType)
        {
            Type[] interfaces = pi.ParameterType.GetInterfaces();
            if (interfaces.Contains(typeof(IDictionary)))
            {
                return false;
            }
            if (interfaces.Contains(typeof(IEnumerable)))
            {
                return true;
            }
            bool flag = TypeUtils.AreEquivalent(pi.ParameterType, typeof(object)) || TypeUtils.IsSameOrSubclass(pi.ParameterType, sourceType);
            if (!flag && pi.ParameterType.IsInterface)
            {
                flag = SourceValidator.InterfacesSameOrSub(pi.ParameterType, sourceType);
            }
            return (!flag && sourceType != null && pi.ParameterType.IsGenericParameter && !interfaces.Contains(typeof(IEnumerable))) || ((pi.ParameterType.IsValueType || !(sourceType != null) || flag) && (flag || SourceValidator.IsAcceptedValueType(pi.ParameterType)) && !pi.IsOptional && !pi.IsOut);
        }

        internal static bool InterfacesSameOrSub(Type compareToInterface, Type compareWithInterface)
        {
            Type[] interfaces = compareWithInterface.GetInterfaces();
            Type type = null;
            Type[] array = interfaces;
            for (int i = 0; i < array.Length; i++)
            {
                Type type2 = array[i];
                if (type2 == compareToInterface)
                {
                    type = type2;
                    break;
                }
            }
            return type != null && TypeUtils.AreEquivalent(compareToInterface, type);
        }

        internal static bool IsAcceptedValueType(Type type)
        {
            return !(type == typeof(Guid)) && (type == typeof(string) || (type.IsValueType && (type.IsEnum || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(decimal) || type.IsPrimitive || type.IsGenericType)));
        }

        internal static bool IsOperatorTypeUsed(XmlNodeList fields, OperatorType type, out bool isNullable)
		{
			bool result = false;
			bool flag = false;
			foreach (XmlNode xmlNode in fields)
			{
				if (xmlNode.NodeType != XmlNodeType.Comment)
				{
					string name;
					if ((name = xmlNode.Name) != null && name == "function")
					{
                        IEnumerator enumerator2 = xmlNode.ChildNodes.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							XmlNode xmlNode2 = (XmlNode)enumerator2.Current;
							if (xmlNode2.NodeType != XmlNodeType.Comment && xmlNode2.Name == "returns" && (type == OperatorType.Bool || type == Converter.ClientStringToClientType(xmlNode2.Attributes["type"].Value)))
							{
								if (!flag && xmlNode2.Attributes["nullable"].Value == "true")
								{
									flag = true;
								}
								result = true;
							}
						}
						continue;
					}
					OperatorType operatorType = Converter.ClientStringToClientType(xmlNode.Name);
					if (type == OperatorType.Bool || type == operatorType)
					{
						if (operatorType == OperatorType.Collection)
						{
							flag = true;
						}
						else if (!flag && xmlNode.Attributes["nullable"].Value == "true")
						{
							flag = true;
						}
						result = true;
					}
				}
			}
			isNullable = flag;
			return result;
		}

        private static void ValidateAction(XmlNode action)
        {
            if (action.ChildNodes.Count > 0)
            {
                foreach (XmlNode xmlNode in action.ChildNodes[0].ChildNodes)
                {
                    string name;
                    if ((name = xmlNode.Name) != null)
                    {
                        if (!(name == "constant"))
                        {
                            if (!(name == "input"))
                            {
                                if (name == "collection")
                                {
                                    if (xmlNode.ChildNodes.Count != 1)
                                    {
                                        throw new SourceException(SourceException.ErrorIds.InvalidCollectionParameterXML, new string[0]);
                                    }
                                    if (Converter.StringToCollectionType(xmlNode.ChildNodes[0].Name) == CollectionType.Value)
                                    {
                                        SourceValidator.ValidateFieldDataType(xmlNode.ChildNodes[0], Converter.ClientStringToClientType(xmlNode.ChildNodes[0].Attributes["type"].Value));
                                    }
                                }
                            }
                            else
                            {
                                switch (Converter.ClientStringToClientType(xmlNode.Attributes["type"].Value))
                                {
                                    case OperatorType.String:
                                    case OperatorType.Numeric:
                                        SourceValidator.ValidateFieldDataType(xmlNode, Converter.ClientStringToClientType(xmlNode.Attributes["type"].Value));
                                        break;
                                }
                            }
                        }
                        else
                        {
                            SourceValidator.ValidateConstantParam(xmlNode);
                        }
                    }
                }
            }
        }

        private static void ValidateConstantParam(XmlNode param)
        {
            string value;
            if ((value = param.Attributes["type"].Value) != null)
            {
                if (!(value == "string"))
                {
                    if (!(value == "bool"))
                    {
                        if (!(value == "date"))
                        {
                            if (!(value == "time"))
                            {
                                if (!(value == "numeric"))
                                {
                                    if (!(value == "collection"))
                                    {
                                        return;
                                    }
                                    throw new MalformedXmlException(MalformedXmlException.ErrorIds.CollectionsAsConstantParams, new string[0]);
                                }
                                else
                                {
                                    decimal num = 0m;
                                    if (!decimal.TryParse(param.Attributes["value"].Value, out num))
                                    {
                                        throw new MalformedXmlException(MalformedXmlException.ErrorIds.InvalidNumericParamFormat, new string[0]);
                                    }
                                }
                            }
                            else
                            {
                                DateTime minValue = DateTime.MinValue;
                                if (!DateTime.TryParse("01/01/2010 " + param.Attributes["value"].Value, out minValue))
                                {
                                    throw new MalformedXmlException(MalformedXmlException.ErrorIds.InvalidTimeParamValue, new string[0]);
                                }
                            }
                        }
                        else
                        {
                            DateTime minValue2 = DateTime.MinValue;
                            if (!DateTime.TryParse(param.Attributes["value"].Value, Thread.CurrentThread.CurrentCulture, DateTimeStyles.None, out minValue2))
                            {
                                throw new MalformedXmlException(MalformedXmlException.ErrorIds.InvalidDateParamValue, new string[0]);
                            }
                        }
                    }
                    else if (param.Attributes["value"].Value != "true" && param.Attributes["value"].Value != "false")
                    {
                        throw new MalformedXmlException(MalformedXmlException.ErrorIds.InvalidBoolParamValue, new string[0]);
                    }
                }
                else if ((long)param.Attributes["value"].Value.Trim().Length > 256L)
                {
                    throw new MalformedXmlException(MalformedXmlException.ErrorIds.InvalidStringParamLength, new string[0]);
                }
            }
        }

        private static void ValidateFieldDataType(XmlNode field, OperatorType type)
        {
            switch (Converter.ClientStringToClientType((field.Attributes["type"] == null) ? field.Name : field.Attributes["type"].Value))
            {
                case OperatorType.String:
                    if (field.Attributes["valueInputType"] != null && field.Attributes["valueInputType"].Value != "fields")
                    {
                        int num = 0;
                        if (!int.TryParse(field.Attributes["maxLength"].Value, out num))
                        {
                            throw new MalformedXmlException(MalformedXmlException.ErrorIds.MissingMaxlengthAttribute, new string[0]);
                        }
                        if ((long)num > 256L)
                        {
                            throw new MalformedXmlException(MalformedXmlException.ErrorIds.MissingMaxlengthAttribute, new string[0]);
                        }
                    }
                    break;
                case OperatorType.Numeric:
                    {
                        decimal d = 0m;
                        decimal d2 = 0m;
                        if (!decimal.TryParse(field.Attributes["min"].Value, out d))
                        {
                            throw new MalformedXmlException(MalformedXmlException.ErrorIds.MissingMinAttribute, new string[0]);
                        }
                        if (!decimal.TryParse(field.Attributes["max"].Value, out d2))
                        {
                            throw new MalformedXmlException(MalformedXmlException.ErrorIds.MissingMaxAttribute, new string[0]);
                        }
                        if (d >= d2)
                        {
                            throw new MalformedXmlException(MalformedXmlException.ErrorIds.InvalidMinValue, new string[0]);
                        }
                        break;
                    }
                default:
                    return;
            }
        }
    }
}
