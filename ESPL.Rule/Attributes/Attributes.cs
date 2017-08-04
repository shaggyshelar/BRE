using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// Allows customization of rule actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ActionAttribute : Attribute, IDescribableAttribute, IDisplayableAttribute
    {
        /// <summary>
        /// Gets or sets the label that will be used to represent this action on UI.
        /// If not set, the declared name of the method will be used.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of this action. Users can view this description when
        /// they hover the mouse over the action in the Rule Editor. 
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Empty public c-tor.
        /// </summary>
        public ActionAttribute()
        {
        }

        /// <summary>
        /// Public c-tor.
        /// </summary>
        /// <param name="displayName">The label that will be used to represent this action on UI.</param>
        public ActionAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }

        /// <summary>
        /// Public c-tor.
        /// </summary>
        /// <param name="displayName">The label that will be used to represent this action on UI.</param>
        /// <param name="description">Description of this action.</param>
        public ActionAttribute(string displayName, string description)
            : this(displayName)
        {
            this.Description = description;
        }
    }
}
