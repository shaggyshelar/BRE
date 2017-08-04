using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// Usually used to represent a single reusable rule in Tool Bar menus
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Gets or sets the ID of the menu item.
        /// </summary>
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the display name of the menu item.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the source object used to generate this rule.
        /// </summary>
        public string SourceName
        {
            get;
            set;
        }

        /// <summary>
        /// Optional. Gets or sets the description of the menu item.
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        public MenuItem()
        {
            this.ID = Guid.Empty.ToString();
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="id">Sets the ID property of this instance of MenuItem class</param>
        /// <param name="displayName">Sets the DisplayName property of this instance of MenuItem class</param>
        public MenuItem(string id, string displayName)
        {
            this.ID = id;
            this.DisplayName = displayName;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="id">Sets the ID property of this instance of MenuItem class</param>
        /// <param name="displayName">Sets the DisplayName property of this instance of MenuItem class</param>
        /// <param name="description">Sets the Description property of this instance of MenuItem class</param>
        public MenuItem(string id, string displayName, string description)
            : this(id, displayName)
        {
            this.Description = description;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="id">Sets the ID property of this instance of MenuItem class</param>
        /// <param name="displayName">Sets the DisplayName property of this instance of MenuItem class</param>
        /// <param name="description">Sets the Description property of this instance of MenuItem class</param>
        /// <param name="sourceName">Sets the SourceName property of this instance of MenuItem class</param>
        public MenuItem(string id, string displayName, string description, string sourceName)
            : this(id, displayName)
        {
            this.SourceName = sourceName;
            this.Description = description;
        }

        /// <summary>
        /// This method is not intended for public use
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            stringBuilder.Append("\"v\":\"").Append(ESPL.Rule.Core.Encoder.Sanitize(this.ID));
            stringBuilder.Append("\",\"n\":\"").Append(ESPL.Rule.Core.Encoder.Sanitize(this.DisplayName)).Append("\"}");
            return stringBuilder.ToString();
        }
    }
}
