using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Models
{
    internal class SourceState
    {
        public int MaxLevel
        {
            get;
            set;
        }

        public bool DeclaredMembersOnly
        {
            get;
            set;
        }

        public bool Persisted
        {
            get;
            set;
        }

        public SourceState()
        {
            this.DeclaredMembersOnly = false;
            this.Persisted = true;
            this.MaxLevel = 4;
        }
    }
}
