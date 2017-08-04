using ESPL.Rule.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    public class DataSourceHolder
    {
        /// <summary>
        /// Gets or sets the unique name of this data source that can be used to
        /// distinguish this delegate from all other data source methods. This is
        /// the same name used by FieldAttribute.DataSourceName,
        /// ReturnAttribute.DataSourceName and ParameterAttribute.DataSourceName properties.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets signature of a method that takes nothing and returns a list
        /// of data source items used by Dynamic Menu Data Source feature of Code Effects control.
        /// </summary>
        public GetDataSourceDelegate Delegate
        {
            get;
            set;
        }
    }
}
