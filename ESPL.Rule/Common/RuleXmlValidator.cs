using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ESPL.Rule.Common
{
    internal class RuleXmlValidator
    {
        private bool ruleIsValid;

        private XmlSchemaSet schemas;

        private List<string> errors;

        public List<string> Errors
        {
            get
            {
                return this.errors;
            }
        }

        public RuleXmlValidator()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            this.schemas = new XmlSchemaSet();
            RuleXmlValidator.AddSchema(this.schemas, "Rule", executingAssembly);
            RuleXmlValidator.AddSchema(this.schemas, "UI", executingAssembly);
            this.errors = new List<string>();
        }

        private static void AddSchema(XmlSchemaSet schemas, string schemaName, Assembly assembly)
        {
            using (Stream manifestResourceStream = assembly.GetManifestResourceStream(string.Format("CodeEffects.Rule.Resources.Schemas.{0}.xsd", schemaName)))
            {
                using (XmlReader xmlReader = XmlReader.Create(manifestResourceStream, new XmlReaderSettings
                {
                    CloseInput = true,
                    ConformanceLevel = ConformanceLevel.Document,
                    ValidationType = ValidationType.Schema
                }))
                {
                    schemas.Add(null, xmlReader);
                }
            }
        }

        public bool Validate(string xmlString)
        {
            XDocument xDocument = XDocument.Parse(xmlString);
            this.ruleIsValid = true;
            xDocument.Validate(this.schemas, delegate(object o, ValidationEventArgs e)
            {
                this.ruleIsValid = false;
                this.errors.Add(string.Format("{0}", e.Message));
            }, true);
            if (!this.ruleIsValid)
            {
                return this.ruleIsValid;
            }
            XNamespace defaultNamespace = xDocument.Root.GetDefaultNamespace();
            foreach (XElement current in xDocument.Descendants(defaultNamespace + "value"))
            {
                this.ruleIsValid = this.ValidateValue(current);
                if (!this.ruleIsValid)
                {
                    break;
                }
            }
            foreach (XElement current2 in xDocument.Descendants(defaultNamespace + "condition"))
            {
                this.ruleIsValid = RuleXmlValidator.ValidateCondition(current2);
                if (!this.ruleIsValid)
                {
                    break;
                }
            }
            foreach (XElement current3 in xDocument.Descendants(defaultNamespace + "method"))
            {
                this.ruleIsValid = RuleXmlValidator.ValidateMethod(current3);
                if (!this.ruleIsValid)
                {
                    break;
                }
            }
            return this.ruleIsValid;
        }

        private static bool ValidateCondition(XElement condition)
        {
            bool result = false;
            string key;
            switch (key = (string)condition.Attribute("type"))
            {
                case "equal":
                case "notEqual":
                case "less":
                case "lessOrEqual":
                case "greater":
                case "greaterOrEqual":
                    result = (condition.Elements().Count<XElement>() == 2);
                    break;
                case "isNull":
                case "isNotNull":
                    result = (condition.Elements().Count<XElement>() == 1);
                    break;
                case "between":
                    result = (condition.Elements().Count<XElement>() == 3);
                    break;
            }
            return result;
        }

        private bool ValidateValue(XElement value)
        {
            bool flag = false;
            string text = ((string)value.Attribute("type")) ?? "string";
            text = text.ToLower();
            if (text == "string" || value.IsEmpty)
            {
                return true;
            }
            if (string.IsNullOrWhiteSpace(value.Value))
            {
                flag = false;
            }
            else
            {
                string key;
                switch (key = text)
                {
                    case "bool":
                        {
                            bool flag2;
                            flag = bool.TryParse(value.Value, out flag2);
                            goto IL_324;
                        }
                    case "integer":
                        {
                            int num2;
                            flag = int.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out num2);
                            goto IL_324;
                        }
                    case "time":
                        {
                            TimeSpan timeSpan;
                            flag = TimeSpan.TryParse(value.Value, CultureInfo.InvariantCulture, out timeSpan);
                            goto IL_324;
                        }
                    case "date":
                    case "datetime":
                        {
                            DateTime dateTime;
                            flag = DateTime.TryParse(value.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                            goto IL_324;
                        }
                    case "numeric":
                        {
                            double num3;
                            flag = double.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out num3);
                            goto IL_324;
                        }
                    case "double":
                        {
                            double num4;
                            flag = double.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out num4);
                            goto IL_324;
                        }
                }
                Type type = Type.GetType((string)value.Attribute("type"), true, true);
                if (type.IsEnum)
                {
                    try
                    {
                        Enum.Parse(type, value.Value, true);
                        flag = true;
                        goto IL_324;
                    }
                    catch
                    {
                        goto IL_324;
                    }
                }
                if (ExpressionBuilderBase.IsGenericNullable(type))
                {
                    type = Nullable.GetUnderlyingType(type);
                }
                MethodInfo method = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[]
				{
					typeof(string),
					typeof(IFormatProvider)
				}, null);
                if (method != null)
                {
                    try
                    {
                        method.Invoke(null, new object[]
						{
							value.Value,
							CultureInfo.InvariantCulture
						});
                        flag = true;
                    }
                    catch
                    {
                    }
                }
                method = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[]
				{
					typeof(string)
				}, null);
                if (method != null)
                {
                    try
                    {
                        method.Invoke(null, new object[]
						{
							value.Value
						});
                        flag = true;
                    }
                    catch
                    {
                    }
                }
                if (!flag)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(type, value.GetDefaultNamespace().NamespaceName);
                    using (StringReader stringReader = new StringReader(value.FirstNode.ToString(SaveOptions.DisableFormatting)))
                    {
                        try
                        {
                            xmlSerializer.Deserialize(stringReader);
                            flag = true;
                        }
                        catch
                        {
                        }
                    }
                }
                if (!flag)
                {
                    try
                    {
                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        javaScriptSerializer.Deserialize(value.Value, type);
                        flag = true;
                    }
                    catch
                    {
                    }
                }
            }
        IL_324:
            if (!flag)
            {
                this.errors.Add(string.Format("Value of type '{0}', cannot parse '{1}'", value.Attribute("type"), value.Value));
            }
            return flag;
        }

        private static bool ValidateMethod(XElement method)
        {
            return (((string)method.Attribute("instance")) ?? "").ToLower() != "true" || method.Elements().Count<XElement>() > 0;
        }

        private void Schemas_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            this.ruleIsValid = false;
        }

        public void DumpInvalidNodes(XElement el)
        {
            if (el.GetSchemaInfo().Validity != XmlSchemaValidity.Valid)
            {
                this.errors.Add(string.Format("Invalid Element {0}", el.AncestorsAndSelf().InDocumentOrder<XElement>().Aggregate("", (string s, XElement i) => s + "/" + i.Name.ToString())));
            }
            foreach (XAttribute current in el.Attributes())
            {
                if (current.GetSchemaInfo().Validity != XmlSchemaValidity.Valid)
                {
                    this.errors.Add(string.Format("Invalid Attribute {0}", current.Parent.AncestorsAndSelf().InDocumentOrder<XElement>().Aggregate("", (string s, XElement i) => s + "/" + i.Name.ToString()) + "/@" + current.Name.ToString()));
                }
            }
            foreach (XElement current2 in el.Elements())
            {
                this.DumpInvalidNodes(current2);
            }
        }
    }
}
