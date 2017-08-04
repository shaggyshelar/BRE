using ESPL.Rule.Common;
using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    internal class Function : Item
    {
        public List<Parameter> Parameters
        {
            get;
            set;
        }

        public Returns Returns
        {
            get;
            set;
        }

        public bool IncludeInCalculations
        {
            get;
            set;
        }

        public bool Gettable
        {
            get;
            set;
        }

        internal bool IncludeReturnInJson
        {
            get;
            set;
        }

        public Function()
        {
            this.Parameters = new List<Parameter>();
            this.Returns = new Returns();
            this.IncludeInCalculations = (this.Gettable = true);
            this.IncludeReturnInJson = false;
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
            if (this.Returns.DataType == OperatorType.Numeric)
            {
                stringBuilder.Append(",i:").Append(this.IncludeInCalculations ? "true" : "false");
            }
            if (!this.Gettable)
            {
                stringBuilder.Append(",gtb:false");
            }
            stringBuilder.Append(",t:").Append(int.Parse(Enum.Format(typeof(ElementType), base.Type, "D"))).Append(",ps:[");
            if (this.Parameters.Count > 0)
            {
                bool flag = true;
                foreach (Parameter current in this.Parameters)
                {
                    if (!flag)
                    {
                        stringBuilder.Append(",");
                    }
                    else
                    {
                        flag = false;
                    }
                    stringBuilder.Append(current.ToString());
                }
            }
            stringBuilder.Append("]");
            if (this.IncludeReturnInJson)
            {
                stringBuilder.Append(",rt:").Append(this.Returns.ToString());
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}
