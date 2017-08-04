using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// Allows customization of in-rule method's return type
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    public sealed class ReturnAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the input type that user can use to set the value of this return type
        /// </summary>
        public ValueInputType ValueInputType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum value that user can input for this return type.
        /// Used by numeric types only; ignored by all other types.
        /// </summary>
        public long Min
        {
            get;
            set;
        }

        /// <summary>
        /// For numeric types, gets or sets the maximum value that user can input for this return type.
        /// For string types, gets the maximum length of the string that user can input for this return type.
        /// Ignored by all other types.
        /// </summary>
        public long Max
        {
            get;
            set;
        }

        /// <summary>
        /// For date and time types, gets or sets the .NET DateTime format string
        /// that is used by Code Effects control to display the value of this return type in UI.
        /// Rule evaluation does not depend on this property.
        /// </summary>
        public string DateTimeFormat
        {
            get;
            set;
        }

        /// <summary>
        /// For numeric types, gets or sets the value indicating whether this
        /// return type allows user to enter calculations as its value.
        /// Ignored by all other types.
        /// </summary>
        public bool AllowCalculations
        {
            get;
            set;
        }

        /// <summary>
        /// For string types, gets or sets the string comparison type for this return type.
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
        /// Empty public c-tor
        /// </summary>
        public ReturnAttribute()
        {
            this.ValueInputType = ValueInputType.All;
            this.Min = -9223372036854775808L;
            this.Max = 9223372036854775807L;
            this.AllowCalculations = true;
            this.StringComparison = StringComparison.OrdinalIgnoreCase;
        }
    }
}
