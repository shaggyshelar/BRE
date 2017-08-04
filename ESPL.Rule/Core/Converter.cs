using ESPL.Rule.Client;
using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    internal sealed class Converter
    {
        private Converter()
        {
        }

        internal static string ThemeTypeToResourceName(ThemeType theme)
        {
            switch (theme)
            {
                case ThemeType.White:
                    return "CodeEffects.Rule.Resources.Styles.White.css";
                case ThemeType.Green:
                    return "CodeEffects.Rule.Resources.Styles.Green.css";
                case ThemeType.Red:
                    return "CodeEffects.Rule.Resources.Styles.Red.css";
                case ThemeType.Black:
                    return "CodeEffects.Rule.Resources.Styles.Black.css";
                case ThemeType.Blue:
                    return "CodeEffects.Rule.Resources.Styles.Blue.css";
                case ThemeType.Navy:
                    return "CodeEffects.Rule.Resources.Styles.Navy.css";
                default:
                    return "CodeEffects.Rule.Resources.Styles.Gray.css";
            }
        }

        internal static string ClientTypeToClientString(OperatorType type)
        {
            switch (type)
            {
                case OperatorType.String:
                    return "string";
                case OperatorType.Numeric:
                    return "numeric";
                case OperatorType.Date:
                    return "date";
                case OperatorType.Time:
                    return "time";
                case OperatorType.Bool:
                    return "bool";
                case OperatorType.Enum:
                    return "enum";
                case OperatorType.Collection:
                    return "collection";
            }
            throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownDataType, new string[0]);
        }

        internal static OperatorType ClientStringToClientType(string val)
        {
            string key;
            switch (key = val.ToLower())
            {
                case "string":
                    return OperatorType.String;
                case "numeric":
                    return OperatorType.Numeric;
                case "date":
                    return OperatorType.Date;
                case "time":
                    return OperatorType.Time;
                case "enum":
                    return OperatorType.Enum;
                case "bool":
                    return OperatorType.Bool;
                case "collection":
                    return OperatorType.Collection;
            }
            throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownDataType, new string[0]);
        }

        internal static string FeatureLocationToString(FeatureLocation location)
        {
            switch (location)
            {
                case FeatureLocation.Client:
                    return "client";
                case FeatureLocation.Server:
                    return "server";
                default:
                    throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownDataSourceLocationType, new string[0]);
            }
        }

        internal static FeatureLocation StringToFeatureLocation(string location)
        {
            if (location != null)
            {
                if (location == "client")
                {
                    return FeatureLocation.Client;
                }
                if (location == "server")
                {
                    return FeatureLocation.Server;
                }
            }
            throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownDataSourceLocationString, new string[0]);
        }

        internal static ValueInputType StringToValueInputType(string type)
        {
            if (type != null)
            {
                if (type == "fields")
                {
                    return ValueInputType.Fields;
                }
                if (type == "user")
                {
                    return ValueInputType.User;
                }
            }
            return ValueInputType.All;
        }

        internal static string ValueInputTypeToString(ValueInputType type)
        {
            switch (type)
            {
                case ValueInputType.Fields:
                    return "fields";
                case ValueInputType.User:
                    return "user";
            }
            return "all";
        }

        internal static string InputTypeToString(InputType type)
        {
            switch (type)
            {
                case InputType.Field:
                    return "field";
                case InputType.Input:
                    return "input";
                default:
                    throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownInputType, new string[0]);
            }
        }

        internal static InputType StringToInputType(string val)
        {
            string a;
            if ((a = val.ToLower()) != null)
            {
                if (a == "field")
                {
                    return InputType.Field;
                }
                if (a == "input")
                {
                    return InputType.Input;
                }
            }
            throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownInputType, new string[0]);
        }

        internal static CalculationType StringToCalculationType(string val)
        {
            string a;
            if ((a = val.ToLower()) != null)
            {
                if (a == "add")
                {
                    return CalculationType.Addition;
                }
                if (a == "divide")
                {
                    return CalculationType.Division;
                }
                if (a == "multiply")
                {
                    return CalculationType.Multiplication;
                }
                if (a == "subtract")
                {
                    return CalculationType.Subtraction;
                }
            }
            throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownInputType, new string[0]);
        }

        internal static ParameterType StringToParameterType(string val)
        {
            string a;
            if ((a = val.ToLower()) != null)
            {
                if (a == "source")
                {
                    return ParameterType.Source;
                }
                if (a == "input")
                {
                    return ParameterType.Input;
                }
                if (a == "constant")
                {
                    return ParameterType.Constant;
                }
                if (a == "collection")
                {
                    return ParameterType.Collection;
                }
            }
            throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownParameterType, new string[0]);
        }

        internal static CollectionType StringToCollectionType(string val)
        {
            if (val != null)
            {
                if (val == "reference")
                {
                    return CollectionType.Reference;
                }
                if (val == "value")
                {
                    return CollectionType.Value;
                }
                if (val == "generic")
                {
                    return CollectionType.Generic;
                }
            }
            throw new MalformedXmlException(MalformedXmlException.ErrorIds.UnknownCollectionType, new string[0]);
        }

        internal static string SelectionTypeToString(SelectionType type)
        {
            switch (type)
            {
                case SelectionType.Exists:
                    return "exists";
                case SelectionType.DoesNotExist:
                    return "doesNotExist";
                default:
                    throw new MalformedXmlException(MalformedXmlException.ErrorIds.InvalidSelectionType, new string[0]);
            }
        }

        internal static SelectionType StringToSelectionType(string val)
        {
            if (val != null)
            {
                if (val == "doesNotExist")
                {
                    return SelectionType.DoesNotExist;
                }
                if (val == "exists")
                {
                    return SelectionType.Exists;
                }
            }
            throw new MalformedXmlException(MalformedXmlException.ErrorIds.InvalidSelectionType, new string[0]);
        }
    }
}
