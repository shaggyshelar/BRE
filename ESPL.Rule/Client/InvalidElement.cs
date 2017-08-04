using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Client
{
    public class InvalidElement
    {
        public string ClientID
        {
            get;
            set;
        }

        public string Hint
        {
            get;
            set;
        }

        public InvalidElement()
        {
        }

        public InvalidElement(string clientID, string hint)
        {
            this.ClientID = clientID;
            this.Hint = hint;
        }

        public override string ToString()
        {
            return new StringBuilder().Append("{c:\"").Append(this.ClientID).Append("\",h:\"").Append(ESPL.Rule.Core.Encoder.Sanitize(this.Hint)).Append("\"}").ToString();
        }
    }
}
