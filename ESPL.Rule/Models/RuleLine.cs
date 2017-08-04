using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Models
{
    internal class RuleLine
    {
        public int Index
        {
            get;
            set;
        }

        public int Tabs
        {
            get;
            set;
        }

        public bool Completed
        {
            get;
            set;
        }

        public RuleLine(int index)
        {
            this.Index = index;
            this.Tabs = 0;
            this.Completed = false;
        }
    }
}
