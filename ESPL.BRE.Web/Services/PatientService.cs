using CodeEffects.Rule.Attributes;
using CodeEffects.Rule.Common;
using ESPL.BRE.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESPL.BRE.Web.Services
{
    public class PatientService
    {
        // Static in-rule method
        [Method("Is Today", "Indicates if the param date is today")]
        public static bool IsToday([Parameter(ValueInputType.All, Description = "The date to test")] DateTime? date)
        {
            if (date == null) return false;
            DateTime now = DateTime.Now;
            return date.Value.Day == now.Day && date.Value.Month == now.Month && date.Value.Year == now.Year;
        }

        // Instance action (void) method
        [Action("Request More Info", "Requires additional info from the patient")]
        public void RequestInfo(Patient patient, [Parameter(ValueInputType.User, Description = "Output message")] string message)
        {
            patient.Output = message;
        }
    }
}