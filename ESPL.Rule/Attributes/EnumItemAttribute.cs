using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// Used to define the UI label of enum member
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class EnumItemAttribute : Attribute, IDisplayableAttribute
    {
        /// <summary>
        /// Gets or sets the label that will be used to represent this enum member on UI.
        /// If not set, the declared name of the member will be used.
        /// </summary>
        public string DisplayName
        {
            get;
            private set;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="displayName">Label that will be used to represent this enum member on UI.</param>
        public EnumItemAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }
    }
}
