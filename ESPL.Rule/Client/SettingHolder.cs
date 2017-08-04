using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    internal class SettingHolder
    {
        public decimal? Min
        {
            get;
            set;
        }

        public decimal? Max
        {
            get;
            set;
        }

        public bool AllowDecimals
        {
            get;
            set;
        }

        public bool AllowCalculations
        {
            get;
            set;
        }

        public bool IncludeInCalculations
        {
            get;
            set;
        }

        public string DataSourceName
        {
            get;
            set;
        }

        public string Format
        {
            get;
            set;
        }

        public string TypeFullName
        {
            get;
            set;
        }

        public string Assembly
        {
            get;
            set;
        }

        public bool Nullable
        {
            get;
            set;
        }

        public SettingHolder()
        {
            this.Nullable = false;
            this.AllowDecimals = (this.IncludeInCalculations = true);
            this.AllowCalculations = false;
        }

        public override string ToString()
        {
            throw new NotImplementedException("Use the other public overload");
        }

        public string ToString(SettingType elementType, OperatorType dataType)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (elementType != SettingType.Field)
            {
                stringBuilder.Append(",pl:").Append(this.Nullable ? "true" : "false");
            }
            switch (dataType)
            {
                case OperatorType.String:
                    if (this.Max.HasValue)
                    {
                        stringBuilder.Append(",max:").Append(this.Max);
                    }
                    break;
                case OperatorType.Numeric:
                    if (this.Max.HasValue)
                    {
                        stringBuilder.Append(",max:").Append(this.Max);
                    }
                    if (this.Min.HasValue)
                    {
                        stringBuilder.Append(",min:").Append(this.Min);
                    }
                    stringBuilder.Append(",dec:").Append(this.AllowDecimals ? "true" : "false");
                    switch (elementType)
                    {
                        case SettingType.Field:
                            stringBuilder.Append(",i:").Append(this.IncludeInCalculations ? "true" : "false");
                            break;
                        case SettingType.Parameter:
                            goto IL_197;
                        case SettingType.Return:
                            break;
                        default:
                            goto IL_197;
                    }
                    stringBuilder.Append(",cal:").Append(this.AllowCalculations ? "true" : "false");
                IL_197:
                    if (!string.IsNullOrWhiteSpace(this.DataSourceName))
                    {
                        stringBuilder.Append(",mds:\"").Append(this.DataSourceName).Append("\"");
                    }
                    break;
                case OperatorType.Date:
                case OperatorType.Time:
                    if (!string.IsNullOrEmpty(this.Format))
                    {
                        stringBuilder.Append(",f:\"").Append(ESPL.Rule.Core.Encoder.Sanitize(this.Format)).Append("\"");
                    }
                    break;
                case OperatorType.Enum:
                    stringBuilder.Append(",e:\"").Append(this.TypeFullName).Append("\"");
                    break;
            }
            return stringBuilder.ToString();
        }
    }
}
