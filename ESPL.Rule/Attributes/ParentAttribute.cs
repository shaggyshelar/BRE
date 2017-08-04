using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// Used to distinguish between multiple fields of the same reference type declared by the source object.
    /// See Code Effects control online documentation for details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public sealed class ParentAttribute : Attribute, IDescribableAttribute, IDisplayableAttribute
    {
        /// <summary>
        /// Gets the declared property name
        /// </summary>
        public string ParentName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the label that represents this type in UI
        /// </summary>
        public string DisplayName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the description of this type used in UI
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        public ParentAttribute(string parentName, string displayName)
        {
            this.ParentName = parentName;
            this.DisplayName = displayName;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        public ParentAttribute(string parentName, string displayName, string description)
            : this(parentName, displayName)
        {
            this.Description = description;
        }
    }
}
