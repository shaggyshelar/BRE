using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// Allows customization of in-rule method and rule action parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class ParameterAttribute : Attribute, ISettingsAttribute
    {
        /// <summary>
        /// Gets or sets the input type that user can use to set the value of this parameter. Code Effects control always defaults to ValueInputType.Fields for IEnumerable members of the source object; any other value set for collections is ignored.
        /// </summary>
        public ValueInputType ValueInputType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of this parameter. Users can view this description
        /// by hovering the mouse over the parameter in the Rule Editor. 
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the item type of the non-generic IEnumerable parameter. Ingnored for IEnumerable&lt;T&gt; collections and non-collection types. The entire method is ignored by Code Effects control if the value of this property is not set for any of the non-generic IEnumerable parameter of the method.
        /// </summary>
        public Type CollectionItemType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum value that user can input for this parameter.
        /// Used by numeric types only; ignored by all other types.
        /// </summary>
        public long Min
        {
            get;
            set;
        }

        /// <summary>
        /// For numeric types, gets or sets the maximum value that user can input for this parameter.
        /// For string types, gets the maximum length of the string that user can input for this parameter.
        /// Ignored by all other types.
        /// </summary>
        public long Max
        {
            get;
            set;
        }

        /// <summary>
        /// For date and time types, gets or sets the .NET DateTime format string
        /// that is used by Code Effects control to display the value of this parameter in UI.
        /// Rule evaluation does not depend on this property.
        /// </summary>
        public string DateTimeFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Get or sets a constant value for this parameter. If this value is set,
        /// this parameter will be hidden from users. This constant value is used
        /// by rule evaluation.
        /// </summary>
        public string ConstantValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of the unique name of Dynamic Menu Data Source.
        /// See Code Effects control online documentation for details.
        /// </summary>
        public string DataSourceName
        {
            get;
            set;
        }

        /// <summary>
        /// Empty public c-tor
        /// </summary>
        public ParameterAttribute()
            : this(ValueInputType.All)
        {
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="valueInputType">Input type that user can use to set the value of this parameter</param>
        public ParameterAttribute(ValueInputType valueInputType)
        {
            this.Min = -9223372036854775808L;
            this.Max = 9223372036854775807L;
            this.ValueInputType = valueInputType;
        }
    }
}
