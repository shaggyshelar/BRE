using ESPL.Rule.Common;
using ESPL.Rule.Demo.Enums;
using ESPL.Rule.Demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ESPL.Rule.Demo.Services
{
    public class DataService
    {
        public static List<SelectListItem> GetGenders(bool valueAsIndex)
        {
            List<SelectListItem> genders = new List<SelectListItem>();
            genders.Add(new SelectListItem { Selected = true, Text = Gender.Male.ToString(), Value = valueAsIndex ? "0" : Gender.Male.ToString() });
            genders.Add(new SelectListItem { Text = Gender.Female.ToString(), Value = valueAsIndex ? "1" : Gender.Female.ToString() });
            return genders;
        }

        public static List<SelectListItem> GetStates(bool valueAsIndex)
        {
            List<SelectListItem> states = new List<SelectListItem>();
            states.Add(new SelectListItem { Selected = true, Text = State.Arizona.ToString(), Value = valueAsIndex ? "0" : State.Arizona.ToString() });
            states.Add(new SelectListItem { Text = State.California.ToString(), Value = valueAsIndex ? "1" : State.California.ToString() });
            states.Add(new SelectListItem { Text = State.Florida.ToString(), Value = valueAsIndex ? "2" : State.Florida.ToString() });
            states.Add(new SelectListItem { Text = State.NorthCarolina.ToString(), Value = valueAsIndex ? "3" : State.NorthCarolina.ToString() });
            states.Add(new SelectListItem { Text = State.Georgia.ToString(), Value = valueAsIndex ? "4" : State.Georgia.ToString() });
            return states;
        }

        public static List<SelectListItem> GetEducationLevels()
        {
            List<SelectListItem> educationLevels = new List<SelectListItem>();
            educationLevels.Add(new SelectListItem { Selected = true, Text = "Master Degree", Value = "0" });
            educationLevels.Add(new SelectListItem { Text = "College", Value = "1" });
            educationLevels.Add(new SelectListItem { Text = "High School", Value = "2" });
            educationLevels.Add(new SelectListItem { Text = "Other", Value = "3" });
            return educationLevels;
        }

        public static List<SelectListItem> GetPhysicians()
        {
            int i = 0;
            List<SelectListItem> physicians = new List<SelectListItem>();
            foreach (DataSourceItem item in Physician.List())
                physicians.Add(Convert(item, ++i == 0));
            return physicians;
        }

        private static SelectListItem Convert(DataSourceItem item, bool selected)
        {
            return new SelectListItem { Text = item.Name, Value = item.ID.ToString(), Selected = selected };
        }
    }
}