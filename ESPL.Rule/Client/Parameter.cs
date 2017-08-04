using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    internal class Parameter
    {
        public ParameterType Type
        {
            get;
            set;
        }

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

        public SettingHolder Settings
        {
            get;
            set;
        }

        public CollectionHolder Collection
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public bool Nullable
        {
            get;
            set;
        }

        public Parameter()
        {
            this.Type = ParameterType.None;
            this.Nullable = false;
            this.DataType = OperatorType.None;
            this.ValueInputType = ValueInputType.All;
            this.Collection = new CollectionHolder();
            this.Settings = new SettingHolder();
        }

        public override string ToString()
        {
            if (this.Type != ParameterType.Input)
            {
                return string.Empty;
            }
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("ai:").Append(int.Parse(Enum.Format(typeof(ValueInputType), this.ValueInputType, "D")));
            stringBuilder.Append(",o:").Append(int.Parse(Enum.Format(typeof(OperatorType), this.DataType, "D")));
            if (!string.IsNullOrWhiteSpace(this.Description))
            {
                stringBuilder.Append(",d:\"").Append(ESPL.Rule.Core.Encoder.Sanitize(this.Description)).Append("\"");
            }
            stringBuilder.Append(",l:").Append(this.Nullable ? "true" : "false");
            if (this.DataType == OperatorType.Collection)
            {
                stringBuilder.Append(this.Collection.ToString(null, SettingType.Parameter));
            }
            stringBuilder.Append(this.Settings.ToString(SettingType.Parameter, (this.DataType == OperatorType.Collection) ? this.Collection.DataType : this.DataType));
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}
