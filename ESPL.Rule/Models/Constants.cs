using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Models
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct Constants
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct DateTimeFormats
        {
            internal const string TimeStorageWithSeconds = "HH:mm:ss";

            internal const string TimeStorageNoSeconds = "HH:mm";

            internal const string TimeClientDisplay = "hh:mm tt";

            internal const string DateClientDisplay = "MMM dd, yyyy";

            internal const string DateClientTransport = "M/d/yyyy H:m:s";

            internal const string Storage = "yyyy-MM-ddTHH:mm:ss.ffff";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Variables
        {
            internal const string DefaultSourceName = "Default.Source.Name";

            internal const string True = "true";

            internal const string False = "false";

            internal const string Input = "input";

            internal const string Field = "field";

            internal const string Calculation = "calculation";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct ValueInputType
        {
            internal const string Fields = "fields";

            internal const string User = "user";

            internal const string All = "all";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct FeatureLocation
        {
            internal const string Client = "client";

            internal const string Server = "server";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Field
        {
            internal const string String = "string";

            internal const string Date = "date";

            internal const string Time = "time";

            internal const string Numeric = "numeric";

            internal const string Bool = "bool";

            internal const string Enum = "enum";

            internal const string Collection = "collection";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Flow
        {
            internal const string If = "if";

            internal const string ElseIf = "elseIf";

            internal const string Else = "else";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Clauses
        {
            internal const string And = "and";

            internal const string Or = "or";

            internal const string Then = "then";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Setters
        {
            internal const string Set = "set";

            internal const string To = "to";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Collections
        {
            internal const string Exists = "exists";

            internal const string DoesNotExist = "doesNotExist";

            internal const string Where = "where";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Parameter
        {
            internal const string Source = "source";

            internal const string Input = "input";

            internal const string Constant = "constant";

            internal const string Collection = "collection";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct LegacyEqualityOperators
        {
            internal const string Is = "is";

            internal const string IsNot = "isNot";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct BaseOperators
        {
            internal const string Is = "equal";

            internal const string IsNot = "notEqual";

            internal const string IsNull = "isNull";

            internal const string IsNotNull = "isNotNull";

            internal const string Less = "less";

            internal const string LessOrEqual = "lessOrEqual";

            internal const string Greater = "greater";

            internal const string GreaterOrEqual = "greaterOrEqual";

            internal const string Contains = "contains";

            internal const string DoesNotContain = "doesNotContain";

            internal const string StartsWith = "startsWith";

            internal const string DoesNotStartWith = "doesNotStartWith";

            internal const string EndsWith = "endsWith";

            internal const string DoesNotEndWith = "doesNotEndWith";

            internal const string Between = "between";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Operators
        {
            [StructLayout(LayoutKind.Sequential, Size = 1)]
            internal struct Bool
            {
                internal const string Is = "equal";

                internal const string IsNull = "isNull";

                internal const string IsNotNull = "isNotNull";
            }

            [StructLayout(LayoutKind.Sequential, Size = 1)]
            internal struct Enum
            {
                internal const string Is = "equal";

                internal const string IsNot = "notEqual";
            }

            [StructLayout(LayoutKind.Sequential, Size = 1)]
            internal struct Date
            {
                internal const string Is = "equal";

                internal const string IsNot = "notEqual";

                internal const string Less = "less";

                internal const string LessOrEqual = "lessOrEqual";

                internal const string Greater = "greater";

                internal const string GreaterOrEqual = "greaterOrEqual";

                internal const string IsNull = "isNull";

                internal const string IsNotNull = "isNotNull";
            }

            [StructLayout(LayoutKind.Sequential, Size = 1)]
            internal struct Time
            {
                internal const string Is = "equal";

                internal const string IsNot = "notEqual";

                internal const string Less = "less";

                internal const string LessOrEqual = "lessOrEqual";

                internal const string Greater = "greater";

                internal const string GreaterOrEqual = "greaterOrEqual";

                internal const string IsNull = "isNull";

                internal const string IsNotNull = "isNotNull";
            }

            [StructLayout(LayoutKind.Sequential, Size = 1)]
            internal struct Numeric
            {
                internal const string Is = "equal";

                internal const string IsNot = "notEqual";

                internal const string Less = "less";

                internal const string LessOrEqual = "lessOrEqual";

                internal const string Greater = "greater";

                internal const string GreaterOrEqual = "greaterOrEqual";

                internal const string IsNull = "isNull";

                internal const string IsNotNull = "isNotNull";
            }

            [StructLayout(LayoutKind.Sequential, Size = 1)]
            internal struct String
            {
                internal const string Is = "equal";

                internal const string IsNot = "notEqual";

                internal const string Contains = "contains";

                internal const string DoesNotContain = "doesNotContain";

                internal const string StartsWith = "startsWith";

                internal const string DoesNotStartWith = "doesNotStartWith";

                internal const string EndsWith = "endsWith";

                internal const string DoesNotEndWith = "doesNotEndWith";

                internal const string IsNull = "isNull";

                internal const string IsNotNull = "isNotNull";
            }

            [StructLayout(LayoutKind.Sequential, Size = 1)]
            internal struct Collection
            {
                internal const string Contains = "contains";

                internal const string DoesNotContain = "doesNotContain";

                internal const string StartsWith = "startsWith";

                internal const string DoesNotStartWith = "doesNotStartWith";

                internal const string EndsWith = "endsWith";

                internal const string DoesNotEndWith = "doesNotEndWith";

                internal const string IsNull = "isNull";

                internal const string IsNotNull = "isNotNull";
            }

            [StructLayout(LayoutKind.Sequential, Size = 1)]
            internal struct Source
            {
                internal const string Exists = "exists";

                internal const string DoesNotExist = "doesNotExist";
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Nodes
        {
            internal const string While = "while";

            internal const string Do = "do";

            internal const string Not = "not";

            internal const string Set = "set";

            internal const string Source = "source";

            internal const string Self = "self";

            internal const string Rule = "rule";

            internal const string Definition = "definition";

            internal const string Name = "name";

            internal const string Description = "description";

            internal const string Expression = "expression";

            internal const string Format = "format";

            internal const string Lines = "lines";

            internal const string Then = "then";

            internal const string Value = "value";

            internal const string Generic = "generic";

            internal const string Reference = "reference";

            internal const string Condition = "condition";

            internal const string Function = "function";

            internal const string Method = "method";

            internal const string Parameters = "parameters";

            internal const string Returns = "returns";

            internal const string Operator = "operator";

            internal const string Calculation = "calculation";

            internal const string Group = "group";

            internal const string Parentheses = "parentheses";

            internal const string Property = "property";

            internal const string Number = "number";

            internal const string Add = "add";

            internal const string Divide = "divide";

            internal const string Multiply = "multiply";

            internal const string Subtract = "subtract";

            internal const string Clause = "clause";

            internal const string Line = "line";

            internal const string NewLine = "newLine";

            internal const string Tab = "tab";

            internal const string Fields = "fields";

            internal const string Field = "field";

            internal const string Actions = "actions";

            internal const string Action = "action";

            internal const string DataSources = "dataSources";

            internal const string DataSource = "dataSource";

            internal const string EvaluationIf = "evaluationIf";

            internal const string ExecutionIf = "executionIf";

            internal const string LoopIf = "loopIf";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Attributes
        {
            internal const string ValueInputType = "valueInputType";

            internal const string Type = "type";

            internal const string Persisted = "persisted";

            internal const string Block = "block";

            internal const string Max = "max";

            internal const string Min = "min";

            internal const string MaxLength = "maxLength";

            internal const string Name = "name";

            internal const string DisplayName = "displayName";

            internal const string Description = "description";

            internal const string Property = "property";

            internal const string PropertyName = "propertyName";

            internal const string AllowDecimal = "allowDecimal";

            internal const string AllowCalculation = "allowCalculation";

            internal const string IncludeInCalculations = "includeInCalculations";

            internal const string Format = "format";

            internal const string Value = "value";

            internal const string Operator = "operator";

            internal const string Class = "class";

            internal const string Assembly = "assembly";

            internal const string MethodName = "methodName";

            internal const string Nullable = "nullable";

            internal const string Settable = "settable";

            internal const string Gettable = "gettable";

            internal const string DataType = "dataType";

            internal const string Utc = "utc";

            internal const string Tabs = "tabs";

            internal const string Token = "token";

            internal const string Index = "index";

            internal const string Id = "id";

            internal const string Instance = "instance";

            internal const string ToolBar = "toolbar";

            internal const string WebRule = "webrule";

            internal const string IsEvalType = "eval";

            internal const string Location = "location";

            internal const string DataSourceName = "dataSourceName";

            internal const string StringComparison = "stringComparison";

            internal const string Generic = "generic";

            internal const string Array = "array";

            internal const string ItemType = "itemType";

            internal const string ComparisonName = "comparisonName";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct Scopes
        {
            internal const string CalculationBegin = "calculationBegin";

            internal const string CalculationEnd = "calculationEnd";

            internal const string SectionBegin = "sectionBegin";

            internal const string SectionEnd = "sectionEnd";

            internal const string CollectionBegin = "collectionBegin";

            internal const string CollectionEnd = "collectionEnd";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct HelpXMLPaths
        {
            internal const string Validation = "/codeeffects/validation";

            internal const string Help = "/codeeffects/help";

            internal const string Errors = "/codeeffects/errors";

            internal const string Flows = "/codeeffects/flow";

            internal const string Clauses = "/codeeffects/clauses";

            internal const string Collections = "/codeeffects/collections";

            internal const string Scopes = "/codeeffects/scopes";

            internal const string Setters = "/codeeffects/setters";

            internal const string Operators = "/codeeffects/operators";

            internal const string Labels = "/codeeffects/labels";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct SourceXMLPaths
        {
            internal const string Fields = "{0}:fields";

            internal const string Actions = "{0}:actions";

            internal const string DataSources = "{0}:dataSources";
        }

        internal class ErrorXMLPaths
        {
            internal const string Exceptions = "/codeeffects/values";
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct ViewState
        {
            internal const string SourceXmlFile = "ce701";

            internal const string SourceType = "ce702";

            internal const string SourceAssembly = "ce703";

            internal const string CacheSource = "ce704";

            internal const string ClientOnly = "ce705";

            internal const string ShowLineDots = "ce706";

            internal const string ShowMenuOnRightArrowKey = "ce707";

            internal const string ShowMenuOnElementClicked = "ce708";

            internal const string ShowDescriptionsOnMouseHover = "ce711";

            internal const string RuleNameIsRequired = "ce712";

            internal const string ShowToolBar = "ce709";

            internal const string KeepDeclaredOrder = "ce713";

            internal const string Mode = "ce710";

            internal const string Theme = "ce824";

            internal const string HelpXmlFile = "ce601";

            internal const string ShowHelpString = "ce604";

            internal const string CacheHelp = "ce603";
        }

        internal const long MaxStringLength = 256L;
    }
}
