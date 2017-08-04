using ESPL.Rule.Client;
using ESPL.Rule.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Models
{
    internal class RuleCondition
    {
        public Element Field
        {
            get;
            set;
        }

        public List<Element> Parameters
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

        public Element Operator
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

        public bool Valid
        {
            get
            {
                return this.Field != null && this.Operator != null && (RuleValidator.IsNullableOperator(this.Operator.Value) || (this.Value != null && (this.Value.Type == ElementType.Value || (this.Value.Type == ElementType.Function && (this.ValueParametersCount == 0 || this.ValueParameters.Count == this.ValueParametersCount)))) || this.Calculation != null);
            }
        }

        public RuleCondition()
        {
            this.ValueParametersCount = 0;
            this.Parameters = new List<Element>();
            this.ValueParameters = new List<Element>();
        }
    }
}
