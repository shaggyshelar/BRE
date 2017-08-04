using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Models
{
    internal class DataSource
    {
        internal string Name
        {
            get;
            set;
        }

        internal FeatureLocation Location
        {
            get;
            set;
        }

        internal string Assembly
        {
            get;
            set;
        }

        internal string Class
        {
            get;
            set;
        }

        internal string Method
        {
            get;
            set;
        }

        public DataSource()
        {
            this.Location = FeatureLocation.Server;
        }
    }
}
