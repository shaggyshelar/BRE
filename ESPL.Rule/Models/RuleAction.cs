using ESPL.Rule.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Models
{
    internal class RuleAction
    {
        public Element Action
        {
            get;
            set;
        }

        public List<Element> Parameters
        {
            get;
            set;
        }

        public RuleAction()
        {
            this.Parameters = new List<Element>();
        }
    }
}
