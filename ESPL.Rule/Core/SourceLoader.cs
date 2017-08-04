using ESPL.Rule.Attributes;
using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Core
{
    internal sealed class SourceLoader
	{
		private class Member
		{
			public MemberInfo Info
			{
				get;
				set;
			}

			public Type Type
			{
				get;
				set;
			}

			public bool Settable
			{
				get;
				set;
			}

			public Member(MemberInfo mi, Type t, bool setable)
			{
				this.Type = t;
				this.Info = mi;
				this.Settable = setable;
			}
		}

		private enum ClientType
		{
			Enum,
			Numeric,
			String,
			Date,
			Time,
			Bool,
			Other
		}

		private SourceLoader()
		{
		}

		internal static XmlDocument GetXml(string assembly, string type, List<Type> processedTypes)
		{
			return SourceLoader.GetXml(SourceLoader.LoadType(assembly, type, SourceException.ErrorIds.FailureToLoadSourceXML, SourceException.ErrorIds.AssemlbyDoesNotDeclareSourceType), processedTypes);
		}

		internal static XmlDocument GetXml(Type type, List<Type> processedTypes)
		{
			XmlDocument emptySourceDocument = Xml.GetEmptySourceDocument();
			SourceLoader.Extract(emptySourceDocument, type, processedTypes);
			return emptySourceDocument;
		}

		internal static string GetTokenByRuleMethodNode(XmlNode source, XmlNode method, bool isMethod)
		{
			string value = method.Attributes["name"].Value;
			Type sourceObject = SourceLoader.GetSourceObject(source);
			Type type;
			if (method.Attributes["type"] != null)
			{
				type = Type.GetType(method.Attributes["type"].Value);
			}
			else
			{
				type = sourceObject;
			}
			return SourceLoader.GetTokenByMethod(source, type, sourceObject, value, method.ChildNodes, isMethod);
		}

		internal static string GetTokenBySourceMethodNode(XmlNode source, XmlNode method)
		{
			string value = method.Attributes["methodName"].Value;
			Type sourceObject = SourceLoader.GetSourceObject(source);
			Type type;
			if (method.Attributes["class"] != null)
			{
				if (method.Attributes["assembly"] != null)
				{
					type = Type.GetType(method.Attributes["class"].Value + ", " + method.Attributes["assembly"].Value);
				}
				else
				{
					type = Type.GetType(method.Attributes["class"].Value);
				}
			}
			else
			{
				type = sourceObject;
			}
			return SourceLoader.GetTokenByMethod(source, type, sourceObject, value, SourceLoader.GetParamNode(method).ChildNodes, method.ChildNodes.Count > 1);
		}

		internal static XmlNode GetActionByToken(XmlNode source, string token)
		{
			return SourceLoader.GetMethodByToken(source, token, false, MalformedXmlException.ErrorIds.MissingActionsNode);
		}

		internal static XmlNode GetFunctionByToken(XmlNode source, string token)
		{
			return SourceLoader.GetMethodByToken(source, token, true, MalformedXmlException.ErrorIds.MissingFieldsNode);
		}

		internal static XmlNode GetFieldByPropertyName(XmlNode source, string propertyName)
		{
			XmlNode xmlNode = source.SelectSingleNode(string.Format("{0}:fields", "s"), Xml.GetSourceNamespaceManager(source));
			if (xmlNode == null || xmlNode.ChildNodes.Count == 0)
			{
				throw new MalformedXmlException(MalformedXmlException.ErrorIds.MissingFieldsNode, new string[0]);
			}
			foreach (XmlNode xmlNode2 in xmlNode.ChildNodes)
			{
				if (xmlNode2.Attributes["propertyName"] != null && xmlNode2.Attributes["propertyName"].Value == propertyName)
				{
					return xmlNode2;
				}
			}
			throw new MalformedXmlException(MalformedXmlException.ErrorIds.MissingFieldsWithPropertyName, new string[]
			{
				propertyName
			});
		}

		internal static Type GetUnderlyingType(Type collectionType, Type userDefinedType)
		{
			Type result = null;
			if (collectionType.IsArray)
			{
				result = collectionType.GetElementType();
			}
			else
			{
				Type baseGenericCollectionType = SourceLoader.GetBaseGenericCollectionType(collectionType);
				if (baseGenericCollectionType != null)
				{
					Type[] genericArguments = baseGenericCollectionType.GetGenericArguments();
					if (genericArguments.Length > 0)
					{
						result = genericArguments[0];
					}
					else
					{
						result = baseGenericCollectionType;
					}
				}
				else if (userDefinedType == null)
				{
					if (collectionType == typeof(BitArray) || collectionType == typeof(BitVector32))
					{
						result = typeof(bool);
					}
					else if (collectionType == typeof(StringCollection))
					{
						result = typeof(string);
					}
				}
				else
				{
					result = userDefinedType;
				}
			}
			return result;
		}

		private static Type GetBaseGenericCollectionType(Type type)
		{
			if (type.IsGenericType)
			{
				return type;
			}
			Type[] interfaces = type.GetInterfaces();
			if (interfaces.Contains(typeof(IEnumerable)))
			{
				Type[] array = interfaces;
				for (int i = 0; i < array.Length; i++)
				{
					Type type2 = array[i];
					Type[] genericArguments = type2.GetGenericArguments();
					if (genericArguments.Length > 0)
					{
						return genericArguments[0];
					}
				}
			}
			if (type.BaseType != null)
			{
				return SourceLoader.GetBaseGenericCollectionType(type.BaseType);
			}
			return null;
		}

		private static Type GetBaseGenericType(Type type)
		{
			if (type.IsGenericType)
			{
				return type;
			}
			Type[] interfaces = type.GetInterfaces();
			if (interfaces.Contains(typeof(IEnumerable)))
			{
				Type[] array = interfaces;
				for (int i = 0; i < array.Length; i++)
				{
					Type type2 = array[i];
					if (type2.IsGenericType)
					{
						return type2;
					}
				}
			}
			if (type.BaseType != null)
			{
				return SourceLoader.GetBaseGenericType(type.BaseType);
			}
			return null;
		}

		internal static XmlNode GetParamNode(XmlNode function)
		{
			if (function == null)
			{
				return null;
			}
			foreach (XmlNode xmlNode in function.ChildNodes)
			{
				if (xmlNode.Name == "parameters")
				{
					return xmlNode;
				}
			}
			return null;
		}

		internal static XmlNode GetReturnNode(XmlNode function)
		{
			foreach (XmlNode xmlNode in function.ChildNodes)
			{
				if (xmlNode.Name == "returns")
				{
					return xmlNode;
				}
			}
			throw new MalformedXmlException(MalformedXmlException.ErrorIds.TooManyOrMissingReturn, new string[0]);
		}

		internal static XmlDocument LoadSourceXml(string sourceAssembly, string sourceType, string sourceXmlFile, List<Type> processedTypes)
		{
			if (!string.IsNullOrEmpty(sourceType))
			{
				return SourceLoader.GetXml(sourceAssembly, sourceType, processedTypes);
			}
			if (!string.IsNullOrEmpty(sourceXmlFile))
			{
				try
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(sourceXmlFile);
					return xmlDocument;
				}
				catch
				{
					throw new MalformedXmlException(MalformedXmlException.ErrorIds.MalformedOrMissingSourceXML, new string[0]);
				}
			}
			throw new SourceException(SourceException.ErrorIds.FailureToLoadSourceXML, new string[0]);
		}

		internal static List<DataSource> GetDataSources(XmlDocument sourceXml)
		{
			List<DataSource> list = new List<DataSource>();
			foreach (XmlNode xmlNode in sourceXml.DocumentElement.ChildNodes)
			{
				XmlNode xmlNode2 = xmlNode.SelectSingleNode(string.Format("{0}:dataSources", "s"), Xml.GetSourceNamespaceManager(sourceXml));
				if (xmlNode2 != null && xmlNode2.ChildNodes.Count > 0)
				{
                    IEnumerator enumerator2 = xmlNode2.ChildNodes.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						XmlNode xmlNode3 = (XmlNode)enumerator2.Current;
						DataSource d = new DataSource();
						d.Name = xmlNode3.Attributes["name"].Value;
						if (list.Any((DataSource s) => s.Name == d.Name))
						{
							throw new SourceException(SourceException.ErrorIds.MultipleDynamicMenuDataSources, new string[0]);
						}
						d.Location = Converter.StringToFeatureLocation(xmlNode3.Attributes["location"].Value);
						d.Method = xmlNode3.Attributes["methodName"].Value;
						if (d.Location != FeatureLocation.Client)
						{
							d.Assembly = xmlNode3.Attributes["assembly"].Value;
							d.Class = xmlNode3.Attributes["class"].Value;
						}
						list.Add(d);
					}
				}
			}
			return list;
		}

		internal static XmlNode GetSourceNode(XmlDocument sourceXml, string sourceName)
		{
			foreach (XmlNode xmlNode in sourceXml.DocumentElement.ChildNodes)
			{
				if (sourceName == null || xmlNode.Attributes["name"] == null || xmlNode.Attributes["name"].Value == sourceName)
				{
					return xmlNode;
				}
			}
			return null;
		}

		internal static XmlNode GetSourceNodeByToken(XmlDocument sourceXml, string token)
		{
			foreach (XmlNode xmlNode in sourceXml.DocumentElement.ChildNodes)
			{
				if (token == null || xmlNode.Attributes["name"] == null || Encoder.GetHashToken(xmlNode.Attributes["name"].Value) == token)
				{
					return xmlNode;
				}
			}
			return null;
		}

		private static void Extract(XmlDocument source, Type type, List<Type> processedTypes)
		{
			if (processedTypes.Contains(type))
			{
				return;
			}
			processedTypes.Add(type);
			foreach (XmlNode xmlNode in source.DocumentElement.ChildNodes)
			{
				if (xmlNode.Attributes["name"] != null && xmlNode.Attributes["name"].Value == type.FullName)
				{
					return;
				}
			}
			XmlElement xmlElement = source.CreateElement("source", "http://codeeffects.com/schemas/source/42");
			XmlElement xmlElement2 = source.CreateElement("fields", "http://codeeffects.com/schemas/source/42");
			XmlElement xmlElement3 = source.CreateElement("actions", "http://codeeffects.com/schemas/source/42");
			XmlElement xmlElement4 = source.CreateElement("dataSources", "http://codeeffects.com/schemas/source/42");
			xmlElement.SetAttribute("name", type.FullName);
			xmlElement.SetAttribute("webrule", Assembly.GetExecutingAssembly().GetName().Version.ToString());
			xmlElement.SetAttribute("utc", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffff"));
			xmlElement.SetAttribute("type", type.AssemblyQualifiedName);
			object[] array = type.GetCustomAttributes(true);
			SourceState sourceState = SourceLoader.ManageSourceAttribute(array);
			if (!sourceState.DeclaredMembersOnly && array.Length > 0)
			{
				List<object> list = array.ToList<object>();
				Type[] interfaces = type.GetInterfaces();
				Type[] array2 = interfaces;
				for (int i = 0; i < array2.Length; i++)
				{
					Type type2 = array2[i];
					object[] customAttributes = type2.GetCustomAttributes(true);
					object[] array3 = customAttributes;
					for (int j = 0; j < array3.Length; j++)
					{
						object obj = array3[j];
						if (obj is ExternalMethodAttribute || obj is ExternalActionAttribute || obj is DataAttribute || obj is SourceAttribute)
						{
							list.Add(obj);
						}
					}
				}
				if (list.Count > 0)
				{
					array = list.ToArray<object>();
				}
			}
			xmlElement.SetAttribute("persisted", sourceState.Persisted ? "true" : "false");
			Dictionary<string, XmlElement> dictionary = new Dictionary<string, XmlElement>();
			Dictionary<string, XmlElement> dictionary2 = new Dictionary<string, XmlElement>();
			Dictionary<string, XmlElement> dictionary3 = new Dictionary<string, XmlElement>();
			object[] array4 = array;
			for (int k = 0; k < array4.Length; k++)
			{
				object obj2 = array4[k];
				if (obj2 is ExternalMethodAttribute)
				{
					SourceLoader.ManageExternalMethodAttribute((ExternalMethodAttribute)obj2, source, dictionary, type, processedTypes);
				}
				else if (obj2 is ExternalActionAttribute)
				{
					SourceLoader.ManageExternalMethodAttribute((ExternalActionAttribute)obj2, source, dictionary2, type, processedTypes);
				}
				else if (obj2 is DataAttribute)
				{
					SourceLoader.ManageDataAttribute((DataAttribute)obj2, source, dictionary3);
				}
			}
			SourceLoader.ManageProperties(type, null, null, source, dictionary, 1, sourceState, processedTypes);
			SourceLoader.ManageMethods(type, source, dictionary, dictionary2, sourceState, processedTypes);
			if (dictionary.Count == 0)
			{
				throw new SourceException(SourceException.ErrorIds.MissingPublicValueTypeFields, new string[]
				{
					type.FullName
				});
			}
			foreach (string current in dictionary.Keys)
			{
				xmlElement2.AppendChild(dictionary[current]);
			}
			dictionary.Clear();
			xmlElement.AppendChild(xmlElement2);
			if (dictionary2.Count > 0)
			{
				foreach (string current2 in dictionary2.Keys)
				{
					xmlElement3.AppendChild(dictionary2[current2]);
				}
				dictionary2.Clear();
				xmlElement.AppendChild(xmlElement3);
			}
			if (dictionary3.Count > 0)
			{
				foreach (string current3 in dictionary3.Keys)
				{
					xmlElement4.AppendChild(dictionary3[current3]);
				}
				dictionary3.Clear();
				xmlElement.AppendChild(xmlElement4);
			}
			source.DocumentElement.PrependChild(xmlElement);
		}

		private static SourceState ManageSourceAttribute(object[] attributes)
		{
			SourceState sourceState = new SourceState();
			object attribute = SourceLoader.GetAttribute<SourceAttribute>(attributes);
			SourceAttribute sourceAttribute;
			if (attribute == null)
			{
				sourceAttribute = new SourceAttribute();
			}
			else
			{
				sourceAttribute = (SourceAttribute)attribute;
			}
			sourceState.MaxLevel = sourceAttribute.MaxTypeNestingLevel;
			sourceState.DeclaredMembersOnly = sourceAttribute.DeclaredMembersOnly;
			sourceState.Persisted = sourceAttribute.PersistTypeNameInRuleXml;
			return sourceState;
		}

		private static void ManageMethods(Type type, XmlDocument doc, IDictionary<string, XmlElement> fields, IDictionary<string, XmlElement> actions, SourceState state, List<Type> processedTypes)
		{
			Type[] interfaces = type.GetInterfaces();
			List<MethodInfo> methods = SourceLoader.GetMethods(type, state);
			foreach (MethodInfo current in methods)
			{
				if (!current.IsSpecialName)
				{
					Type[] methodParameterTypes = SourceLoader.GetMethodParameterTypes(current);
					if (current.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), true).Length == 0 && !SourceLoader.IsMethodExcludedInInterface(interfaces, current.Name, methodParameterTypes))
					{
						object[] customAttributes = current.GetCustomAttributes(false);
						IDescribableAttribute describableAttribute = null;
						object[] array = customAttributes;
						for (int i = 0; i < array.Length; i++)
						{
							object obj = array[i];
							if (obj is MethodAttribute || obj is ActionAttribute)
							{
								describableAttribute = (IDescribableAttribute)obj;
								break;
							}
						}
						if (describableAttribute == null)
						{
							describableAttribute = SourceLoader.GetInterfaceDescribableAttribute(interfaces, current.Name, methodParameterTypes);
						}
						if (describableAttribute == null)
						{
							bool flag = current.ReturnType == typeof(void);
							if (SourceValidator.IsMethodValid(current, !flag))
							{
								if (flag)
								{
									SourceLoader.ManageMethod(current, null, doc, actions, type, false, processedTypes);
								}
								else if (current.ReturnType.IsValueType || current.ReturnType == typeof(string))
								{
									SourceLoader.ManageMethod(current, null, doc, fields, type, true, processedTypes);
								}
							}
						}
						else
						{
							bool flag2 = describableAttribute is MethodAttribute;
							if (!SourceValidator.IsMethodValid(current, flag2))
							{
								throw new SourceException(SourceException.ErrorIds.MethodIsDecoratedButInvalid, new string[]
								{
									current.Name
								});
							}
							if (flag2)
							{
								SourceLoader.ManageMethod(current, describableAttribute, doc, fields, type, true, processedTypes);
							}
							else
							{
								SourceLoader.ManageMethod(current, describableAttribute, doc, actions, type, false, processedTypes);
							}
						}
					}
				}
			}
		}

		private static void ManageMethod(MethodInfo m, IDescribableAttribute attr, XmlDocument doc, IDictionary<string, XmlElement> parent, Type sourceType, bool isMethod, List<Type> processedTypes)
		{
			XmlElement xmlElement = doc.CreateElement(isMethod ? "function" : "action", "http://codeeffects.com/schemas/source/42");
			xmlElement.SetAttribute("methodName", m.Name);
			xmlElement.SetAttribute("displayName", (attr == null || string.IsNullOrEmpty(attr.DisplayName)) ? m.Name : Encoder.Sanitize(attr.DisplayName));
			if (attr != null && !string.IsNullOrWhiteSpace(attr.Description))
			{
				xmlElement.SetAttribute("description", Encoder.Sanitize(attr.Description));
			}
			if (isMethod)
			{
				if (attr != null)
				{
					if (!((MethodAttribute)attr).Gettable)
					{
						xmlElement.SetAttribute("gettable", "false");
					}
					bool flag = false;
					bool flag2 = false;
					if (SourceLoader.GetClientType(m.ReturnType, out flag, out flag2) == SourceLoader.ClientType.Numeric)
					{
						xmlElement.SetAttribute("includeInCalculations", ((MethodAttribute)attr).IncludeInCalculations ? "true" : "false");
					}
				}
				else
				{
					xmlElement.SetAttribute("includeInCalculations", "true");
				}
			}
			XmlElement xmlElement2 = doc.CreateElement("parameters", "http://codeeffects.com/schemas/source/42");
			xmlElement.AppendChild(xmlElement2);
			if (!SourceLoader.ManageParameters(m.GetParameters(), doc, xmlElement2, m, sourceType, attr != null, processedTypes))
			{
				return;
			}
			if (isMethod)
			{
				XmlElement xmlElement3 = doc.CreateElement("returns", "http://codeeffects.com/schemas/source/42");
				xmlElement.AppendChild(xmlElement3);
				try
				{
					SourceLoader.ManageReturn(m, xmlElement3);
				}
				catch (SourceException)
				{
					if (attr != null)
					{
						throw;
					}
					return;
				}
			}
			try
			{
				parent.Add((attr == null || string.IsNullOrEmpty(attr.DisplayName)) ? m.Name : Encoder.Sanitize(attr.DisplayName), xmlElement);
			}
			catch (ArgumentException)
			{
				if (attr != null)
				{
					throw new SourceException(SourceException.ErrorIds.MethodHasOverloads, new string[]
					{
						m.Name
					});
				}
			}
		}

		private static void ManageExternalMethodAttribute(IExternalAttribute attr, XmlDocument doc, IDictionary<string, XmlElement> parent, Type sourceType, List<Type> processedTypes)
		{
			bool flag = attr is ExternalMethodAttribute;
			Type type;
			if (attr.Type == null)
			{
				type = SourceLoader.LoadType(attr.Assembly, attr.TypeName, SourceException.ErrorIds.AssemblyNameIsEmpty, SourceException.ErrorIds.AssemblyDoesNotContainType);
			}
			else
			{
				type = attr.Type;
			}
			Type[] interfaces = type.GetInterfaces();
			List<MethodInfo> methods = SourceLoader.GetMethods(type);
			foreach (MethodInfo current in methods)
			{
				if (!(current.Name != attr.Method) && current.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), true).Length == 0 && !SourceLoader.IsMethodExcludedInInterface(interfaces, current.Name, SourceLoader.GetMethodParameterTypes(current)) && SourceValidator.IsMethodValid(current, flag))
				{
					IDescribableAttribute describableAttribute = null;
					string text = null;
					string text2 = null;
					object[] customAttributes;
					if (flag)
					{
						customAttributes = current.GetCustomAttributes(typeof(MethodAttribute), false);
					}
					else
					{
						customAttributes = current.GetCustomAttributes(typeof(ActionAttribute), false);
					}
					if (customAttributes.Length > 0)
					{
						describableAttribute = (IDescribableAttribute)customAttributes[0];
						text2 = describableAttribute.DisplayName;
						text = describableAttribute.Description;
					}
					else
					{
						text2 = attr.Method;
					}
					XmlElement xmlElement = doc.CreateElement(flag ? "function" : "action", "http://codeeffects.com/schemas/source/42");
					xmlElement.SetAttribute("methodName", attr.Method);
					xmlElement.SetAttribute("displayName", Encoder.Sanitize(text2));
					if (!string.IsNullOrWhiteSpace(text))
					{
						xmlElement.SetAttribute("description", Encoder.Sanitize(text));
					}
					if (attr.Type == null)
					{
						xmlElement.SetAttribute("class", attr.TypeName);
						xmlElement.SetAttribute("assembly", attr.Assembly);
					}
					else
					{
						xmlElement.SetAttribute("class", attr.Type.FullName);
						xmlElement.SetAttribute("assembly", Assembly.GetAssembly(attr.Type).FullName);
					}
					if (flag)
					{
						ExternalMethodAttribute externalMethodAttribute = (ExternalMethodAttribute)attr;
						if (!externalMethodAttribute.Gettable)
						{
							xmlElement.SetAttribute("gettable", "false");
						}
					}
					XmlElement xmlElement2 = doc.CreateElement("parameters", "http://codeeffects.com/schemas/source/42");
					xmlElement.AppendChild(xmlElement2);
					if (!SourceLoader.ManageParameters(current.GetParameters(), doc, xmlElement2, current, sourceType, describableAttribute != null, processedTypes))
					{
						break;
					}
					if (flag)
					{
						XmlElement xmlElement3 = doc.CreateElement("returns", "http://codeeffects.com/schemas/source/42");
						xmlElement.AppendChild(xmlElement3);
						try
						{
							SourceLoader.ManageReturn(current, xmlElement3);
						}
						catch (SourceException)
						{
							if (describableAttribute != null)
							{
								throw;
							}
							break;
						}
					}
					try
					{
						parent.Add(text2, xmlElement);
					}
					catch (ArgumentException)
					{
						if (describableAttribute != null)
						{
							throw new SourceException(SourceException.ErrorIds.ExternalMethodHasOverloads, new string[]
							{
								current.Name
							});
						}
					}
				}
			}
		}

		private static void ManageDataAttribute(DataAttribute attr, XmlDocument doc, Dictionary<string, XmlElement> parent)
		{
			XmlElement xmlElement = doc.CreateElement("dataSource", "http://codeeffects.com/schemas/source/42");
			xmlElement.SetAttribute("name", attr.Name);
			xmlElement.SetAttribute("methodName", attr.Method);
			xmlElement.SetAttribute("location", Converter.FeatureLocationToString(attr.Location));
			if (attr.Location != FeatureLocation.Client)
			{
				Type type = null;
				if (attr.Type == null)
				{
					type = SourceLoader.LoadType(attr.Assembly, attr.TypeName, SourceException.ErrorIds.AssemblyNameIsEmpty, SourceException.ErrorIds.AssemblyDoesNotContainType);
				}
				else
				{
					type = attr.Type;
				}
				Type[] interfaces = type.GetInterfaces();
				List<MethodInfo> methods = SourceLoader.GetMethods(type);
				bool flag = false;
				foreach (MethodInfo current in methods)
				{
					if (!(current.Name != attr.Method) && current.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), true).Length == 0 && !SourceLoader.IsMethodExcludedInInterface(interfaces, current.Name, SourceLoader.GetMethodParameterTypes(current)) && SourceValidator.IsDataSourceMethodValid(current))
					{
						xmlElement.SetAttribute("class", type.FullName);
						xmlElement.SetAttribute("assembly", Assembly.GetAssembly(type).FullName);
						flag = true;
					}
				}
				if (!flag)
				{
					throw new SourceException(SourceException.ErrorIds.InvalidDataSourceMethod, new string[]
					{
						type.FullName,
						attr.Method
					});
				}
			}
			try
			{
				parent.Add(attr.Name, xmlElement);
			}
			catch (ArgumentException)
			{
				throw new SourceException(SourceException.ErrorIds.DuplicateDataAttributes, new string[]
				{
					attr.Name
				});
			}
		}

		private static void ManageProperties(Type type, string name, string parent, XmlDocument doc, IDictionary<string, XmlElement> fields, int level, SourceState state, List<Type> processedTypes)
		{
			Type[] interfaces = type.GetInterfaces();
			BindingFlags bindingAttr = state.DeclaredMembersOnly ? (BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) : (BindingFlags.Instance | BindingFlags.Public);
			FieldInfo[] fields2 = type.GetFields(bindingAttr);
			PropertyInfo[] properties = type.GetProperties(bindingAttr);
			Dictionary<string, SourceLoader.Member> dictionary = new Dictionary<string, SourceLoader.Member>(fields2.Length + properties.Length);
			int num = 0;
			FieldInfo[] array = fields2;
			for (int i = 0; i < array.Length; i++)
			{
				FieldInfo fieldInfo = array[i];
				Dictionary<string, SourceLoader.Member> arg_86_0 = dictionary;
				int num2;
				num = (num2 = num + 1);
				arg_86_0.Add(num2.ToString(), new SourceLoader.Member(fieldInfo, fieldInfo.FieldType, !fieldInfo.IsLiteral && !fieldInfo.IsInitOnly && !SourceLoader.IsReadOnly(fieldInfo)));
			}
			PropertyInfo[] array2 = properties;
			for (int j = 0; j < array2.Length; j++)
			{
				PropertyInfo propertyInfo = array2[j];
				if (propertyInfo.GetGetMethod() != null)
				{
					Dictionary<string, SourceLoader.Member> arg_F3_0 = dictionary;
					int num3;
					num = (num3 = num + 1);
					arg_F3_0.Add(num3.ToString(), new SourceLoader.Member(propertyInfo, propertyInfo.PropertyType, propertyInfo.GetSetMethod() != null && !SourceLoader.IsReadOnly(propertyInfo)));
				}
			}
			foreach (string current in dictionary.Keys)
			{
				Type type2 = dictionary[current].Type;
				Type[] interfaces2 = type2.GetInterfaces();
				if (!interfaces2.Contains(typeof(IDictionary)))
				{
					bool flag = interfaces2.Contains(typeof(IEnumerable));
					if (!(type2 == typeof(object)) && !(type2 == typeof(Guid)) && (type2.IsValueType || !type2.IsGenericType || flag) && !type2.IsGenericTypeDefinition && !type2.IsImport && (!type2.IsAbstract || type2.IsInterface || flag) && !type2.IsMarshalByRef && !type2.IsSpecialName)
					{
						MemberInfo info = dictionary[current].Info;
						XmlElement xmlElement = null;
						if ((info.MemberType == MemberTypes.Property || info.MemberType == MemberTypes.Field) && info.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), true).Length == 0 && !SourceLoader.IsPropertyExcludedInInterface(interfaces, info.Name))
						{
							if (!flag || !(type2 != typeof(string)))
							{
								goto IL_439;
							}
							xmlElement = doc.CreateElement("collection", "http://codeeffects.com/schemas/source/42");
							xmlElement.SetAttribute("propertyName", SourceLoader.BuildName(info, name));
							FieldAttribute fieldAttribute = SourceLoader.GetFieldAttribute(info, interfaces);
							SourceLoader.SetDisplayName(info, xmlElement, fieldAttribute, null, name, null);
							string value = xmlElement.Attributes["displayName"].Value;
							XmlElement xmlElement2 = SourceLoader.BuildItemElement(doc, type2, fieldAttribute, value, false, processedTypes);
							if (xmlElement2 != null)
							{
								Type type3 = SourceLoader.GetBaseGenericType(type2);
								if (type3 == null)
								{
									type3 = type2;
								}
								xmlElement.SetAttribute("generic", type3.IsGenericType ? "true" : "false");
								xmlElement.SetAttribute("array", type2.IsArray ? "true" : "false");
								string text = type2.FullName;
								if (type3 != type2 && !type2.FullName.Contains(","))
								{
									text = text + ", " + Assembly.GetAssembly(type2).FullName;
								}
								xmlElement.SetAttribute("class", text);
								if (fieldAttribute != null && !fieldAttribute.Gettable)
								{
									xmlElement.SetAttribute("gettable", "false");
								}
								if (xmlElement2.Name == "value")
								{
									if (fieldAttribute != null)
									{
										xmlElement.SetAttribute("valueInputType", Converter.ValueInputTypeToString(fieldAttribute.ValueInputType));
									}
									else
									{
										xmlElement.SetAttribute("valueInputType", Converter.ValueInputTypeToString(ValueInputType.All));
									}
								}
								if (!type2.IsArray && type3.IsGenericType)
								{
									xmlElement.SetAttribute("comparisonName", SourceLoader.GetComparisonName(type3));
								}
								else
								{
									xmlElement.SetAttribute("comparisonName", type2.FullName);
								}
								xmlElement.AppendChild(xmlElement2);
								try
								{
									fields.Add(value, xmlElement);
									goto IL_92D;
								}
								catch (ArgumentException)
								{
									goto IL_92D;
								}
								goto IL_439;
							}
							continue;
							IL_92D:
							if (xmlElement != null)
							{
								try
								{
									if (!fields.ContainsKey(xmlElement.Attributes["displayName"].Value))
									{
										fields.Add(xmlElement.Attributes["displayName"].Value, xmlElement);
									}
									continue;
								}
								catch (ArgumentException)
								{
									continue;
								}
							}
							if (level < state.MaxLevel && !flag && !type2.IsEnum && type2 != typeof(DateTime) && type2 != typeof(DateTime?) && type2 != typeof(TimeSpan) && type2 != typeof(TimeSpan?) && type2 != typeof(Guid) && type2 != typeof(Guid?))
							{
								string parent2 = (parent == null) ? info.Name : parent;
								SourceLoader.ManageProperties(type2, SourceLoader.BuildName(info, name), parent2, doc, fields, level + 1, state, processedTypes);
								continue;
							}
							continue;
							IL_439:
							bool flag2 = false;
							bool flag3 = false;
							bool flag4 = dictionary[current].Settable;
							bool gettable = true;
							fieldAttribute = SourceLoader.GetFieldAttribute(info, interfaces);
							if (flag4 && fieldAttribute != null && !fieldAttribute.Settable)
							{
								flag4 = false;
							}
							if (fieldAttribute != null)
							{
								gettable = fieldAttribute.Gettable;
							}
							SourceLoader.ClientType clientType = SourceLoader.GetClientType(type2, out flag2, out flag3);
							if (clientType == SourceLoader.ClientType.Other)
							{
								goto IL_92D;
							}
							string parentName = null;
							string parentDescription = null;
							if (parent != null)
							{
								object[] array3 = info.GetCustomAttributes(typeof(ParentAttribute), true);
								if (array3 == null || array3.Length == 0)
								{
									array3 = SourceLoader.GetInterfaceParentAttributes(interfaces, info.Name);
								}
								if (array3 != null && array3.Length > 0)
								{
									object[] array4 = array3;
									int k = 0;
									while (k < array4.Length)
									{
										object obj = array4[k];
										ParentAttribute parentAttribute = (ParentAttribute)obj;
										if (parentAttribute.ParentName.Contains(parent))
										{
											parentName = parentAttribute.DisplayName;
											if (!string.IsNullOrWhiteSpace(parentAttribute.Description))
											{
												parentDescription = parentAttribute.Description;
												break;
											}
											break;
										}
										else
										{
											k++;
										}
									}
								}
							}
							switch (clientType)
							{
							case SourceLoader.ClientType.Enum:
								xmlElement = SourceLoader.GetPropertyElement(doc, "enum", type2, fieldAttribute, SourceLoader.BuildName(info, name), false, flag4, gettable);
								SourceLoader.SetDisplayName(info, xmlElement, fieldAttribute, parentName, name, parentDescription);
								xmlElement.SetAttribute("assembly", Assembly.GetAssembly(type2).FullName);
								goto IL_92D;
							case SourceLoader.ClientType.Numeric:
								xmlElement = SourceLoader.GetPropertyElement(doc, "numeric", type2, fieldAttribute, SourceLoader.BuildName(info, name), flag2, flag4, gettable);
								SourceLoader.SetDisplayName(info, xmlElement, fieldAttribute, parentName, name, parentDescription);
								if (fieldAttribute != null)
								{
									xmlElement.SetAttribute("allowCalculation", fieldAttribute.AllowCalculations ? "true" : "false");
								}
								else
								{
									xmlElement.SetAttribute("allowCalculation", "true");
								}
								if (fieldAttribute != null)
								{
									xmlElement.SetAttribute("includeInCalculations", fieldAttribute.IncludeInCalculations ? "true" : "false");
								}
								else
								{
									xmlElement.SetAttribute("includeInCalculations", "true");
								}
								xmlElement.SetAttribute("allowDecimal", flag2 ? SourceLoader.GetNullableDecimalByNumeric(type2) : SourceLoader.GetDecimalByNumeric(type2.Name.ToLower()));
								if (fieldAttribute != null && fieldAttribute.Min != -9223372036854775808L)
								{
									xmlElement.SetAttribute("min", fieldAttribute.Min.ToString());
								}
								else
								{
									xmlElement.SetAttribute("min", flag2 ? SourceLoader.GetNullableMinByNumeric(type2) : SourceLoader.GetMinByNumeric(type2.Name.ToLower()));
								}
								if (fieldAttribute != null && fieldAttribute.Max != 9223372036854775807L)
								{
									xmlElement.SetAttribute("max", fieldAttribute.Max.ToString());
								}
								else
								{
									xmlElement.SetAttribute("max", flag2 ? SourceLoader.GetNullableMaxByNumeric(type2) : SourceLoader.GetMaxByNumeric(type2.Name.ToLower()));
								}
								if (fieldAttribute != null && !string.IsNullOrWhiteSpace(fieldAttribute.DataSourceName) && flag3)
								{
									xmlElement.SetAttribute("dataSourceName", fieldAttribute.DataSourceName);
									goto IL_92D;
								}
								goto IL_92D;
							case SourceLoader.ClientType.String:
								xmlElement = SourceLoader.GetPropertyElement(doc, "string", type2, fieldAttribute, SourceLoader.BuildName(info, name), true, flag4, gettable);
								SourceLoader.SetDisplayName(info, xmlElement, fieldAttribute, parentName, name, parentDescription);
								if (fieldAttribute != null && fieldAttribute.Max != 9223372036854775807L && fieldAttribute.Max <= 256L)
								{
									xmlElement.SetAttribute("maxLength", fieldAttribute.Max.ToString());
								}
								else
								{
									xmlElement.SetAttribute("maxLength", 256L.ToString());
								}
								if (fieldAttribute != null && fieldAttribute.StringComparison != StringComparison.OrdinalIgnoreCase)
								{
									xmlElement.SetAttribute("stringComparison", fieldAttribute.StringComparison.ToString());
									goto IL_92D;
								}
								goto IL_92D;
							case SourceLoader.ClientType.Date:
								xmlElement = SourceLoader.GetPropertyElement(doc, "date", type2, fieldAttribute, SourceLoader.BuildName(info, name), flag2, flag4, gettable);
								SourceLoader.SetDisplayName(info, xmlElement, fieldAttribute, parentName, name, parentDescription);
								if (fieldAttribute != null && !string.IsNullOrEmpty(fieldAttribute.DateTimeFormat))
								{
									xmlElement.SetAttribute("format", Encoder.Sanitize(fieldAttribute.DateTimeFormat));
									goto IL_92D;
								}
								xmlElement.SetAttribute("format", "MMM dd, yyyy");
								goto IL_92D;
							case SourceLoader.ClientType.Time:
								xmlElement = SourceLoader.GetPropertyElement(doc, "time", type2, fieldAttribute, SourceLoader.BuildName(info, name), flag2, flag4, gettable);
								SourceLoader.SetDisplayName(info, xmlElement, fieldAttribute, parentName, name, parentDescription);
								if (fieldAttribute != null && !string.IsNullOrEmpty(fieldAttribute.DateTimeFormat))
								{
									xmlElement.SetAttribute("format", Encoder.Sanitize(fieldAttribute.DateTimeFormat));
									goto IL_92D;
								}
								xmlElement.SetAttribute("format", "hh:mm tt");
								goto IL_92D;
							case SourceLoader.ClientType.Bool:
								xmlElement = SourceLoader.GetPropertyElement(doc, "bool", type2, fieldAttribute, SourceLoader.BuildName(info, name), flag2, flag4, gettable);
								SourceLoader.SetDisplayName(info, xmlElement, fieldAttribute, parentName, name, parentDescription);
								goto IL_92D;
							default:
								goto IL_92D;
							}
						}
					}
				}
			}
		}

		private static bool ManageParameters(ParameterInfo[] pars, XmlDocument doc, XmlElement elParams, MethodInfo m, Type sourceType, bool attrExists, List<Type> processedTypes)
		{
			List<XmlElement> list = new List<XmlElement>();
			int num = 0;
			for (int i = 0; i < pars.Length; i++)
			{
				ParameterInfo parameterInfo = pars[i];
				if (parameterInfo.ParameterType.IsGenericParameter)
				{
					Type[] interfaces = parameterInfo.ParameterType.GetInterfaces();
					if (!interfaces.Contains(typeof(IEnumerable)))
					{
						num++;
					}
				}
			}
			if (num > 1)
			{
				if (attrExists)
				{
					throw new SourceException(SourceException.ErrorIds.InvalidMethodParameters, new string[]
					{
						m.Name
					});
				}
				return false;
			}
			else
			{
				int j = 0;
				while (j < pars.Length)
				{
					ParameterInfo parameterInfo2 = pars[j];
					if (!SourceValidator.IsParameterValid(parameterInfo2, sourceType))
					{
						if (attrExists)
						{
							throw new SourceException(SourceException.ErrorIds.InvalidMethodParameters, new string[]
							{
								m.Name
							});
						}
						break;
					}
					else
					{
						Type parameterType = parameterInfo2.ParameterType;
						object[] customAttributes = parameterInfo2.GetCustomAttributes(typeof(ParameterAttribute), true);
						ParameterAttribute parameterAttribute = null;
						if (customAttributes.Length > 0)
						{
							parameterAttribute = (ParameterAttribute)customAttributes[0];
						}
						if (parameterAttribute == null)
						{
							parameterAttribute = SourceLoader.GetInterfaceParameterAttribute(m, parameterInfo2.Name, parameterType);
						}
						XmlElement xmlElement;
						if (parameterType.IsGenericParameter)
						{
							if (!attrExists)
							{
								break;
							}
							xmlElement = doc.CreateElement("source", "http://codeeffects.com/schemas/source/42");
						}
						else if ((parameterType.IsInterface && SourceValidator.InterfacesSameOrSub(parameterType, sourceType)) || TypeUtils.AreEquivalent(parameterType, typeof(object)) || TypeUtils.IsSameOrSubclass(parameterType, sourceType))
						{
							xmlElement = doc.CreateElement("source", "http://codeeffects.com/schemas/source/42");
						}
						else if (parameterType.GetInterfaces().Contains(typeof(IEnumerable)) && parameterType != typeof(string))
						{
							xmlElement = doc.CreateElement("collection", "http://codeeffects.com/schemas/source/42");
							xmlElement.SetAttribute("generic", parameterType.IsGenericType ? "true" : "false");
							xmlElement.SetAttribute("array", parameterType.IsArray ? "true" : "false");
							if (parameterAttribute != null && !string.IsNullOrEmpty(parameterAttribute.Description))
							{
								xmlElement.SetAttribute("description", Encoder.Sanitize(parameterAttribute.Description));
							}
							XmlElement xmlElement2 = SourceLoader.BuildItemElement(doc, parameterType, parameterAttribute, null, true, processedTypes);
							if (xmlElement2 == null)
							{
								if (attrExists)
								{
									throw new SourceException(SourceException.ErrorIds.InvalidMethodParameters, new string[]
									{
										m.Name
									});
								}
								break;
							}
							else
							{
								xmlElement.SetAttribute("class", (xmlElement2.Name == "generic") ? parameterType.Name : parameterType.FullName);
								if ((!parameterType.IsArray && parameterType.IsGenericType) || xmlElement2.Name == "generic")
								{
									xmlElement.SetAttribute("comparisonName", SourceLoader.GetComparisonName(parameterType));
								}
								else
								{
									xmlElement.SetAttribute("comparisonName", parameterType.FullName);
								}
								xmlElement.AppendChild(xmlElement2);
							}
						}
						else
						{
							bool flag = false;
							bool flag2 = false;
							SourceLoader.ClientType clientType = SourceLoader.GetClientType(parameterType, out flag, out flag2);
							if (clientType == SourceLoader.ClientType.Enum)
							{
								if (parameterAttribute != null && parameterAttribute.ConstantValue != null)
								{
									throw new SourceException(SourceException.ErrorIds.EnumMethodParamNotSupported, new string[0]);
								}
								xmlElement = doc.CreateElement("input", "http://codeeffects.com/schemas/source/42");
								SourceLoader.SetElement(xmlElement, "enum", parameterAttribute, false);
								xmlElement.SetAttribute("class", parameterType.FullName);
								xmlElement.SetAttribute("assembly", Assembly.GetAssembly(parameterType).FullName);
								if (parameterAttribute != null)
								{
									xmlElement.SetAttribute("valueInputType", Converter.ValueInputTypeToString(parameterAttribute.ValueInputType));
									if (!string.IsNullOrWhiteSpace(parameterAttribute.Description))
									{
										xmlElement.SetAttribute("description", Encoder.Sanitize(parameterAttribute.Description));
									}
								}
								else
								{
									xmlElement.SetAttribute("valueInputType", Converter.ValueInputTypeToString(ValueInputType.All));
								}
							}
							else
							{
								bool flag3 = false;
								if (parameterAttribute != null && parameterAttribute.ConstantValue != null)
								{
									xmlElement = doc.CreateElement("constant", "http://codeeffects.com/schemas/source/42");
									xmlElement.SetAttribute("value", Encoder.Sanitize(parameterAttribute.ConstantValue));
									flag3 = true;
								}
								else
								{
									xmlElement = doc.CreateElement("input", "http://codeeffects.com/schemas/source/42");
									if (parameterAttribute != null)
									{
										xmlElement.SetAttribute("valueInputType", Converter.ValueInputTypeToString(parameterAttribute.ValueInputType));
										if (!string.IsNullOrWhiteSpace(parameterAttribute.Description))
										{
											xmlElement.SetAttribute("description", Encoder.Sanitize(parameterAttribute.Description));
										}
									}
									else
									{
										xmlElement.SetAttribute("valueInputType", Converter.ValueInputTypeToString(ValueInputType.All));
									}
								}
								xmlElement.SetAttribute("class", parameterType.FullName);
								switch (clientType)
								{
								case SourceLoader.ClientType.Numeric:
									SourceLoader.SetElement(xmlElement, "numeric", parameterAttribute, flag);
									if (!flag3)
									{
										if (parameterAttribute != null && parameterAttribute.Min != -9223372036854775808L)
										{
											xmlElement.SetAttribute("min", parameterAttribute.Min.ToString());
										}
										else
										{
											xmlElement.SetAttribute("min", flag ? SourceLoader.GetNullableMinByNumeric(parameterType) : SourceLoader.GetMinByNumeric(parameterType.Name.ToLower()));
										}
										if (parameterAttribute != null && parameterAttribute.Max != 9223372036854775807L)
										{
											xmlElement.SetAttribute("max", parameterAttribute.Max.ToString());
										}
										else
										{
											xmlElement.SetAttribute("max", flag ? SourceLoader.GetNullableMaxByNumeric(parameterType) : SourceLoader.GetMaxByNumeric(parameterType.Name.ToLower()));
										}
										xmlElement.SetAttribute("allowDecimal", flag ? SourceLoader.GetNullableDecimalByNumeric(parameterType) : SourceLoader.GetDecimalByNumeric(parameterType.Name.ToLower()));
										if (parameterAttribute != null && !string.IsNullOrWhiteSpace(parameterAttribute.DataSourceName) && flag2)
										{
											xmlElement.SetAttribute("dataSourceName", parameterAttribute.DataSourceName);
										}
									}
									break;
								case SourceLoader.ClientType.String:
									SourceLoader.SetElement(xmlElement, "string", parameterAttribute, true);
									if (!flag3)
									{
										if (parameterAttribute != null)
										{
											if (parameterAttribute.ValueInputType != ValueInputType.Fields)
											{
												xmlElement.SetAttribute("maxLength", (parameterAttribute.Max == 9223372036854775807L || parameterAttribute.Max > 256L) ? 256L.ToString() : parameterAttribute.Max.ToString());
											}
										}
										else
										{
											xmlElement.SetAttribute("maxLength", 256L.ToString());
										}
									}
									break;
								case SourceLoader.ClientType.Date:
									SourceLoader.SetElement(xmlElement, "date", parameterAttribute, flag);
									if (!flag3)
									{
										if (parameterAttribute != null && !string.IsNullOrEmpty(parameterAttribute.DateTimeFormat))
										{
											xmlElement.SetAttribute("format", Encoder.Sanitize(parameterAttribute.DateTimeFormat));
										}
										else
										{
											xmlElement.SetAttribute("format", "MMM dd, yyyy");
										}
									}
									break;
								case SourceLoader.ClientType.Time:
									SourceLoader.SetElement(xmlElement, "time", parameterAttribute, flag);
									if (!flag3)
									{
										if (parameterAttribute != null && !string.IsNullOrEmpty(parameterAttribute.DateTimeFormat))
										{
											xmlElement.SetAttribute("format", Encoder.Sanitize(parameterAttribute.DateTimeFormat));
										}
										else
										{
											xmlElement.SetAttribute("format", "hh:mm tt");
										}
									}
									break;
								case SourceLoader.ClientType.Bool:
									SourceLoader.SetElement(xmlElement, "bool", parameterAttribute, flag);
									break;
								}
							}
						}
						if (xmlElement != null)
						{
							list.Add(xmlElement);
							j++;
						}
						else
						{
							if (attrExists)
							{
								throw new SourceException(SourceException.ErrorIds.InvalidMethodParameters, new string[]
								{
									m.Name
								});
							}
							break;
						}
					}
				}
				if (list.Count != pars.Length)
				{
					return false;
				}
				foreach (XmlElement current in list)
				{
					elParams.AppendChild(current);
				}
				return true;
			}
		}

		private static void ManageReturn(MethodInfo m, XmlElement elReturn)
		{
			object[] customAttributes = m.ReturnParameter.GetCustomAttributes(typeof(ReturnAttribute), true);
			ReturnAttribute returnAttribute;
			if (customAttributes.Length > 0)
			{
				returnAttribute = (ReturnAttribute)customAttributes[0];
			}
			else
			{
				returnAttribute = SourceLoader.GetInterfaceReturnAttribute(m);
			}
			if (returnAttribute != null)
			{
				elReturn.SetAttribute("valueInputType", Converter.ValueInputTypeToString(returnAttribute.ValueInputType));
			}
			else
			{
				elReturn.SetAttribute("valueInputType", Converter.ValueInputTypeToString(ValueInputType.All));
			}
			Type returnType = m.ReturnType;
			elReturn.SetAttribute("class", returnType.FullName);
			bool flag = false;
			bool flag2 = false;
			switch (SourceLoader.GetClientType(returnType, out flag, out flag2))
			{
			case SourceLoader.ClientType.Enum:
				SourceLoader.SetElement(elReturn, "enum", null, false);
				elReturn.SetAttribute("assembly", Assembly.GetAssembly(returnType).FullName);
				return;
			case SourceLoader.ClientType.Numeric:
				SourceLoader.SetElement(elReturn, "numeric", null, flag);
				if (returnAttribute != null && returnAttribute.Min != -9223372036854775808L)
				{
					elReturn.SetAttribute("min", returnAttribute.Min.ToString());
				}
				else
				{
					elReturn.SetAttribute("min", flag ? SourceLoader.GetNullableMinByNumeric(returnType) : SourceLoader.GetMinByNumeric(returnType.Name.ToLower()));
				}
				if (returnAttribute != null && returnAttribute.Max != 9223372036854775807L)
				{
					elReturn.SetAttribute("max", returnAttribute.Max.ToString());
				}
				else
				{
					elReturn.SetAttribute("max", flag ? SourceLoader.GetNullableMaxByNumeric(returnType) : SourceLoader.GetMaxByNumeric(returnType.Name.ToLower()));
				}
				elReturn.SetAttribute("allowDecimal", flag ? SourceLoader.GetNullableDecimalByNumeric(returnType) : SourceLoader.GetDecimalByNumeric(returnType.Name.ToLower()));
				if (returnAttribute != null)
				{
					elReturn.SetAttribute("allowCalculation", returnAttribute.AllowCalculations ? "true" : "false");
				}
				else
				{
					elReturn.SetAttribute("allowCalculation", "true");
				}
				if (returnAttribute != null && !string.IsNullOrWhiteSpace(returnAttribute.DataSourceName) && flag2)
				{
					elReturn.SetAttribute("dataSourceName", returnAttribute.DataSourceName);
					return;
				}
				break;
			case SourceLoader.ClientType.String:
				SourceLoader.SetElement(elReturn, "string", null, true);
				elReturn.SetAttribute("type", "string");
				if (returnAttribute != null && returnAttribute.Max != 9223372036854775807L && returnAttribute.Max <= 256L)
				{
					elReturn.SetAttribute("maxLength", returnAttribute.Max.ToString());
				}
				else
				{
					elReturn.SetAttribute("maxLength", 256L.ToString());
				}
				if (returnAttribute != null && returnAttribute.StringComparison != StringComparison.OrdinalIgnoreCase)
				{
					elReturn.SetAttribute("stringComparison", returnAttribute.StringComparison.ToString());
					return;
				}
				break;
			case SourceLoader.ClientType.Date:
				SourceLoader.SetElement(elReturn, "date", null, flag);
				if (returnAttribute != null && !string.IsNullOrEmpty(returnAttribute.DateTimeFormat))
				{
					elReturn.SetAttribute("format", Encoder.Sanitize(returnAttribute.DateTimeFormat));
					return;
				}
				elReturn.SetAttribute("format", "MMM dd, yyyy");
				return;
			case SourceLoader.ClientType.Time:
				SourceLoader.SetElement(elReturn, "time", null, flag);
				if (returnAttribute != null && !string.IsNullOrEmpty(returnAttribute.DateTimeFormat))
				{
					elReturn.SetAttribute("format", Encoder.Sanitize(returnAttribute.DateTimeFormat));
					return;
				}
				elReturn.SetAttribute("format", "hh:mm tt");
				return;
			case SourceLoader.ClientType.Bool:
				SourceLoader.SetElement(elReturn, "bool", null, flag);
				return;
			default:
				throw new SourceException(flag ? SourceException.ErrorIds.NullableReturnNotSupported : SourceException.ErrorIds.ReturnTypeNotSupported, new string[]
				{
					m.Name
				});
			}
		}

		private static string GetTokenByMethod(XmlNode source, Type type, Type sourceObject, string methodName, XmlNodeList parameters, bool isMethod)
		{
			List<MethodInfo> methods = SourceLoader.GetMethods(type);
			foreach (MethodInfo current in methods)
			{
				if (!(current.Name != methodName) && current.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), true).Length == 0 && SourceValidator.IsMethodValid(current, isMethod))
				{
					ParameterInfo[] parameters2 = current.GetParameters();
					if (parameters2.Length == parameters.Count)
					{
						if (parameters2.Length == 0)
						{
							string hashToken = Encoder.GetHashToken(current);
							return hashToken;
						}
						if (SourceLoader.ParamsMatch(source, sourceObject, parameters2, parameters))
						{
							string hashToken = Encoder.GetHashToken(current);
							return hashToken;
						}
					}
				}
			}
			throw new SourceException(SourceException.ErrorIds.MethodNotFound, new string[]
			{
				methodName
			});
		}

		private static XmlNode GetMethodByToken(XmlNode sourceXml, string token, bool isMethod, MalformedXmlException.ErrorIds errorTag)
		{
			XmlNode xmlNode = sourceXml.SelectSingleNode(string.Format(isMethod ? "{0}:fields" : "{0}:actions", "s"), Xml.GetSourceNamespaceManager(sourceXml));
			if (xmlNode == null || xmlNode.ChildNodes.Count == 0)
			{
				throw new MalformedXmlException(errorTag, new string[0]);
			}
			Type sourceObject = SourceLoader.GetSourceObject(sourceXml);
			foreach (XmlNode xmlNode2 in xmlNode.ChildNodes)
			{
				if (xmlNode2.NodeType != XmlNodeType.Comment && (!isMethod || !(xmlNode2.Name != "function")))
				{
					string value = xmlNode2.Attributes["methodName"].Value;
					Type type;
					if (xmlNode2.Attributes["class"] != null)
					{
						type = SourceLoader.LoadType(xmlNode2.Attributes["assembly"].Value, xmlNode2.Attributes["class"].Value, SourceException.ErrorIds.AssemblyNameIsEmpty, SourceException.ErrorIds.AssemblyDoesNotContainType);
					}
					else
					{
						type = sourceObject;
					}
					MethodInfo matchingMethod = SourceLoader.GetMatchingMethod(value, type, token, isMethod);
					if (!(matchingMethod == null))
					{
						ParameterInfo[] parameters = matchingMethod.GetParameters();
						XmlNode paramNode = SourceLoader.GetParamNode(xmlNode2);
						if (parameters.Length == paramNode.ChildNodes.Count && (parameters.Length == 0 || SourceLoader.ParamsMatch(sourceXml, sourceObject, parameters, paramNode.ChildNodes)))
						{
							return xmlNode2;
						}
					}
				}
			}
			throw new SourceException(SourceException.ErrorIds.NoMethodForTheToken, new string[]
			{
				token
			});
		}

		private static MethodInfo GetMatchingMethod(string methodName, Type type, string token, bool isMethod)
		{
			List<MethodInfo> methods = SourceLoader.GetMethods(type);
			foreach (MethodInfo current in methods)
			{
				if (!(current.Name != methodName) && current.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), true).Length == 0 && SourceValidator.IsMethodValid(current, isMethod) && Encoder.GetHashToken(current) == token)
				{
					return current;
				}
			}
			return null;
		}

		private static bool ParamsMatch(XmlNode source, Type sourceObject, ParameterInfo[] pis, XmlNodeList paramNodes)
        {
            return false;
        //TODO
        //{
        //    bool flag = true;
        //    int i = 0;
        //    while (i < pis.Length)
        //    {
        //        XmlNode xmlNode = paramNodes[i];
        //        string name;
        //        if ((name = xmlNode.Name) != null)
        //        {
        //            if (<PrivateImplementationDetails>{C6EA5F6E-C064-4F46-8F27-151D6168C23D}.$$method0x6000390-1 == null)
        //            {
        //                <PrivateImplementationDetails>{C6EA5F6E-C064-4F46-8F27-151D6168C23D}.$$method0x6000390-1 = new Dictionary<string, int>(7)
        //                {
        //                    {
        //                        "source",
        //                        0
        //                    },
        //                    {
        //                        "self",
        //                        1
        //                    },
        //                    {
        //                        "collection",
        //                        2
        //                    },
        //                    {
        //                        "input",
        //                        3
        //                    },
        //                    {
        //                        "constant",
        //                        4
        //                    },
        //                    {
        //                        "property",
        //                        5
        //                    },
        //                    {
        //                        "value",
        //                        6
        //                    }
        //                };
        //            }
        //            int num;
        //            if (<PrivateImplementationDetails>{C6EA5F6E-C064-4F46-8F27-151D6168C23D}.$$method0x6000390-1.TryGetValue(name, out num))
        //            {
        //                switch (num)
        //                {
        //                case 0:
        //                case 1:
        //                {
        //                    Type parameterType = pis[i].ParameterType;
        //                    bool flag2 = false;
        //                    if (parameterType.IsGenericParameter)
        //                    {
        //                        Type[] interfaces = parameterType.GetInterfaces();
        //                        flag2 = !interfaces.Contains(typeof(IEnumerable));
        //                    }
        //                    flag = (flag2 || (parameterType.IsInterface && SourceValidator.InterfacesSameOrSub(parameterType, sourceObject)) || TypeUtils.AreEquivalent(parameterType, typeof(object)) || TypeUtils.IsSameOrSubclass(parameterType, sourceObject));
        //                    break;
        //                }
        //                case 2:
        //                    if (xmlNode.ChildNodes[0].Name == "generic")
        //                    {
        //                        flag = (string.IsNullOrEmpty(pis[i].ParameterType.FullName) && pis[i].ParameterType.Name == xmlNode.Attributes["class"].Value);
        //                    }
        //                    else
        //                    {
        //                        Type underlyingType = SourceLoader.GetUnderlyingType(pis[i].ParameterType, null);
        //                        if (xmlNode.Attributes["array"].Value == "true")
        //                        {
        //                            flag = (pis[i].ParameterType.IsArray && xmlNode.ChildNodes[0].Attributes["class"].Value == pis[i].ParameterType.GetElementType().FullName);
        //                        }
        //                        else
        //                        {
        //                            Type type = Type.GetType(xmlNode.Attributes["class"].Value);
        //                            Type underlyingType2 = SourceLoader.GetUnderlyingType(type, Type.GetType(xmlNode.ChildNodes[0].Attributes["class"].Value));
        //                            flag = (TypeUtils.IsSameOrSubclass(type, pis[i].ParameterType) && TypeUtils.IsSameOrSubclass(underlyingType2, underlyingType));
        //                        }
        //                    }
        //                    break;
        //                case 3:
        //                case 4:
        //                {
        //                    Type type;
        //                    if (xmlNode.Attributes["assembly"] != null)
        //                    {
        //                        type = Type.GetType(xmlNode.Attributes["class"].Value + ", " + xmlNode.Attributes["assembly"].Value);
        //                        if (type == null)
        //                        {
        //                            type = Type.GetType(xmlNode.Attributes["class"].Value);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        type = Type.GetType(xmlNode.Attributes["class"].Value);
        //                    }
        //                    flag = (TypeUtils.AreEquivalent(type, pis[i].ParameterType) || TypeUtils.IsImplicitlyConvertible(type, pis[i].ParameterType));
        //                    break;
        //                }
        //                case 5:
        //                {
        //                    XmlNode fieldByPropertyName = SourceLoader.GetFieldByPropertyName(source, xmlNode.Attributes["name"].Value);
        //                    Type type;
        //                    if (fieldByPropertyName.Attributes["assembly"] != null)
        //                    {
        //                        type = Type.GetType(fieldByPropertyName.Attributes["class"].Value + ", " + fieldByPropertyName.Attributes["assembly"].Value);
        //                        if (type == null)
        //                        {
        //                            type = Type.GetType(fieldByPropertyName.Attributes["class"].Value);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        type = Type.GetType(fieldByPropertyName.Attributes["class"].Value);
        //                    }
        //                    Type[] interfaces2 = type.GetInterfaces();
        //                    if (interfaces2.Contains(typeof(IDictionary)))
        //                    {
        //                        flag = false;
        //                    }
        //                    else if (interfaces2.Contains(typeof(IEnumerable)) && type != typeof(string))
        //                    {
        //                        if (type.IsGenericType)
        //                        {
        //                            flag = true;
        //                        }
        //                        else
        //                        {
        //                            type = SourceLoader.GetBaseGenericType(type);
        //                            flag = ((type.IsInterface && type.IsGenericType && pis[i].ParameterType.IsInterface && pis[i].ParameterType.IsGenericType) || TypeUtils.IsSameOrSubclass(type.GetGenericTypeDefinition(), pis[i].ParameterType.GetGenericTypeDefinition()));
        //                        }
        //                    }
        //                    else
        //                    {
        //                        flag = (TypeUtils.AreEquivalent(type, pis[i].ParameterType) || TypeUtils.IsImplicitlyConvertible(type, pis[i].ParameterType));
        //                    }
        //                    break;
        //                }
        //                case 6:
        //                {
        //                    string a = (xmlNode.Attributes["type"] == null) ? typeof(string).FullName : Type.GetType(xmlNode.Attributes["type"].Value).FullName;
        //                    flag = (a == pis[i].ParameterType.FullName);
        //                    break;
        //                }
        //                default:
        //                    goto IL_513;
        //                }
        //                if (flag)
        //                {
        //                    i++;
        //                    continue;
        //                }
        //                break;
        //            }
        //        }
        //        IL_513:
        //        throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownNodeName, new string[]
        //        {
        //            paramNodes[i].Name
        //        });
        //    }
        //    return flag;
        }

		private static List<MethodInfo> GetMethods(Type type)
		{
			return SourceLoader.GetMethods(type, null);
		}

		private static List<MethodInfo> GetMethods(Type type, SourceState state)
		{
			BindingFlags bindingAttr = (state != null && state.DeclaredMembersOnly) ? (BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public) : (BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			MethodInfo[] methods = type.GetMethods(bindingAttr);
			List<MethodInfo> list = new List<MethodInfo>();
			MethodInfo[] array = methods;
			for (int i = 0; i < array.Length; i++)
			{
				MethodInfo methodInfo = array[i];
				if (!(methodInfo.DeclaringType == typeof(object)))
				{
					list.Add(methodInfo);
				}
			}
			return list;
		}

		private static Type GetSourceObject(XmlNode source)
		{
			return Type.GetType(source.Attributes["type"].Value);
		}

		private static object GetAttribute<T>(object[] attributes)
		{
			for (int i = 0; i < attributes.Length; i++)
			{
				if (attributes[i] is T)
				{
					return attributes[i];
				}
			}
			return null;
		}

		private static XmlElement GetPropertyElement(XmlDocument doc, string node, Type t, FieldAttribute attr, string name, bool nullable, bool settable, bool gettable)
		{
			XmlElement xmlElement = doc.CreateElement(node, "http://codeeffects.com/schemas/source/42");
			xmlElement.SetAttribute("propertyName", name);
			xmlElement.SetAttribute("class", t.FullName);
			xmlElement.SetAttribute("nullable", nullable ? "true" : "false");
			if (!settable)
			{
				xmlElement.SetAttribute("settable", "false");
			}
			if (!gettable)
			{
				xmlElement.SetAttribute("gettable", "false");
			}
			if (attr != null)
			{
				xmlElement.SetAttribute("valueInputType", Converter.ValueInputTypeToString(attr.ValueInputType));
			}
			else
			{
				xmlElement.SetAttribute("valueInputType", Converter.ValueInputTypeToString(ValueInputType.All));
			}
			return xmlElement;
		}

		private static bool IsReadOnly(MemberInfo mi)
		{
			ReadOnlyAttribute readOnlyAttribute = Attribute.GetCustomAttribute(mi, typeof(ReadOnlyAttribute)) as ReadOnlyAttribute;
			return readOnlyAttribute != null && readOnlyAttribute.IsReadOnly;
		}

		private static void SetElement(XmlElement el, string type, ParameterAttribute attr, bool nullable)
		{
			el.SetAttribute("type", type);
			if (attr != null && !string.IsNullOrEmpty(attr.Description))
			{
				el.SetAttribute("description", Encoder.Sanitize(attr.Description));
			}
			el.SetAttribute("nullable", nullable ? "true" : "false");
		}

		private static SourceLoader.ClientType GetClientType(Type t, out bool nullable, out bool sourceable)
		{
			nullable = (sourceable = false);
			if (!t.IsValueType && t != typeof(string))
			{
				return SourceLoader.ClientType.Other;
			}
			if (t.IsEnum)
			{
				return SourceLoader.ClientType.Enum;
			}
			if (t.IsGenericType)
			{
				nullable = true;
				if (t == typeof(byte?) || t == typeof(sbyte?) || t == typeof(short?) || t == typeof(ushort?) || t == typeof(int?) || t == typeof(uint?) || t == typeof(long?) || t == typeof(ulong?))
				{
					sourceable = true;
					return SourceLoader.ClientType.Numeric;
				}
				if (t == typeof(float?) || t == typeof(double?) || t == typeof(decimal?))
				{
					return SourceLoader.ClientType.Numeric;
				}
				if (t == typeof(DateTime?))
				{
					return SourceLoader.ClientType.Date;
				}
				if (t == typeof(TimeSpan?))
				{
					return SourceLoader.ClientType.Time;
				}
				if (t == typeof(bool?))
				{
					return SourceLoader.ClientType.Bool;
				}
			}
			else
			{
				string text = t.Name.ToLower();
				string key;
				switch (key = text)
				{
				case "byte":
				case "sbyte":
				case "int16":
				case "uint16":
				case "int32":
				case "uint32":
				case "int64":
				case "uint64":
					sourceable = true;
					return SourceLoader.ClientType.Numeric;
				case "single":
				case "double":
				case "decimal":
					return SourceLoader.ClientType.Numeric;
				case "datetime":
					return SourceLoader.ClientType.Date;
				case "timespan":
					return SourceLoader.ClientType.Time;
				case "boolean":
					return SourceLoader.ClientType.Bool;
				case "string":
					nullable = true;
					return SourceLoader.ClientType.String;
				}
			}
			return SourceLoader.ClientType.Other;
		}

		private static XmlElement BuildItemElement(XmlDocument sourceXml, Type collectionType, ISettingsAttribute attr, string displayName, bool isParameter, List<Type> processedTypes)
		{
			FieldAttribute fieldAttribute = isParameter ? null : ((FieldAttribute)attr);
			Type underlyingType = SourceLoader.GetUnderlyingType(collectionType, (attr == null || attr.CollectionItemType == null) ? null : attr.CollectionItemType);
			if (underlyingType == null)
			{
				return null;
			}
			XmlElement xmlElement;
			if (underlyingType.IsValueType || underlyingType == typeof(string))
			{
				if (underlyingType.IsEnum && underlyingType.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), true).Length != 0)
				{
					return null;
				}
				xmlElement = sourceXml.CreateElement("value", "http://codeeffects.com/schemas/source/42");
				xmlElement.SetAttribute("class", underlyingType.FullName);
				bool flag = false;
				bool flag2 = false;
				switch (SourceLoader.GetClientType(underlyingType, out flag, out flag2))
				{
				case SourceLoader.ClientType.Enum:
					xmlElement.SetAttribute("type", "enum");
					xmlElement.SetAttribute("assembly", Assembly.GetAssembly(underlyingType).FullName);
					xmlElement.SetAttribute("nullable", "false");
					break;
				case SourceLoader.ClientType.Numeric:
					xmlElement.SetAttribute("type", "numeric");
					if (!isParameter)
					{
						if (attr != null)
						{
							xmlElement.SetAttribute("allowCalculation", fieldAttribute.AllowCalculations ? "true" : "false");
						}
						else
						{
							xmlElement.SetAttribute("allowCalculation", "true");
						}
						if (attr != null)
						{
							xmlElement.SetAttribute("includeInCalculations", fieldAttribute.IncludeInCalculations ? "true" : "false");
						}
						else
						{
							xmlElement.SetAttribute("includeInCalculations", "true");
						}
						if (attr != null && !string.IsNullOrWhiteSpace(attr.DataSourceName) && flag2)
						{
							xmlElement.SetAttribute("dataSourceName", attr.DataSourceName);
						}
					}
					xmlElement.SetAttribute("allowDecimal", flag ? SourceLoader.GetNullableDecimalByNumeric(underlyingType) : SourceLoader.GetDecimalByNumeric(underlyingType.Name.ToLower()));
					if (attr != null && attr.Min != -9223372036854775808L)
					{
						xmlElement.SetAttribute("min", attr.Min.ToString());
					}
					else
					{
						xmlElement.SetAttribute("min", flag ? SourceLoader.GetNullableMinByNumeric(underlyingType) : SourceLoader.GetMinByNumeric(underlyingType.Name.ToLower()));
					}
					if (attr != null && attr.Max != 9223372036854775807L)
					{
						xmlElement.SetAttribute("max", attr.Max.ToString());
					}
					else
					{
						xmlElement.SetAttribute("max", flag ? SourceLoader.GetNullableMaxByNumeric(underlyingType) : SourceLoader.GetMaxByNumeric(underlyingType.Name.ToLower()));
					}
					xmlElement.SetAttribute("nullable", flag ? "true" : "false");
					break;
				case SourceLoader.ClientType.String:
					xmlElement.SetAttribute("type", "string");
					if (!isParameter)
					{
						if (attr != null && attr.Max != 9223372036854775807L && attr.Max <= 256L)
						{
							xmlElement.SetAttribute("maxLength", attr.Max.ToString());
						}
						else
						{
							xmlElement.SetAttribute("maxLength", 256L.ToString());
						}
						if (attr != null && fieldAttribute.StringComparison != StringComparison.OrdinalIgnoreCase)
						{
							xmlElement.SetAttribute("stringComparison", fieldAttribute.StringComparison.ToString());
						}
					}
					xmlElement.SetAttribute("nullable", "true");
					break;
				case SourceLoader.ClientType.Date:
					xmlElement.SetAttribute("type", "date");
					if (attr != null && !string.IsNullOrEmpty(attr.DateTimeFormat))
					{
						xmlElement.SetAttribute("format", Encoder.Sanitize(attr.DateTimeFormat));
					}
					else
					{
						xmlElement.SetAttribute("format", "MMM dd, yyyy");
					}
					xmlElement.SetAttribute("nullable", flag ? "true" : "false");
					break;
				case SourceLoader.ClientType.Time:
					xmlElement.SetAttribute("type", "time");
					if (attr != null && !string.IsNullOrEmpty(attr.DateTimeFormat))
					{
						xmlElement.SetAttribute("format", Encoder.Sanitize(attr.DateTimeFormat));
					}
					else
					{
						xmlElement.SetAttribute("format", "hh:mm tt");
					}
					xmlElement.SetAttribute("nullable", flag ? "true" : "false");
					break;
				case SourceLoader.ClientType.Bool:
					xmlElement.SetAttribute("type", "bool");
					xmlElement.SetAttribute("nullable", flag ? "true" : "false");
					break;
				default:
					return null;
				}
			}
			else if (string.IsNullOrEmpty(collectionType.FullName) || underlyingType.GUID == Guid.Empty || string.IsNullOrEmpty(underlyingType.FullName))
			{
				if (!isParameter)
				{
					return null;
				}
				xmlElement = sourceXml.CreateElement("generic", "http://codeeffects.com/schemas/source/42");
			}
			else
			{
				if (underlyingType.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), true).Length != 0)
				{
					return null;
				}
				xmlElement = sourceXml.CreateElement("reference", "http://codeeffects.com/schemas/source/42");
				xmlElement.SetAttribute("class", underlyingType.FullName);
				if (!isParameter)
				{
					xmlElement.SetAttribute("displayName", (fieldAttribute == null || string.IsNullOrWhiteSpace(fieldAttribute.CollectionItemName)) ? displayName : fieldAttribute.CollectionItemName);
				}
				SourceLoader.Extract(sourceXml, underlyingType, processedTypes);
			}
			return xmlElement;
		}

		private static bool IsPropertyExcludedInInterface(Type[] interfaces, string name)
		{
			if (interfaces == null || interfaces.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < interfaces.Length; i++)
			{
				Type type = interfaces[i];
				MemberInfo memberInfo = SourceLoader.GetMemberInfo(type, name);
				if (memberInfo != null)
				{
					object[] customAttributes = memberInfo.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), true);
					if (customAttributes.Length > 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool IsMethodExcludedInInterface(Type[] interfaces, string name, Type[] pars)
		{
			if (interfaces == null || interfaces.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < interfaces.Length; i++)
			{
				Type type = interfaces[i];
				MethodInfo method = type.GetMethod(name, pars);
				if (method != null)
				{
					object[] customAttributes = method.GetCustomAttributes(typeof(ExcludeFromEvaluationAttribute), true);
					if (customAttributes.Length > 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static IDescribableAttribute GetInterfaceDescribableAttribute(Type[] interfaces, string name, Type[] pars)
		{
			if (interfaces == null || interfaces.Length == 0)
			{
				return null;
			}
			for (int i = 0; i < interfaces.Length; i++)
			{
				Type type = interfaces[i];
				MethodInfo method = type.GetMethod(name, pars);
				if (method != null)
				{
					object[] customAttributes = method.GetCustomAttributes(false);
					if (customAttributes != null && customAttributes.Length > 0)
					{
						object[] array = customAttributes;
						for (int j = 0; j < array.Length; j++)
						{
							object obj = array[j];
							if (obj is MethodAttribute || obj is ActionAttribute)
							{
								return (IDescribableAttribute)obj;
							}
						}
					}
				}
			}
			return null;
		}

		private static ParameterAttribute GetInterfaceParameterAttribute(MethodInfo m, string name, Type t)
		{
			Type declaringType = m.DeclaringType;
			Type[] interfaces = declaringType.GetInterfaces();
			Type[] methodParameterTypes = SourceLoader.GetMethodParameterTypes(m);
			if (methodParameterTypes.Length == 0 || interfaces == null || interfaces.Length == 0)
			{
				return null;
			}
			Type[] array = interfaces;
			for (int i = 0; i < array.Length; i++)
			{
				Type type = array[i];
				MethodInfo method = type.GetMethod(m.Name, methodParameterTypes);
				if (method != null)
				{
					ParameterInfo[] parameters = method.GetParameters();
					if (parameters.Length > 0)
					{
						ParameterInfo[] array2 = parameters;
						for (int j = 0; j < array2.Length; j++)
						{
							ParameterInfo parameterInfo = array2[j];
							if (parameterInfo.Name == name && parameterInfo.ParameterType == t)
							{
								object[] customAttributes = parameterInfo.GetCustomAttributes(typeof(ParameterAttribute), true);
								if (customAttributes.Length > 0)
								{
									return (ParameterAttribute)customAttributes[0];
								}
							}
						}
					}
				}
			}
			return null;
		}

		private static ReturnAttribute GetInterfaceReturnAttribute(MethodInfo m)
		{
			Type declaringType = m.DeclaringType;
			Type[] interfaces = declaringType.GetInterfaces();
			Type[] methodParameterTypes = SourceLoader.GetMethodParameterTypes(m);
			if (interfaces == null || interfaces.Length == 0)
			{
				return null;
			}
			Type[] array = interfaces;
			for (int i = 0; i < array.Length; i++)
			{
				Type type = array[i];
				MethodInfo method = type.GetMethod(m.Name, methodParameterTypes);
				if (method != null)
				{
					object[] customAttributes = m.ReturnParameter.GetCustomAttributes(typeof(ReturnAttribute), true);
					if (customAttributes.Length > 0)
					{
						return (ReturnAttribute)customAttributes[0];
					}
				}
			}
			return null;
		}

		private static FieldAttribute GetInterfaceFieldAttribute(Type[] interfaces, string name)
		{
			if (interfaces == null || interfaces.Length == 0)
			{
				return null;
			}
			for (int i = 0; i < interfaces.Length; i++)
			{
				Type type = interfaces[i];
				MemberInfo memberInfo = SourceLoader.GetMemberInfo(type, name);
				if (memberInfo != null)
				{
					object[] customAttributes = memberInfo.GetCustomAttributes(typeof(FieldAttribute), true);
					if (customAttributes.Length > 0)
					{
						return (FieldAttribute)customAttributes[0];
					}
				}
			}
			return null;
		}

		private static object[] GetInterfaceParentAttributes(Type[] interfaces, string name)
		{
			if (interfaces == null || interfaces.Length == 0)
			{
				return null;
			}
			for (int i = 0; i < interfaces.Length; i++)
			{
				Type type = interfaces[i];
				MemberInfo memberInfo = SourceLoader.GetMemberInfo(type, name);
				if (memberInfo != null)
				{
					object[] customAttributes = memberInfo.GetCustomAttributes(typeof(ParentAttribute), true);
					if (customAttributes.Length > 0)
					{
						return customAttributes;
					}
				}
			}
			return null;
		}

		private static MemberInfo GetMemberInfo(Type type, string name)
		{
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
			if (property != null)
			{
				return property;
			}
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				return field;
			}
			return null;
		}

		private static Type[] GetMethodParameterTypes(MethodInfo m)
		{
			ParameterInfo[] parameters = m.GetParameters();
			List<Type> list = new List<Type>(parameters.Length);
			ParameterInfo[] array = parameters;
			for (int i = 0; i < array.Length; i++)
			{
				ParameterInfo parameterInfo = array[i];
				list.Add(parameterInfo.ParameterType);
			}
			return list.ToArray();
		}

		private static Type LoadType(string assembly, string type, SourceException.ErrorIds nullError, SourceException.ErrorIds typeError)
		{
			if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(assembly))
			{
				throw new SourceException(nullError, new string[0]);
			}
			Assembly assembly2 = null;
			try
			{
				assembly2 = Assembly.Load(assembly);
			}
			catch (Exception ex)
			{
				throw new SourceException(SourceException.ErrorIds.FailedToFindOrLoadAssembly, new string[]
				{
					assembly,
					ex.Message
				});
			}
			Type[] types = assembly2.GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				Type type2 = types[i];
				if (type2.FullName == type)
				{
					return type2;
				}
			}
			throw new SourceException(typeError, new string[]
			{
				assembly,
				type
			});
		}

		private static string GetMinByNumeric(string type)
		{
			switch (type)
			{
			case "byte":
			case "uint16":
			case "uint32":
			case "uint64":
				return "0";
			case "sbyte":
				return "-128";
			case "int16":
				return "-32768";
			case "int32":
				return "-2147483648";
			}
			return "-9007199254740992";
		}

		private static string GetMaxByNumeric(string type)
		{
			switch (type)
			{
			case "byte":
				return "255";
			case "sbyte":
				return "127";
			case "int16":
				return "32767";
			case "uint16":
				return "65535";
			case "int32":
				return "2147483647";
			case "uint32":
				return "4294967295";
			}
			return "9007199254740992";
		}

		private static string GetDecimalByNumeric(string type)
		{
			switch (type)
			{
			case "byte":
			case "sbyte":
			case "int16":
			case "uint16":
			case "int32":
			case "uint32":
			case "int64":
			case "uint64":
				return "false";
			}
			return "true";
		}

		private static string GetNullableMinByNumeric(Type type)
		{
			if (type == typeof(byte?) || type == typeof(ushort?) || type == typeof(uint?) || type == typeof(ulong?))
			{
				return "0";
			}
			if (type == typeof(sbyte?))
			{
				return "-128";
			}
			if (type == typeof(short?))
			{
				return "-32768";
			}
			if (type == typeof(int?))
			{
				return "-2147483648";
			}
			return "-9007199254740992";
		}

		private static string GetNullableMaxByNumeric(Type type)
		{
			if (type == typeof(byte?))
			{
				return "255";
			}
			if (type == typeof(sbyte?))
			{
				return "127";
			}
			if (type == typeof(short?))
			{
				return "32767";
			}
			if (type == typeof(ushort?))
			{
				return "65535";
			}
			if (type == typeof(int?))
			{
				return "2147483647";
			}
			if (type == typeof(uint?))
			{
				return "4294967295";
			}
			return "9007199254740992";
		}

		private static string GetNullableDecimalByNumeric(Type t)
		{
			if (t == typeof(byte?) || t == typeof(ushort?) || t == typeof(uint?) || t == typeof(ulong?) || t == typeof(sbyte?) || t == typeof(short?) || t == typeof(int?) || t == typeof(long?))
			{
				return "false";
			}
			return "true";
		}

		private static void SetDisplayName(MemberInfo pi, XmlElement el, FieldAttribute attr, string parentName, string name, string parentDescription)
		{
			if (parentName != null)
			{
				el.SetAttribute("displayName", Encoder.Sanitize(parentName));
				if (parentDescription != null)
				{
					el.SetAttribute("description", Encoder.Sanitize(parentDescription));
				}
				else if (attr != null && !string.IsNullOrEmpty(attr.Description))
				{
					el.SetAttribute("description", Encoder.Sanitize(attr.Description));
				}
			}
			else if (attr != null)
			{
				if (!string.IsNullOrEmpty(attr.DisplayName))
				{
					el.SetAttribute("displayName", Encoder.Sanitize(attr.DisplayName));
				}
				if (!string.IsNullOrEmpty(attr.Description))
				{
					el.SetAttribute("description", Encoder.Sanitize(attr.Description));
				}
			}
			if (el.Attributes["displayName"] == null)
			{
				el.SetAttribute("displayName", SourceLoader.BuildName(pi, name));
			}
		}

		private static FieldAttribute GetFieldAttribute(MemberInfo mi, Type[] interfaces)
		{
			object[] customAttributes = mi.GetCustomAttributes(typeof(FieldAttribute), true);
			FieldAttribute result;
			if (customAttributes.Length > 0)
			{
				result = (FieldAttribute)customAttributes[0];
			}
			else
			{
				result = SourceLoader.GetInterfaceFieldAttribute(interfaces, mi.Name);
			}
			return result;
		}

		private static string BuildName(MemberInfo pi, string name)
		{
			if (name != null)
			{
				return name + "." + pi.Name;
			}
			return pi.Name;
		}

		private static string GetComparisonName(Type type)
		{
			return type.Namespace + "." + type.Name;
		}
	}
}
