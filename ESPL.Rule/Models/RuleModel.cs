using ESPL.Rule.Client;
using ESPL.Rule.Common;
using ESPL.Rule.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Models
{
    /// <summary>
    /// Business rule model class
    /// </summary>
    [TypeConverter(typeof(RuleDataTypeConverter))]
    public class RuleModel
    {
        private bool validated;

        private bool valid;

        private RuleType mode;

        internal bool NameIsInvalid
        {
            get;
            set;
        }

        internal RuleFormatType Format
        {
            get;
            set;
        }

        internal string SourceType
        {
            get;
            set;
        }

        internal string SourceAssembly
        {
            get;
            set;
        }

        internal string SourceXmlFile
        {
            get;
            set;
        }

        internal XmlDocument SourceXml
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ID of the rule
        /// </summary>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the rule. Users set this value by
        /// typing the rule's name into the Name text box on the Tool Bar
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the rule. Users set this value by
        /// typing the rule's description into the Description text box of the Tool Bar
        /// </summary>
        public string Desc
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating if Code Effects control should disallow rules with empty names
        /// </summary>
        public bool SkipNameValidation
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public string Command
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating if the rule is of evaluation type.
        /// </summary>
        public bool? IsLoadedRuleOfEvalType
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public RuleType Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (value == RuleType.Filter)
                {
                    this.SkipNameValidation = true;
                }
                this.mode = value;
            }
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public List<Element> Elements
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public List<InvalidElement> InvalidElements
        {
            get;
            set;
        }

        /// <summary>
        /// Empty public c-tor
        /// </summary>
        public RuleModel()
        {
            this.Mode = RuleType.Evaluation;
            this.NameIsInvalid = (this.validated = (this.valid = (this.SkipNameValidation = false)));
            this.Format = RuleFormatType.CodeEffects;
            this.Elements = new List<Element>();
            this.InvalidElements = new List<InvalidElement>();
        }

        /// <summary>
        /// Returns a new instance of RuleModel class.
        /// </summary>
        /// <param name="sourceAssembly">Fully qualified name of the assembly that declares the source object</param>
        /// <param name="sourceType">Full name of source object's type</param>
        /// <returns>Instance of RuleModel class</returns>
        public static RuleModel Create(string sourceAssembly, string sourceType)
        {
            return new RuleModel
            {
                SourceAssembly = sourceAssembly,
                SourceType = sourceType
            };
        }

        /// <summary>
        /// Returns a new instance of RuleModel class.
        /// </summary>
        /// <param name="sourceXmlFile">Full path to a Source XML file</param>
        /// <returns>Instance of RuleModel class</returns>
        public static RuleModel Create(string sourceXmlFile)
        {
            return new RuleModel
            {
                SourceXmlFile = sourceXmlFile
            };
        }

        /// <summary>
        /// Returns a new instance of RuleModel class.
        /// </summary>
        /// <param name="sourceXml">Source XML document</param>
        /// <returns>Instance of RuleModel class</returns>
        public static RuleModel Create(XmlDocument sourceXml)
        {
            return new RuleModel
            {
                SourceXml = sourceXml
            };
        }

        /// <summary>
        /// Returns a new instance of RuleModel class.
        /// </summary>
        /// <param name="sourceType">Type that declares the source object</param>
        /// <returns>Instance of RuleModel class</returns>
        public static RuleModel Create(Type sourceType)
        {
            return new RuleModel
            {
                SourceType = sourceType.FullName,
                SourceAssembly = sourceType.Assembly.FullName
            };
        }

        /// <summary>
        /// Returns a new instance of RuleModel class.
        /// </summary>
        /// <param name="ruleXml">Rule XML document as string</param>
        /// <param name="sourceAssembly">Fully qualified name of the assembly that declares the source object</param>
        /// <param name="sourceType">Full name of source object's type</param>
        /// <returns>Instance of RuleModel class</returns>
        public static RuleModel Create(string ruleXml, string sourceAssembly, string sourceType)
        {
            return RuleModel.Create(ruleXml, sourceAssembly, sourceType, null);
        }

        /// <summary>
        /// Returns a new instance of RuleModel class.
        /// </summary>
        /// <param name="ruleXml">Rule XML document as string</param>
        /// <param name="sourceAssembly">Fully qualified name of the assembly that declares the source object</param>
        /// <param name="sourceType">Full name of source object's type</param>
        /// <param name="ruleDelegate">Method that takes rule ID and returns rule XML string.
        /// Pass this parameter if you store each rule in a separate XML document.</param>
        /// <returns>Instance of RuleModel class</returns>
        public static RuleModel Create(string ruleXml, string sourceAssembly, string sourceType, GetRuleDelegate ruleDelegate)
        {
            List<Type> processedTypes = new List<Type>();
            RuleModel ruleModel = RuleModel.LoadModel(ruleXml, SourceLoader.GetXml(sourceAssembly, sourceType, processedTypes), ruleDelegate);
            ruleModel.SourceAssembly = sourceAssembly;
            ruleModel.SourceType = sourceType;
            return ruleModel;
        }

        /// <summary>
        /// Returns a new instance of RuleModel class.
        /// </summary>
        /// <param name="ruleXml">Rule XML document as string</param>
        /// <param name="sourceXml">Source XML document</param>
        /// <returns>Instance of RuleModel class</returns>
        public static RuleModel Create(string ruleXml, XmlDocument sourceXml)
        {
            return RuleModel.Create(ruleXml, sourceXml, null);
        }

        /// <summary>
        /// Returns a new instance of RuleModel class.
        /// </summary>
        /// <param name="ruleXml">Rule XML document as string</param>
        /// <param name="sourceXml">Source XML document</param>
        /// <param name="ruleDelegate">Method that takes rule ID and returns rule XML string.
        /// Pass this parameter if you store each rule in a separate XML document.</param>
        /// <returns>Instance of RuleModel class</returns>
        public static RuleModel Create(string ruleXml, XmlDocument sourceXml, GetRuleDelegate ruleDelegate)
        {
            return RuleModel.LoadModel(ruleXml, sourceXml, ruleDelegate);
        }

        /// <summary>
        /// Returns a new instance of RuleModel class.
        /// </summary>
        /// <param name="ruleXml">Rule XML document as string</param>
        /// <param name="sourceType">Type that declares the source object</param>
        /// <returns>Instance of RuleModel class</returns>
        public static RuleModel Create(string ruleXml, Type sourceType)
        {
            return RuleModel.Create(ruleXml, sourceType, null);
        }

        /// <summary>
        /// Returns a new instance of RuleModel class.
        /// </summary>
        /// <param name="ruleXml">Rule XML document as string</param>
        /// <param name="sourceType">Type that declares the source object</param>
        /// <param name="ruleDelegate">Method that takes rule ID and returns rule XML string.
        /// Pass this parameter if you store each rule in a separate XML document.</param>
        /// <returns>Instance of RuleModel class</returns>
        public static RuleModel Create(string ruleXml, Type sourceType, GetRuleDelegate ruleDelegate)
        {
            List<Type> processedTypes = new List<Type>();
            RuleModel ruleModel = RuleModel.LoadModel(ruleXml, SourceLoader.GetXml(sourceType, processedTypes), ruleDelegate);
            ruleModel.SourceAssembly = sourceType.Assembly.FullName;
            ruleModel.SourceType = sourceType.FullName;
            return ruleModel;
        }

        /// <summary>
        /// Returns string representation of Rule XML. Calling this method is the only way to obtain Rule XML in Code Effects control.
        /// </summary>
        public string GetRuleXml()
        {
            if (this.IsEmpty())
            {
                return null;
            }
            if (!this.validated)
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.FailtureToValidate, new string[0]);
            }
            if (!this.valid)
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleIsInvalid, new string[0]);
            }
            List<Element> items = this.CopyElements(this.Elements);
            XmlDocument xmlDocument = this.LoadSourceXml();
            RuleValidator.EnsureIgnoredProperties(items, xmlDocument);
            RuleValidator.EnsureValues(items);
            return RuleLoader.GetXml(items, string.IsNullOrWhiteSpace(this.Id) ? Guid.NewGuid().ToString() : this.Id, this.Name, this.Desc, xmlDocument, RuleFormatType.CodeEffects, this.Mode, this.IsLoadedRuleOfEvalType.Value);
        }

        /// <summary>
        /// Returns a value indicating if the rule is valid. This overload does not check the current rule for circular references.
        /// </summary>
        public bool IsValid()
        {
            return this.IsValid(null);
        }

        /// <summary>
        /// Returns a value indicating if the rule is valid.
        /// </summary>
        /// <param name="ruleDelegate">Method that returns rule XML by rule ID. If this overload is
        /// used, Code Effects control checks the current rule for circular references. See the Reusable Rules
        /// documentation topic for details</param>
        public bool IsValid(GetRuleDelegate ruleDelegate)
        {
            if (!this.IsLoadedRuleOfEvalType.HasValue)
            {
                this.IsLoadedRuleOfEvalType = new bool?(!RuleValidator.HasActions(this.Elements));
            }
            List<InvalidElement> list = RuleValidator.Validate(this.Elements, this.LoadSourceXml(), this.IsLoadedRuleOfEvalType.GetValueOrDefault());
            if (list.Count > 0)
            {
                this.InvalidElements = list;
            }
            else if (ruleDelegate != null)
            {
                this.valid = (this.validated = true);
                list = RuleValidator.CheckRecursion(this.Elements, this.GetRuleXml(), null, ruleDelegate);
            }
            if (list.Count > 0)
            {
                this.InvalidElements = list;
            }
            else
            {
                this.InvalidElements = new List<InvalidElement>();
            }
            if (this.SkipNameValidation)
            {
                this.NameIsInvalid = false;
            }
            else
            {
                this.NameIsInvalid = string.IsNullOrWhiteSpace(this.Name);
            }
            if (string.IsNullOrWhiteSpace(this.Desc))
            {
                this.Desc = null;
            }
            this.validated = true;
            this.valid = (this.InvalidElements.Count == 0 && !this.NameIsInvalid);
            return this.valid;
        }

        /// <summary>
        /// Returns a value indicating if the rule is empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return this.Elements == null || this.Elements.Count == 0;
        }

        /// <summary>
        /// Binds instance of the RuleModel class to source object
        /// </summary>
        /// <param name="sourceAssembly">Fully qualified name of the assembly that declares the source object</param>
        /// <param name="sourceType">Full name of source object's type</param>
        public void BindSource(string sourceAssembly, string sourceType)
        {
            this.SourceAssembly = sourceAssembly;
            this.SourceType = sourceType;
            this.BindSource(this.LoadSourceXml());
        }

        /// <summary>
        /// Binds instance of the RuleModel class to source object
        /// </summary>
        /// <param name="sourceXmlFile">Full path to a Source XML file</param>
        public void BindSource(string sourceXmlFile)
        {
            this.SourceXmlFile = sourceXmlFile;
            this.BindSource(this.LoadSourceXml());
        }

        /// <summary>
        /// Binds instance of the RuleModel class to source object
        /// </summary>
        /// <param name="sourceType">Type that declares the source object</param>
        public void BindSource(Type sourceType)
        {
            this.SourceAssembly = sourceType.Assembly.FullName;
            this.SourceType = sourceType.FullName;
            this.BindSource(this.LoadSourceXml());
        }

        /// <summary>
        /// Binds instance of the RuleModel class to source object
        /// </summary>
        /// <param name="sourceXml">Source XML document</param>
        public void BindSource(XmlDocument sourceXml)
        {
            if (sourceXml == null)
            {
                throw new SourceException(SourceException.ErrorIds.FailureToLoadSourceXML, new string[0]);
            }
            this.SourceXml = sourceXml;
            if (this.Elements != null && this.Elements.Count > 0)
            {
                RuleValidator.EnsureTokens(this.Elements, this.SourceXml);
            }
        }

        internal XmlDocument LoadSourceXml()
        {
            if (this.SourceXml != null)
            {
                return this.SourceXml;
            }
            List<Type> processedTypes = new List<Type>();
            this.SourceXml = SourceLoader.LoadSourceXml(this.SourceAssembly, this.SourceType, this.SourceXmlFile, processedTypes);
            return this.SourceXml;
        }

        private List<Element> CopyElements(List<Element> list)
        {
            List<Element> list2 = new List<Element>();
            foreach (Element current in list)
            {
                if (current.FuncType != FunctionType.Comma)
                {
                    list2.Add(current.Clone());
                }
            }
            return list2;
        }

        private static RuleModel LoadModel(string ruleXml, XmlDocument sourceXml, GetRuleDelegate ruleDelegate)
        {
            RuleModel ruleModel = RuleLoader.LoadXml(ruleXml, sourceXml, ruleDelegate);
            ruleModel.IsLoadedRuleOfEvalType = new bool?(!RuleValidator.HasActions(ruleModel.Elements));
            return ruleModel;
        }
    }
}
