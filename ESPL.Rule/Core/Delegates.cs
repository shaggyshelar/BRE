using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// Declares a signature of a method that takes nothing and returns a list
    /// of data source items used by Dynamic Menu Data Source feature of Code Effects control.
    /// See Code Effects control online documentation for details.
    /// </summary>
    public delegate List<DataSourceItem> GetDataSourceDelegate();

    internal delegate XElement GetRuleInternalDelegate(string ruleId);

    /// <summary>
    /// Declares a signature of a method that takes rule ID and returns Rule XML as a string.
    /// </summary>
    public delegate string GetRuleDelegate(string ruleId);
}
