using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// Used by Code Effects control for source object-related exceptions
    /// </summary>
    public class SourceException : RuleException
    {
        public enum ErrorIds
        {
            FailureToLoadSourceXML = 100,
            AssemblyNameIsEmpty,
            AssemlbyDoesNotDeclareSourceType,
            FailedToFindOrLoadAssembly,
            AssemblyDoesNotContainType,
            FailedToLoadAssemblyOrType,
            ExternalMethodHasOverloads,
            MissingPublicValueTypeFields,
            MethodHasOverloads,
            MethodIsDecoratedButInvalid = 111,
            NoMethodForTheToken,
            DuplicateDataAttributes,
            MethodIsNotDataSource,
            MissingOrInaccessibleMethod,
            MethodInvokeOrConversionError,
            InvalidMethodParameters,
            InvalidDataSourceMethod,
            DelegateInvokeOrConversionError,
            EnumMethodParamNotSupported,
            GenericCollectionsAndPropertiesNotSupported,
            NullableReturnNotSupported,
            ReturnTypeNotSupported,
            MethodNotFound,
            MultipleDynamicMenuDataSources,
            InvalidCollectionParameterXML,
            InvalidSourceXML,
            SchemaValidationFailed,
            SourceNodeNotFound = 130
        }

        internal SourceException(SourceException.ErrorIds error, params string[] parameters)
            : base("s" + (int)error, parameters)
        {
        }

        internal SourceException(string error, params string[] parameters)
            : base(error, parameters)
        {
        }
    }
}
