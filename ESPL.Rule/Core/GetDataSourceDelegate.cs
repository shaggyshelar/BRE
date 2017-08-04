using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// Declares a signature of a method that takes nothing and returns a list
    /// of data source items used by Dynamic Menu Data Source feature of Code Effects control.
    /// See Code Effects control online documentation for details.
    /// </summary>
    public delegate List<DataSourceItem> GetDataSourceDelegate();
}
