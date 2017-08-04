using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// Allows customization of source object
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class SourceAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the maximum number of levels up to which Code Effects control performs
        /// the recursive search for value type members declared by the source object.
        /// The default value is 4.
        /// </summary>
        public int MaxTypeNestingLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating if Code Effects control should ignore
        /// properties and fields it inherits from its base types.
        /// The default value is False.
        /// </summary>
        public bool DeclaredMembersOnly
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating if Code Effects control should set the
        /// type name of the source object in rule XML. The default value is True.
        /// </summary>
        public bool PersistTypeNameInRuleXml
        {
            get;
            set;
        }

        /// <summary>
        /// Empty public c-tor
        /// </summary>
        public SourceAttribute()
        {
            this.DeclaredMembersOnly = false;
            this.PersistTypeNameInRuleXml = true;
            this.MaxTypeNestingLevel = 4;
        }
    }
}
