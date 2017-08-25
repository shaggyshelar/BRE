using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// XML utility class
    /// </summary>
    public static class Xml
    {
        internal const string SourceNamespaceTag = "s";

        private const string currentRuleNamespace = "http://codeeffects.com/schemas/rule/41";

        /// <summary>
        /// Gets the UI XML sub-namespace (location of UI XML schema) used by the current version of Code Effects control
        /// </summary>
        public const string UiNamespace = "http://codeeffects.com/schemas/ui/4";

        /// <summary>
        /// Gets the Source XML namespace (location of Source XML schema) used by the current version of Code Effects control
        /// </summary>
        public const string SourceNamespace = "http://codeeffects.com/schemas/source/42";

        internal static string RuleNamespaceTag
        {
            get
            {
                return "x";
            }
        }

        internal static string UiNamespaceTag
        {
            get
            {
                return "y";
            }
        }

        /// <summary>
        /// Gets the Rule XML namespace (location of Rule XML schema) used by the current version of Code Effects control
        /// </summary>
        public static string RuleNamespace
        {
            get
            {
                return "http://codeeffects.com/schemas/rule/41";
            }
        }

        /// <summary>
        /// Returns an empty Rule XML document
        /// </summary>
        public static XmlDocument GetEmptyRuleDocument()
        {
            return Xml.GetEmptyRuleDocument(RuleFormatType.CodeEffects);
        }

        /// <summary>
        /// Returns an empty Rule XML document
        /// </summary>
        public static XmlDocument GetEmptyRuleDocument(RuleFormatType format)
        {
            if (format == RuleFormatType.CodeEffects)
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.InnerXml = string.Format("<codeeffects xmlns=\"{0}\" xmlns:ui=\"{1}\"></codeeffects>", Xml.RuleNamespace, "http://codeeffects.com/schemas/ui/4");
                xmlDocument.InsertBefore(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null), xmlDocument.DocumentElement);
                return xmlDocument;
            }
            throw new NotSupportedException("This XML format is not supported by this version of Code Effects.");
        }

        /// <summary>
        /// Validates Rule XML document against its current schema.
        /// </summary>
        /// <param name="sourceXml">Source XML document</param>
        public static bool IsRuleValid(XmlDocument ruleXml)
        {
            return new RuleXmlValidator().Validate(ruleXml.OuterXml);
        }

        internal static bool IsVersion2XmlNamespace(string namespaceUri)
        {
            string key;
            switch (key = namespaceUri.ToLower())
            {
                case "http://codeeffects.com/schemas/rule":
                case "http://rule.codeeffects.com/schemas/rule":
                case "http://codeeffects.com/schemas/rule/3":
                case "http://rule.codeeffects.com/schemas/rule/3":
                case "http://codeeffects.com/schemas/rule/4":
                case "http://rule.codeeffects.com/schemas/rule/4":
                case "http://rule.codeeffects.com/schemas/rule/41":
                case "http://codeeffects.com/schemas/rule/41":
                    return true;
            }
            return false;
        }

        internal static string GetUiNamespaceByRuleNamespace(string ruleNamespaceUri)
        {
            string key;
            switch (key = ruleNamespaceUri.ToLower())
            {
                case "http://codeeffects.com/schemas/rule":
                case "http://rule.codeeffects.com/schemas/rule":
                    return "http://codeeffects.com/schemas/ui";
                case "http://codeeffects.com/schemas/rule/3":
                case "http://rule.codeeffects.com/schemas/rule/3":
                    return "http://codeeffects.com/schemas/ui/3";
                case "http://codeeffects.com/schemas/rule/4":
                case "http://rule.codeeffects.com/schemas/rule/4":
                case "http://rule.codeeffects.com/schemas/rule/41":
                case "http://codeeffects.com/schemas/rule/41":
                    return "http://codeeffects.com/schemas/ui/4";
            }
            throw new InvalidRuleException(InvalidRuleException.ErrorIds.UnknownNameSpace, new string[]
			{
				ruleNamespaceUri
			});
        }

        public static string GetSourceNameByNamespace(string sourceNamespaceUri)
        {
            string key;
            switch (key = sourceNamespaceUri.ToLower())
            {
                //case "http://codeeffects.com/schemas/source/3":
                //case "http://rule.codeeffects.com/schemas/source/3":
                //    return "CodeEffects.Rule.Resources.Schemas.Source.3.xsd";
                //case "http://rule.codeeffects.com/schemas/source/4":
                //case "http://codeeffects.com/schemas/source/4":
                //    return "CodeEffects.Rule.Resources.Schemas.Source.4.xsd";
                //case "http://rule.codeeffects.com/schemas/source/42":
                //case "http://codeeffects.com/schemas/source/42":
                //    return "CodeEffects.Rule.Resources.Schemas.Source.4.2.xsd";

                case "http://espl.com/schemas/source/3":
                case "http://rule.espl.com/schemas/source/3":
                    return "ESPL.Rule.Resources.Schemas.Source.3.xsd";
                case "http://rule.espl.com/schemas/source/4":
                case "http://espl.com/schemas/source/4":
                    return "ESPL.Rule.Resources.Schemas.Source.4.xsd";
                case "http://rule.espl.com/schemas/source/42":
                case "http://espl.com/schemas/source/42":
                    return "ESPL.Rule.Resources.Schemas.Source.4.2.xsd";
            }
            return "ESPL.Rule.Resources.Schemas.Source.2.xsd";
        }

        internal static XmlDocument GetEmptySourceDocument()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.InnerXml = string.Format("<codeeffects xmlns=\"{0}\"></codeeffects>", "http://codeeffects.com/schemas/source/42");
            //xmlDocument.InnerXml = string.Format("<espl xmlns=\"{0}\"></espl>", "http://espl.com/schemas/source/42");
            xmlDocument.InsertBefore(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"), xmlDocument.DocumentElement);
            return xmlDocument;
        }

        public static void ValidateSchema(XmlDocument doc, string schemaResource)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.CloseInput = true;
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlReaderSettings.ValidationType = ValidationType.Schema;
            using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(schemaResource))
            {
                string targetNamespace = string.Empty;
                XmlReader xmlReader = XmlReader.Create(manifestResourceStream);
                xmlReader.MoveToContent();
                if (xmlReader.MoveToAttribute("targetNamespace"))
                {
                    targetNamespace = xmlReader.Value;
                }
                if (!doc.Schemas.Contains(targetNamespace))
                {
                    manifestResourceStream.Position = 0L;
                    xmlReader = XmlReader.Create(manifestResourceStream, xmlReaderSettings);
                    doc.Schemas.Add(null, xmlReader);
                }
                doc.Validate(new ValidationEventHandler(Xml.ValidationFailed));
                xmlReader.Close();
            }
        }

        internal static XmlNamespaceManager GetSourceNamespaceManager(XmlNode node)
        {
            return Xml.GetSourceNamespaceManager((node.GetType() == typeof(XmlDocument)) ? ((XmlDocument)node) : node.OwnerDocument);
        }

        internal static XmlNamespaceManager GetSourceNamespaceManager(XmlDocument xml)
        {
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xml.NameTable);
            xmlNamespaceManager.AddNamespace("s", xml.DocumentElement.NamespaceURI);
            return xmlNamespaceManager;
        }

        private static void ValidationFailed(object sender, ValidationEventArgs e)
        {
            throw new SourceException(SourceException.ErrorIds.SchemaValidationFailed, new string[]
			{
				e.Message
			});
        }
    }
}
