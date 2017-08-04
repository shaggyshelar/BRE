using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    internal class SourceHolder
    {
        public string Name
        {
            get;
            set;
        }

        public List<Item> Fields
        {
            get;
            set;
        }

        public List<Function> Actions
        {
            get;
            set;
        }

        public SourceHolder()
        {
            this.Actions = new List<Function>();
            this.Fields = new List<Item>();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("n:\"").Append(ESPL.Rule.Core.Encoder.GetHashToken(this.Name)).Append("\",");
            bool flag = true;
            if (this.Actions.Count > 0)
            {
                stringBuilder.Append("acs:[");
                foreach (Function current in this.Actions)
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
                flag = true;
                stringBuilder.Append("],");
            }
            stringBuilder.Append("fds:[");
            foreach (Item current2 in this.Fields)
            {
                if (!flag)
                {
                    stringBuilder.Append(",");
                }
                else
                {
                    flag = false;
                }
                ElementType type = current2.Type;
                if (type == ElementType.Function)
                {
                    stringBuilder.Append(((Function)current2).ToString());
                }
                else
                {
                    stringBuilder.Append(((Field)current2).ToString());
                }
            }
            stringBuilder.Append("]}");
            return stringBuilder.ToString();
        }
    }
}
