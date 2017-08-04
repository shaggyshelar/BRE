using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    internal class EnumHolder
    {
        public string Ns
        {
            get;
            set;
        }

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

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("Ns:\"").Append(this.Ns).Append("\",");
            stringBuilder.Append("Name:\"").Append(this.Name).Append("\",");
            stringBuilder.Append("Data:").Append(this.Data).Append("}");
            return stringBuilder.ToString();
        }
    }
}
