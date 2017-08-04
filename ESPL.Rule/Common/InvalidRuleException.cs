using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// Used by ESPL control for rule validation-related exceptions
    /// </summary>
    public class InvalidRuleException : RuleException
    {
        public enum ErrorIds
        {
            FlowTypeElementExcpected = 100,
            RuleXMLIsInvalid,
            ReferencedRuleNotFound,
            RuleNotFound,
            InvalidOrderOfNodes = 105,
            UnexpectedOrderOfCalculationNodes = 107,
            UnecpectedCalculationType,
            MissingControl = 114,
            RuleIsInvalid,
            FailtureToValidate,
            XMLFileNotFound,
            XMLFileIsMalformed,
            ParameterIsNull,
            RuleXMLNotFound,
            InvalidNumberOfChildNodes = 124,
            RuleXMLIsInvalid2,
            InvalidNumberOfConditionChildNodes,
            UnexpectedChildNodeInCondition,
            UnknownNameSpace,
            UnexpectedNumberOfParameters,
            UnexpectedValueSetters,
            InvalidDefinitionStructure,
            InvalidNumberOfIfElements
        }

        internal InvalidRuleException(InvalidRuleException.ErrorIds error, params string[] parameters)
            : base("i" + (int)error, parameters)
        {
        }
    }
}
