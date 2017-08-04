using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ESPL.Rule.Client
{
    /// <summary>
    /// This class is not intended for public use
    /// </summary>
    public class Element
    {
        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public ElementType Type
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public CalculationType CalType
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public FunctionType FuncType
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public CollectionType CollType
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public SelectionType SelType
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public bool IsFuncValue
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public InputType InpType
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        [ScriptIgnore]
        public ParameterType ParameterType
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public OperatorType Oper
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public decimal? Min
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public decimal? Max
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public bool Dec
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public bool IsDs
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public bool Cal
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public List<Pair> Enums
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public string En
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public string Format
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public bool IsRule
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public bool IsInstance
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        [ScriptIgnore]
        public bool IsOrganicParenthesis
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        public bool NotFound
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        [ScriptIgnore]
        public string Class
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        [ScriptIgnore]
        public string Assembly
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        [ScriptIgnore]
        public StringComparison StringComparison
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        [ScriptIgnore]
        public string ReturnEnumClass
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        [ScriptIgnore]
        public string ReturnEnumAssembly
        {
            get;
            set;
        }

        /// <summary>
        /// This property is not intended for public use
        /// </summary>
        [ScriptIgnore]
        public string Token
        {
            get;
            set;
        }

        /// <summary>
        /// This constructor is not intended for public use
        /// </summary>
        public Element()
        {
            this.Type = ElementType.Flow;
            this.CalType = CalculationType.None;
            this.FuncType = FunctionType.None;
            this.ParameterType = ParameterType.None;
            this.InpType = InputType.None;
            this.SelType = SelectionType.None;
            this.CollType = CollectionType.None;
            this.Dec = (this.IsOrganicParenthesis = true);
            this.Cal = false;
            this.NotFound = (this.IsDs = false);
            this.StringComparison = StringComparison.OrdinalIgnoreCase;
            this.IsRule = (this.IsInstance = (this.IsFuncValue = false));
            this.Enums = new List<Pair>();
            this.Oper = OperatorType.None;
        }

        /// <summary>
        /// This method is not intended for public use
        /// </summary>
        public Element Clone()
        {
            Element element = new Element();
            element.Cal = this.Cal;
            element.CalType = this.CalType;
            element.Dec = this.Dec;
            element.En = this.En;
            element.Enums = this.Enums;
            element.Format = this.Format;
            element.FuncType = this.FuncType;
            element.IsFuncValue = this.IsFuncValue;
            element.InpType = this.InpType;
            element.Max = this.Max;
            element.Min = this.Min;
            element.Name = this.Name;
            element.Oper = this.Oper;
            element.Type = this.Type;
            element.Value = this.Value;
            element.Assembly = this.Assembly;
            element.StringComparison = this.StringComparison;
            element.Class = this.Class;
            element.IsInstance = this.IsInstance;
            element.IsRule = this.IsRule;
            element.IsDs = this.IsDs;
            element.NotFound = this.NotFound;
            element.Token = this.Token;
            element.IsOrganicParenthesis = element.IsOrganicParenthesis;
            element.ParameterType = this.ParameterType;
            element.ReturnEnumAssembly = this.ReturnEnumAssembly;
            element.ReturnEnumClass = this.ReturnEnumClass;
            element.SelType = this.SelType;
            element.CollType = this.CollType;
            return element;
        }

        /// <summary>
        /// This method is not intended for public use
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{");
            switch (this.Type)
            {
                case ElementType.Flow:
                case ElementType.Clause:
                case ElementType.LeftSource:
                case ElementType.RightSource:
                    this.WriteString("n", ESPL.Rule.Core.Encoder.Sanitize(this.Name), stringBuilder);
                    this.WriteString("v", ESPL.Rule.Core.Encoder.Sanitize(this.Value), stringBuilder);
                    break;
                case ElementType.Field:
                    {
                        this.WriteString("n", ESPL.Rule.Core.Encoder.Sanitize(this.Name), stringBuilder);
                        this.WriteString("v", ESPL.Rule.Core.Encoder.Sanitize(this.Value), stringBuilder);
                        this.WriteInteger("l", this.IsRule ? 1 : 0, stringBuilder);
                        this.WriteInteger("d", this.NotFound ? 1 : 0, stringBuilder);
                        this.WriteInteger("o", int.Parse(Enum.Format(typeof(OperatorType), this.Oper, "D")), stringBuilder);
                        OperatorType oper = this.Oper;
                        if (oper == OperatorType.Enum)
                        {
                            this.WriteString("e", this.En, stringBuilder);
                        }
                        break;
                    }
                case ElementType.Function:
                case ElementType.Action:
                    this.WriteString("n", ESPL.Rule.Core.Encoder.Sanitize(this.Name), stringBuilder);
                    this.WriteInteger("f", int.Parse(Enum.Format(typeof(FunctionType), this.FuncType, "D")), stringBuilder);
                    this.WriteInteger("d", this.NotFound ? 1 : 0, stringBuilder);
                    switch (this.FuncType)
                    {
                        case FunctionType.Name:
                        case FunctionType.End:
                            this.WriteString("v", ESPL.Rule.Core.Encoder.Sanitize(this.Value), stringBuilder);
                            this.WriteInteger("o", int.Parse(Enum.Format(typeof(OperatorType), this.Oper, "D")), stringBuilder);
                            if (this.Type == ElementType.Function)
                            {
                                this.WriteInteger("q", this.IsFuncValue ? 1 : 0, stringBuilder);
                            }
                            break;
                        case FunctionType.Param:
                            this.WriteString("v", ESPL.Rule.Core.Encoder.Sanitize(this.Value), stringBuilder);
                            this.WriteInteger("i", int.Parse(Enum.Format(typeof(InputType), this.InpType, "D")), stringBuilder);
                            switch (this.InpType)
                            {
                                case InputType.Field:
                                    this.WriteInteger("o", int.Parse(Enum.Format(typeof(OperatorType), this.Oper, "D")), stringBuilder);
                                    break;
                                case InputType.Input:
                                    this.WriteInteger("o", int.Parse(Enum.Format(typeof(OperatorType), this.Oper, "D")), stringBuilder);
                                    switch (this.Oper)
                                    {
                                        case OperatorType.Date:
                                        case OperatorType.Time:
                                            this.WriteString("r", this.Format, stringBuilder);
                                            break;
                                        case OperatorType.Enum:
                                            this.WriteString("e", this.En, stringBuilder);
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
                case ElementType.Operator:
                case ElementType.Setter:
                    this.WriteString("n", ESPL.Rule.Core.Encoder.Sanitize(this.Name), stringBuilder);
                    this.WriteString("v", ESPL.Rule.Core.Encoder.Sanitize(this.Value), stringBuilder);
                    this.WriteInteger("o", int.Parse(Enum.Format(typeof(OperatorType), this.Oper, "D")), stringBuilder);
                    break;
                case ElementType.Value:
                    this.WriteString("n", ESPL.Rule.Core.Encoder.Sanitize(this.Name), stringBuilder);
                    this.WriteString("v", ESPL.Rule.Core.Encoder.Sanitize(this.Value), stringBuilder);
                    this.WriteInteger("i", int.Parse(Enum.Format(typeof(InputType), this.InpType, "D")), stringBuilder);
                    this.WriteInteger("o", int.Parse(Enum.Format(typeof(OperatorType), this.Oper, "D")), stringBuilder);
                    break;
                case ElementType.LeftParenthesis:
                case ElementType.RightParenthesis:
                case ElementType.LeftBracket:
                case ElementType.RightBracket:
                    this.WriteString("n", ESPL.Rule.Core.Encoder.Sanitize(this.Name), stringBuilder);
                    break;
                case ElementType.Calculation:
                    {
                        this.WriteString("n", ESPL.Rule.Core.Encoder.Sanitize(this.Name), stringBuilder);
                        this.WriteInteger("c", int.Parse(Enum.Format(typeof(CalculationType), this.CalType, "D")), stringBuilder);
                        CalculationType calType = this.CalType;
                        if (calType != CalculationType.Field)
                        {
                            switch (calType)
                            {
                                case CalculationType.Number:
                                    this.WriteString("v", ESPL.Rule.Core.Encoder.Sanitize(this.Value), stringBuilder);
                                    break;
                                case CalculationType.Function:
                                    this.WriteInteger("f", int.Parse(Enum.Format(typeof(FunctionType), this.FuncType, "D")), stringBuilder);
                                    this.WriteInteger("d", this.NotFound ? 1 : 0, stringBuilder);
                                    this.WriteString("v", ESPL.Rule.Core.Encoder.Sanitize(this.Value), stringBuilder);
                                    this.WriteInteger("o", int.Parse(Enum.Format(typeof(OperatorType), this.Oper, "D")), stringBuilder);
                                    break;
                            }
                        }
                        else
                        {
                            this.WriteInteger("d", this.NotFound ? 1 : 0, stringBuilder);
                            this.WriteString("v", ESPL.Rule.Core.Encoder.Sanitize(this.Value), stringBuilder);
                        }
                        break;
                    }
                case ElementType.Where:
                case ElementType.Exists:
                    this.WriteString("n", ESPL.Rule.Core.Encoder.Sanitize(this.Name), stringBuilder);
                    if (this.Value != null)
                    {
                        this.WriteString("v", ESPL.Rule.Core.Encoder.Sanitize(this.Value), stringBuilder);
                    }
                    else
                    {
                        this.WriteInteger("y", int.Parse(Enum.Format(typeof(SelectionType), this.SelType, "D")), stringBuilder);
                    }
                    break;
            }
            stringBuilder.Append("t:").Append(int.Parse(Enum.Format(typeof(ElementType), this.Type, "D"))).Append("}");
            return stringBuilder.ToString();
        }

        private void WriteString(string name, string value, StringBuilder sb)
        {
            sb.Append(name).Append(":");
            if (value == null)
            {
                sb.Append("null,");
                return;
            }
            sb.Append("\"").Append(value).Append("\",");
        }

        private void WriteInteger(string name, int value, StringBuilder sb)
        {
            sb.Append(name).Append(":");
            sb.Append(value).Append(",");
        }
    }
}
