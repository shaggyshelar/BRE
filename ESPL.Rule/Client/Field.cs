using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    internal class Field : Item
    {
        public OperatorType DataType
        {
            get;
            set;
        }

        public ValueInputType ValueInputType
        {
            get;
            set;
        }

        public bool IsRule
        {
            get;
            set;
        }

        public bool Settable
        {
            get;
            set;
        }

        public bool Gettable
        {
            get;
            set;
        }

        public CollectionHolder Collection
        {
            get;
            set;
        }

        public SettingHolder Settings
        {
            get;
            set;
        }

        public Field()
        {
            this.DataType = OperatorType.None;
            this.ValueInputType = ValueInputType.All;
            this.IsRule = false;
            this.Settable = (this.Gettable = true);
            this.Collection = new CollectionHolder();
            this.Settings = new SettingHolder();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("n:\"").Append(ESPL.Rule.Core.Encoder.Sanitize(base.Name)).Append("\"");
            stringBuilder.Append(",v:\"").Append(base.Value).Append("\"");
            if (!string.IsNullOrWhiteSpace(base.Description))
            {
                stringBuilder.Append(",d:\"").Append(ESPL.Rule.Core.Encoder.Sanitize(base.Description)).Append("\"");
            }
            stringBuilder.Append(",o:").Append(int.Parse(Enum.Format(typeof(OperatorType), this.DataType, "D")));
            stringBuilder.Append(",t:").Append(int.Parse(Enum.Format(typeof(ElementType), base.Type, "D")));
            stringBuilder.Append(",ai:").Append(int.Parse(Enum.Format(typeof(ValueInputType), this.ValueInputType, "D")));
            if (this.IsRule)
            {
                stringBuilder.Append(",ir:true");
            }
            if (!this.Settable)
            {
                stringBuilder.Append(",st:false");
            }
            if (!this.Gettable)
            {
                stringBuilder.Append(",gtb:false");
            }
            if (this.DataType == OperatorType.Collection)
            {
                stringBuilder.Append(this.Collection.ToString(new ElementType?(base.Type), SettingType.Field));
            }
            if (base.Type == ElementType.Field)
            {
                stringBuilder.Append(this.Settings.ToString(SettingType.Field, (this.DataType == OperatorType.Collection) ? this.Collection.DataType : this.DataType));
            }
            if (base.IncludeNullableInJson)
            {
                stringBuilder.Append(",l:").Append(base.Nullable ? "true" : "false");
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}
