using ESPL.Rule.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Models
{
    internal class RuleSetter
    {
        public Element Field
        {
            get;
            set;
        }

        public Element Value
        {
            get;
            set;
        }

        public XmlElement Calculation
        {
            get;
            set;
        }

        public List<Element> ValueParameters
        {
            get;
            set;
        }

        public int ValueParametersCount
        {
            get;
            set;
        }

        public bool Valid
        {
            get
            {
                return this.Field != null && ((this.Value != null && (this.Value.Type == ElementType.Value || (this.Value.Type == ElementType.Function && (this.ValueParametersCount == 0 || this.ValueParameters.Count == this.ValueParametersCount)))) || this.Calculation != null);
            }
        }

        public RuleSetter()
        {
            this.ValueParameters = new List<Element>();
            this.ValueParametersCount = 0;
        }
    }
}
