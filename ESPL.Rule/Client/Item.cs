using ESPL.Rule.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    internal class Item
    {
        public string Name
        {
            get;
            set;
        }

        public string Value
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

        public ElementType Type
        {
            get;
            set;
        }

        internal bool IncludeNullableInJson
        {
            get;
            set;
        }

        public Item()
        {
            this.Nullable = true;
            this.Type = ElementType.Flow;
            this.IncludeNullableInJson = false;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("n:\"").Append(ESPL.Rule.Core.Encoder.Sanitize(this.Name)).Append("\"");
            stringBuilder.Append(",v:\"").Append(this.Value).Append("\"");
            stringBuilder.Append(",t:").Append(int.Parse(Enum.Format(typeof(ElementType), this.Type, "D")));
            if (this.IncludeNullableInJson)
            {
                stringBuilder.Append(",l:").Append(this.Nullable ? "true" : "false");
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}
