using ESPL.Rule.Core;
using ESPL.Rule.Models;
using ESPL.Rule.Demo.Models;
using ESPL.Rule.Demo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ESPL.Rule.MVC;

namespace ESPL.Rule.Demo.Controllers
{
    public class AjaxController : Controller
    {
        public AjaxController()
        {
            // Storing data for the Gender drop down declared in the /Views/Shared/_PatientForm.cshtml shared view
            ViewBag.Genders = DataService.GetGenders(true);

            // Storing data for the States drop downs
            ViewBag.States = DataService.GetStates(true);

            // Storing data for the EducationLevels drop down
            ViewBag.EducationLevels = DataService.GetEducationLevels();

            // Storing data for the Physicians drop downs
            ViewBag.Physicians = DataService.GetPhysicians();
        }

        public ActionResult Index()
        {
            // Create a new rule model and store it in the bag
            // The view passes it to the RuleEditor
            ViewBag.Rule = RuleModel.Create(typeof(Patient));

            return View();
        }


        [HttpGet]
        public ActionResult Test()
        {
            Patient patient = new Patient();
            patient.FirstName = "sagar";

            string ruleData = "{\"Elements\":[{\"Type\":0,\"Name\":\"_ruleEditor6\",\"Oper\":16,\"FuncType\":4,\"InpType\":2,\"Value\":\"if\"},{\"Type\":1,\"Name\":\"_ruleEditor7\",\"Oper\":0,\"FuncType\":4,\"InpType\":2,\"Value\":\"FirstName\",\"IsRule\":false,\"Max\":30},{\"Type\":3,\"Name\":\"_ruleEditor8\",\"Oper\":0,\"FuncType\":4,\"InpType\":2,\"Value\":\"equal\"},{\"Type\":4,\"Name\":\"_ruleEditor9\",\"Oper\":0,\"FuncType\":4,\"InpType\":1,\"Value\":\"sagar\"}],\"Id\":\"6d8c9450-fd17-4d22-811c-82b7edf4e245\",\"IsLoadedRuleOfEvalType\":true,\"Command\":\"ceExtract\",\"Mode\":0,\"Name\":\"first\",\"Desc\":\"first rule\",\"SkipNameValidation\":false}";
            Result result = new Result();

            // See the comments in the LoadSettings() method
            RuleEditor editor = this.GetRuleEditor();

            // Load the rule into the editor
            editor.LoadClientData(ruleData);

            // We are not saving the rule, just evaluating it. Tell the editor not to enforce the rule name validation
            editor.Rule.SkipNameValidation = true;

            if (editor.Rule.IsEmpty())
            {
                result.IsRuleEmpty = true;
            }
            else if (!editor.Rule.IsValid(StorageService.LoadRuleXml))
            {
                result.IsRuleValid = false;
                // Load the json string of invalid data into the Result object
                result.ClientInvalidData = editor.GetClientInvalidData();
            }
            else
            {
                // Create an instance of the Evaluator class. Because our rules might reference other rules of evaluation type
                // we use constructor that takes rule's XML and delegate of the method that can load referenced rules by their IDs.
                Evaluator<Patient> evaluator = new Evaluator<Patient>(editor.Rule.GetRuleXml(), StorageService.LoadRuleXml);

                // Evaluate the patient against the rule
                bool success = evaluator.Evaluate(patient);

                // Return the evaluated patient back to the client
                result.Patient = patient;

                // Output the result of the evaluation to the client
                result.Output = string.IsNullOrWhiteSpace(patient.Output) ? "The rule evaluated to " + success.ToString() : patient.Output;
            }

            return Json(result, JsonRequestBehavior.AllowGet);

            //string settings = "Hello World";
            //return Json(settings, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadSettings()
        {
            // For client-only action calls, the MVC pattern "model-controller-view" gets broken
            // because there is no view anymore - raw data travels between the client and the server
            // without any need to create or return the markup. For such calls, the place where the RuleEditor
            // would be normally declared does not exist anymore. Therefore, we need to declare it
            // programmatically. The GetRuleEditor() method does that. It also creates the model and passes
            // it to the instance of the rule editor.
            RuleEditor editor = this.GetRuleEditor();

            // Get UI settings
            string settings = editor.GetClientSettings();

            // Send the settings back to the client
            return Json(settings, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult LoadRule(string ruleId)
        {
            // Load the rule from the storage file by its ID
            string ruleXml = StorageService.LoadRuleXml(ruleId);

            // See the comments in the LoadSettings() method
            RuleEditor editor = this.GetRuleEditor(ruleXml);

            // Get the rule's client data
            string ruleJson = editor.GetClientRuleData();

            // Send it back to the server
            return Json(ruleJson, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult DeleteRule(string ruleId)
        {
            try
            {
                // Delete the rule from the storage file by its ID
                StorageService.DeleteRule(ruleId);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.DenyGet);
            }

            // Respond to the client's request with no data
            return Json(null, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult SaveRule(string ruleData)
        {
            Result result = new Result();

            // See the comments in the LoadSettings() method
            RuleEditor editor = this.GetRuleEditor();

            // Load the rule into the editor
            editor.LoadClientData(ruleData);

            if (editor.Rule.IsEmpty())
            {
                result.IsRuleEmpty = true;
            }
            else if (!editor.Rule.IsValid(StorageService.LoadRuleXml))
            {
                result.IsRuleValid = false;
                // Load the json string of invalid data into the Result object
                result.ClientInvalidData = editor.GetClientInvalidData();
            }
            else
            {
                // Save the rule
                StorageService.SaveRule(editor.Rule.Id.ToString(), editor.Rule.GetRuleXml(), editor.Rule.IsLoadedRuleOfEvalType == null ? true : (bool)editor.Rule.IsLoadedRuleOfEvalType);
                // Send ID of this rule to the client
                result.Output = editor.Rule.Id.ToString();
            }

            return Json(result, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult EvaluateRule(Patient patient, string ruleData)
        {
            Result result = new Result();

            // See the comments in the LoadSettings() method
            RuleEditor editor = this.GetRuleEditor();

            // Load the rule into the editor
            editor.LoadClientData(ruleData);

            // We are not saving the rule, just evaluating it. Tell the editor not to enforce the rule name validation
            editor.Rule.SkipNameValidation = true;

            if (editor.Rule.IsEmpty())
            {
                result.IsRuleEmpty = true;
            }
            else if (!editor.Rule.IsValid(StorageService.LoadRuleXml))
            {
                result.IsRuleValid = false;
                // Load the json string of invalid data into the Result object
                result.ClientInvalidData = editor.GetClientInvalidData();
            }
            else
            {
                // Create an instance of the Evaluator class. Because our rules might reference other rules of evaluation type
                // we use constructor that takes rule's XML and delegate of the method that can load referenced rules by their IDs.
                Evaluator<Patient> evaluator = new Evaluator<Patient>(editor.Rule.GetRuleXml(), StorageService.LoadRuleXml);

                // Evaluate the patient against the rule
                bool success = evaluator.Evaluate(patient);

                // Return the evaluated patient back to the client
                result.Patient = patient;

                // Output the result of the evaluation to the client
                result.Output = string.IsNullOrWhiteSpace(patient.Output) ? "The rule evaluated to " + success.ToString() : patient.Output;
            }

            return Json(result, JsonRequestBehavior.DenyGet);
        }

        private RuleEditor GetRuleEditor()
        {
            return this.GetRuleEditor(null);
        }

        private RuleEditor GetRuleEditor(string ruleXml)
        {
            RuleEditor editor = new RuleEditor("ruleEditor");

            // Client-only editor
            editor.ClientOnly = true;

            editor.Mode = ESPL.Rule.Common.RuleType.Execution;

            if (ruleXml == null)
                editor.Rule = RuleModel.Create(typeof(Patient));
            else
                editor.Rule = RuleModel.Create(ruleXml, typeof(Patient));

            editor.ToolBarRules = StorageService.GetAllRules();
            editor.ContextMenuRules = StorageService.GetEvaluationRules();

            return editor;
        }
	}
}