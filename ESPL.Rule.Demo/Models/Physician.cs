using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESPL.Rule.Demo.Models
{
    public class Physician
    {
        /// <summary>
        /// Returns a list of fictitious physicians
        /// </summary>
        /// <returns>List of data source items</returns>
        public static List<ESPL.Rule.Common.DataSourceItem> List()
        {
            List<ESPL.Rule.Common.DataSourceItem> people = new List<ESPL.Rule.Common.DataSourceItem>();

            people.Add(new ESPL.Rule.Common.DataSourceItem(0, "John Smith"));
            people.Add(new ESPL.Rule.Common.DataSourceItem(1, "Anna Taylor"));
            people.Add(new ESPL.Rule.Common.DataSourceItem(2, "Robert Brown"));
            people.Add(new ESPL.Rule.Common.DataSourceItem(3, "Stephen Lee"));
            people.Add(new ESPL.Rule.Common.DataSourceItem(4, "Joe Wilson"));
            people.Add(new ESPL.Rule.Common.DataSourceItem(5, "Samuel Thompson"));

            return people;
        }
    }
}