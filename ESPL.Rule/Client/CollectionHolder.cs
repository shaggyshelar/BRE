using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    internal class CollectionHolder
    {
        public CollectionType Type
        {
            get;
            set;
        }

        public bool IsArray
        {
            get;
            set;
        }

        public bool IsGeneric
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public string ComparisonName
        {
            get;
            set;
        }

        public string UnderlyingTypeFullName
        {
            get;
            set;
        }

        public OperatorType DataType
        {
            get;
            set;
        }

        public bool IsUnderlyingTypeNullable
        {
            get;
            set;
        }

        public CollectionHolder()
        {
            this.IsUnderlyingTypeNullable = true;
            this.IsArray = (this.IsGeneric = false);
            this.Type = CollectionType.None;
        }

        public override string ToString()
        {
            throw new NotImplementedException("Use the other public overload");
        }

        public string ToString(ElementType? type, SettingType settingType)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(",ct:").Append(int.Parse(Enum.Format(typeof(CollectionType), this.Type, "D")));
            if (this.Type == CollectionType.Value)
            {
                stringBuilder.Append(",co:").Append(int.Parse(Enum.Format(typeof(OperatorType), this.DataType, "D")));
            }
            if (!type.HasValue || type == ElementType.Field)
            {
                stringBuilder.Append(",cg:").Append(this.IsGeneric ? "true" : "false");
                stringBuilder.Append(",cr:").Append(this.IsArray ? "true" : "false");
                stringBuilder.Append(",cc:\"").Append(ESPL.Rule.Core.Encoder.GetHashToken(this.ComparisonName)).Append("\"");
                if (this.Type == CollectionType.Reference && settingType == SettingType.Field)
                {
                    stringBuilder.Append(",cn:\"").Append(ESPL.Rule.Core.Encoder.Sanitize(this.DisplayName)).Append("\"");
                }
                if (this.Type != CollectionType.Generic)
                {
                    stringBuilder.Append(",cl:").Append(this.IsUnderlyingTypeNullable ? "true" : "false");
                    stringBuilder.Append(",cv:\"").Append(ESPL.Rule.Core.Encoder.GetHashToken(this.UnderlyingTypeFullName)).Append("\"");
                }
            }
            return stringBuilder.ToString();
        }
    }
}
