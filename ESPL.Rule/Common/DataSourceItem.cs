using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// Holds data of a single Dynamic Menu Data Source item.
    /// See ESPL control online documentation for details.
    /// </summary>
    public class DataSourceItem
    {
        /// <summary>
        /// Gets or sets the ID of the item. This ID will be stored
        /// in rules and used to locate proper value during rule evaluation.
        /// </summary>
        public int ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the display name of the item. This name will appear in
        /// context menus and as a value element when user selects this item from a menu.
        /// Rule evaluation does not rely on this property.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Public empty constructor. Sets the value of ID property to 0 (zero).
        /// </summary>
        public DataSourceItem()
        {
            this.ID = 0;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="id">ID of the item</param>
        /// <param name="name">Display name of the item</param>
        public DataSourceItem(int id, string name)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}
