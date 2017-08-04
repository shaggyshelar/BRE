using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// Used by ESPL control for XML validation-related exceptions
    /// </summary>
    public class MalformedXmlException : RuleException
    {
        public enum ErrorIds
        {
            UnknownConditionType = 102,
            UnknownNodeName,
            MissingActionsNode,
            ReferenceTypedCollectionNotAllowed,
            MissingFieldsNode,
            MissingFieldsWithPropertyName,
            RuleDelegateIsNull,
            GetRuleDelegateInvokeError,
            UnknownDataSourceLocationType,
            UnknownDataSourceLocationString,
            MissingEnumMember,
            InvalidSelectionType,
            UnknownDataType,
            MalformedOrMissingSourceXML = 129,
            MalformedOrMissingHelpXML,
            CouldNotLoadRuleXML,
            InvalidFieldName = 138,
            MissingMinAttribute = 148,
            MissingMaxAttribute,
            InvalidMinValue = 152,
            MissingMaxlengthAttribute = 155,
            CollectionsAsConstantParams = 200,
            InvalidBoolParamValue = 204,
            InvalidDateParamValue,
            InvalidTimeParamValue,
            InvalidStringParamLength,
            InvalidNumericParamFormat,
            TooManyOrMissingReturn = 211,
            MissingReturnClassAttribute,
            UnknownParameterType = 215,
            UnknownCollectionType = 218,
            UnknownInputType = 225
        }

        internal MalformedXmlException(MalformedXmlException.ErrorIds error, params string[] parameters)
            : base("m" + (int)error, parameters)
        {
        }
    }
}
