using ESPL.Rule.Attributes;
using ESPL.Rule.Common;
using ESPL.Rule.Demo.Enums;
using ESPL.Rule.Demo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESPL.Rule.Demo.Models
{
    // External methods and actions
    [ExternalMethod(typeof(PatientService), "IsToday")]
    [ExternalAction(typeof(PatientService), "RequestInfo")]

    // Dynamic Menu Data Sources; details can be found at
    // http://codeeffects.com/Doc/Business-Rules-Dynamic-Menu-Data-Sources

    // The getEducationTypes() client-side function declared in /Views/Shared/_Layout.cshtml
    [Data("Education", "getEducationTypes")]
    // The List() method declared by the Physician class
    [Data("Physicians", typeof(Physician), "List")]
    public class Patient
    {
        // C-tor
        public Patient()
        {
            this.ID = Guid.Empty;
            this.Gender = Gender.Unknown;
        }

        // This property will not appear in the Rule Editor - Code Effects component ignores Guids.
        // Details at http://codeeffects.com/Doc/Business-Rules-Data-Types
        public Guid ID { get; set; }

        [Field(DisplayName = "First Name", Description = "Patient's first name", Max = 30)]
        public string FirstName { get; set; }

        [Field(DisplayName = "Last Name", Max = 30, Description = "Patient's last name")]
        public string LastName { get; set; }

        [Field(DisplayName = "Email Address", ValueInputType = ValueInputType.User, Max = 150, Description = "Email address of the patient")]
        public string Email { get; set; }

        [Field(DisplayName = "Date of Birth", DateTimeFormat = "MMM dd, yyyy")]
        public DateTime? DOB { get; set; }

        [Field(ValueInputType = ValueInputType.User, Description = "Patient's gender")]
        public Gender Gender { get; set; }

        // This field uses the "Physicians" dynamic menu source (declared at class level)
        [Field(DisplayName = "Physician", DataSourceName = "Physicians", Description = "Patient's primary physician")]
        public int PhysicianID { get; set; }

        // This field uses the "Education" client-side dynamic menu source (declared at class level)
        [Field(DisplayName = "Education", DataSourceName = "Education", Description = "Patient's education level")]
        public int EducationTypeID { get; set; }

        [Field(Min = 0, Max = 200, Description = "Current pulse")]
        public int? Pulse { get; set; }

        [Field(Min = 0, Max = 200, DisplayName = "Systolic Pressure", Description = "Current systolic pressure")]
        public int? SystolicPressure { get; set; }

        [Field(Min = 0, Max = 200, DisplayName = "Diastolic Pressure", Description = "Current Diastolic pressure")]
        public int? DiastolicPressure { get; set; }

        [Field(Min = 0, Max = 110, Description = "Current temperature")]
        public decimal? Temperature { get; set; }

        [Field(DisplayName = "Headaches Box", Description = "Does the patient have frequent headaches?")]
        public bool Headaches { get; set; }

        [Field(DisplayName = "Allergies Box", Description = "Any allergies?")]
        public bool Allergies { get; set; }

        [Field(DisplayName = "Tobacco Box", Description = "Does the patient smoke?")]
        public bool Tobacco { get; set; }

        [Field(DisplayName = "Alcohol Box", Description = "Alcohol use?")]
        public bool Alcohol { get; set; }

        public Address Home { get; set; }
        public Address Work { get; set; }

        // This property is used to display outputs of rule actions
        [ExcludeFromEvaluation]
        public string Output { get; set; }

        [Method("Full Name", "Joins together patient's first and last names")]
        public string FullName()
        {
            return string.Format("{0} {1}", this.FirstName, this.LastName);
        }

        // Empty overload of the Register method.
        // No Method attribute is needed here because its
        // display name is the same as its declared name.
        [Action(Description = "Registers new patient")]
        public void Register()
        {
            this.Output = "The patient has been registered";
        }

        // Overload of the Register method that takes one param.
        // Both overloads can be used in Code Effects as two different actions
        // as long as their display names are different.
        [Action("Register with a Message", "Registers new patient with additional info")]
        public void Register([Parameter(ValueInputType.User, Description = "Output message")] string message)
        {
            this.Output = message;
        }
    }
}