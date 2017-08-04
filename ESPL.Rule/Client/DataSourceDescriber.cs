using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    internal class DataSourceDescriber
    {
        public string Name
        {
            get;
            set;
        }

        public string Data
        {
            get;
            set;
        }

        public bool Client
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("\"Client\":").Append(this.Client ? "true" : "false").Append(",");
            stringBuilder.Append("\"Name\":\"").Append(this.Name).Append("\",");
            stringBuilder.Append("\"Data\":").Append(this.Data).Append("}");
            return stringBuilder.ToString();
        }
    }
}
