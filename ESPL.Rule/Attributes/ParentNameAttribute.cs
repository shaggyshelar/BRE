using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ParentNameAttribute : Attribute
    {
        public string ParentName
        {
            get;
            set;
        }

        public ParentNameAttribute(string parentName)
        {
            this.ParentName = parentName;
        }
    }
}
