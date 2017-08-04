using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    internal class Returns
    {
        public OperatorType DataType
        {
            get;
            set;
        }

        public bool Nullable
        {
            get;
            set;
        }

        public SettingHolder Settings
        {
            get;
            set;
        }

        public ValueInputType ValueInputType
        {
            get;
            set;
        }

        public Returns()
        {
            this.Nullable = false;
            this.DataType = OperatorType.None;
            this.ValueInputType = ValueInputType.All;
            this.Settings = new SettingHolder();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("o:").Append(int.Parse(Enum.Format(typeof(OperatorType), this.DataType, "D")));
            stringBuilder.Append(",ai:").Append(int.Parse(Enum.Format(typeof(ValueInputType), this.ValueInputType, "D")));
            stringBuilder.Append(",l:").Append(this.Nullable ? "true" : "false");
            stringBuilder.Append(this.Settings.ToString(SettingType.Return, this.DataType));
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}
