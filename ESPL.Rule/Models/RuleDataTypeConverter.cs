using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;

namespace ESPL.Rule.Models
{
    /// <summary>
    /// This class is not intended for public use
    /// </summary>
    public class RuleDataTypeConverter : TypeConverter
    {
        /// <summary>
        /// This method is not intended for public use
        /// </summary>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return this.GetRuleModel(value as string);
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// This method is not intended for public use
        /// </summary>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                RuleModel obj = value as RuleModel;
                return new JavaScriptSerializer().Serialize(obj);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// This method is not intended for public use
        /// </summary>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// This method is not intended for public use
        /// </summary>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// This method is not intended for public use
        /// </summary>
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            return base.CreateInstance(context, propertyValues);
        }

        internal RuleModel GetRuleModel(string ruleClientData)
        {
            return this.GetRuleModel(ruleClientData, null, null, null);
        }

        internal RuleModel GetRuleModel(string ruleClientData, string sourceAssembly, string sourceType, XmlDocument sourceXml)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            RuleModel ruleModel = javaScriptSerializer.Deserialize<RuleModel>(ruleClientData);
            if (string.IsNullOrWhiteSpace(ruleModel.Id))
            {
                ruleModel.Id = Guid.NewGuid().ToString();
            }
            if (ruleModel.Name != null)
            {
                ruleModel.Name = ESPL.Rule.Core.Encoder.Desanitize(ruleModel.Name);
            }
            if (ruleModel.Desc != null)
            {
                ruleModel.Desc = ESPL.Rule.Core.Encoder.Desanitize(ruleModel.Desc);
            }
            if (sourceAssembly != null && sourceType != null)
            {
                ruleModel.SourceAssembly = sourceAssembly;
                ruleModel.SourceType = sourceType;
            }
            if (sourceXml != null)
            {
                ruleModel.SourceXml = sourceXml;
            }
            return ruleModel;
        }
    }
}
