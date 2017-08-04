using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// Allows customization of in-rule method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class MethodAttribute : Attribute, IDescribableAttribute, IDisplayableAttribute
    {
        /// <summary>
        /// Gets or sets the label that will be used to represent this method on UI.
        /// If not set, the declared name of the method will be used.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of this method. Users can view this description
        /// by hovering the mouse over the method in the Rule Editor. 
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// For methods with numeric returns, gets or sets the value indicating whether this
        /// method can be included in value calculations of other fields and methods.
        /// Ignored by methods that return non-numeric types.
        /// </summary>
        public bool IncludeInCalculations
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating whether the value of this
        /// method appears in UI context menus that list available rule
        /// fields and in-rule methods for rule conditions.
        /// The default value is True
        /// </summary>
        public bool Gettable
        {
            get;
            set;
        }

        /// <summary>
        /// Empty public c-tor.
        /// </summary>
        public MethodAttribute()
        {
            this.Gettable = true;
            this.IncludeInCalculations = true;
        }

        /// <summary>
        /// Public c-tor.
        /// </summary>
        /// <param name="displayName">The label that will be used to represent this method on UI.</param>
        public MethodAttribute(string displayName)
            : this()
        {
            this.DisplayName = displayName;
        }

        /// <summary>
        /// Public c-tor.
        /// </summary>
        /// <param name="displayName">The label that will be used to represent this method on UI.</param>
        /// <param name="description">Description of this method.</param>
        public MethodAttribute(string displayName, string description)
            : this(displayName)
        {
            this.Description = description;
        }

        /// <summary>
        /// Public c-tor.
        /// </summary>
        /// <param name="includeInCalculations">Indicates whether this method should be included in numeric value calculations</param>
        public MethodAttribute(bool includeInCalculations)
        {
            this.IncludeInCalculations = includeInCalculations;
        }

        /// <summary>
        /// Public c-tor.
        /// </summary>
        /// <param name="displayName">The label that will be used to represent this method on UI.</param>
        /// <param name="includeInCalculations">Indicates whether this method should be included in numeric value calculations</param>
        public MethodAttribute(string displayName, bool includeInCalculations)
            : this(displayName)
        {
            this.IncludeInCalculations = includeInCalculations;
        }

        /// <summary>
        /// Public c-tor.
        /// </summary>
        /// <param name="displayName">The label that will be used to represent this method on UI.</param>
        /// <param name="description">Description of this method.</param>
        /// <param name="includeInCalculations">Indicates whether this method should be included in numeric value calculations</param>
        public MethodAttribute(string displayName, string description, bool includeInCalculations)
            : this(displayName, description)
        {
            this.IncludeInCalculations = includeInCalculations;
        }
    }
}
