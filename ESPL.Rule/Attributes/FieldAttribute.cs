using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// Allows customization of rule field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class FieldAttribute : Attribute, IDescribableAttribute, IDisplayableAttribute, ISettingsAttribute
    {
        /// <summary>
        /// Gets or sets the input type that user can use to set the value of this field
        /// </summary>
        public ValueInputType ValueInputType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the label that will be used in UI to represent this field. Default value is the name of this field.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the singular label for IEnumerable&lt;T&gt; property or field. For example, for collection List&lt;Order&gt; named Orders this value could be set to Order. Setting this value helps to display collection fields in a human-like language in cetain menus and other UI elements. Ingnored for fields of non-generic collections and non-collection types. Default value is the value of the DisplayName property.
        /// </summary>
        public string CollectionItemName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the item type of non-generic IEnumerable collection property. Ingnored for IEnumerable&lt;T&gt; collections and non-collection properties. The non-generic IEnumerable property is ignored by Code Effects control if this value is not set.
        /// </summary>
        public Type CollectionItemType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of this field. Users can view this description
        /// by hovering the mouse over the field in the Rule Editor. 
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum value that user can input for this field.
        /// Used by numeric types only; ignored by all other types
        /// </summary>
        public long Min
        {
            get;
            set;
        }

        /// <summary>
        /// For numeric types, gets or sets the maximum value that user can input for this field.
        /// For string types, gets the maximum length of the string that user can input for this field.
        /// Ignored by all other types.
        /// </summary>
        public long Max
        {
            get;
            set;
        }

        /// <summary>
        /// For date and time types, gets or sets the .NET DateTime format string
        /// that is used by Code Effects control to display the value of this field in UI.
        /// Rule evaluation does not depend on this property.
        /// </summary>
        public string DateTimeFormat
        {
            get;
            set;
        }

        /// <summary>
        /// For numeric types, gets or sets the value indicating whether this
        /// field allows user to enter calculations as its value.
        /// Ignored by all other types.
        /// </summary>
        public bool AllowCalculations
        {
            get;
            set;
        }

        /// <summary>
        /// For numeric types, gets or sets the value indicating whether this
        /// field can be included in value calculations of other fields.
        /// Ignored by all other types.
        /// </summary>
        public bool IncludeInCalculations
        {
            get;
            set;
        }

        /// <summary>
        /// For string types, gets or sets the string comparison type for this field.
        /// The default value is StringComparison.OrdinalIgnoreCase.
        /// Ignored by all other types.
        /// </summary>
        public StringComparison StringComparison
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
        /// Gets or sets the value indicating whether the value of this
        /// field appears in UI context menus that list available rule
        /// fields and in-rule methods for rule conditions.
        /// The default value is True
        /// </summary>
        public bool Gettable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating whether the value of this
        /// field can be set in execution type rules. The default value is
        /// False for const or readonly fields and properties that have a private
        /// setter. It's True for all other fields and properties.
        /// </summary>
        public bool Settable
        {
            get;
            set;
        }

        /// <summary>
        /// Empty public c-tor
        /// </summary>
        public FieldAttribute()
        {
            this.ValueInputType = ValueInputType.All;
            this.Min = -9223372036854775808L;
            this.Max = 9223372036854775807L;
            this.AllowCalculations = (this.IncludeInCalculations = (this.Settable = (this.Gettable = true)));
            this.StringComparison = StringComparison.OrdinalIgnoreCase;
        }
    }
}
