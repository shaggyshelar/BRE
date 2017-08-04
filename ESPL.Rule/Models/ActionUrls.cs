using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Models
{
    internal sealed class ActionUrls
    {
        internal string SaveAction
        {
            get;
            set;
        }

        internal string DeleteAction
        {
            get;
            set;
        }

        internal string LoadAction
        {
            get;
            set;
        }

        internal ActionUrls()
        {
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("\"s\":");
            if (string.IsNullOrEmpty(this.SaveAction))
            {
                stringBuilder.Append("null");
            }
            else
            {
                stringBuilder.Append("\"").Append(this.SaveAction).Append("\"");
            }
            stringBuilder.Append(",\"d\":");
            if (string.IsNullOrEmpty(this.DeleteAction))
            {
                stringBuilder.Append("null");
            }
            else
            {
                stringBuilder.Append("\"").Append(this.DeleteAction).Append("\"");
            }
            stringBuilder.Append(",\"l\":");
            if (string.IsNullOrEmpty(this.LoadAction))
            {
                stringBuilder.Append("null");
            }
            else
            {
                stringBuilder.Append("\"").Append(this.LoadAction).Append("\"");
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}
