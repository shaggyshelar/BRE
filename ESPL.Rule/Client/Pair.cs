using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    public sealed class Pair
    {
        public string ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public Pair()
        {
        }

        public Pair(string id, string name)
        {
            this.ID = id;
            this.Name = name;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("ID:\"").Append(this.ID);
            stringBuilder.Append("\",Name:\"").Append(this.Name).Append("\"}");
            return stringBuilder.ToString();
        }
    }
}
