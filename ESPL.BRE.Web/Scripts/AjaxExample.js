// Global variable to hold reference to Code Effects component
// (optional, just for the sake of convenience)
var codeeffects;

// Init everything on page load
$(function () {
    // Use Code Effects' global shortcut $ce to get reference to
    // the control by its server ID (not its client ID)
    codeeffects = $ce("ruleEditor");

    // Set action delegates that Code Effects component calls on save, load or delete the rule.
    // The functions themselves are declared later in this script
    codeeffects.setClientActions(loadRule, deleteRule, saveRule);

    // Use the evaluateRule method as Evaluate button's click event handler
    $("#Button").click(evaluateRule);

    // Finally, call the GetSettings() web method to get control's settings
    loadSettings();
});

// Method that requests Code Effects UI settings from the
// server and initializes Code Effects
function loadSettings() {
    // Make sure to clear the Rule Area
    codeeffects.clear();
    // Call the LoadSettings MVC action declared in the AjaxController to get the settings
    post("/LoadSettings", null, settingsLoaded);
};

// Method that handles server response to the loadSettings call
function settingsLoaded(settings) {
    // Pass the received json string to Code Effects
    codeeffects.loadSettings(settings);
    // Tell the user that Code Effects component is loaded and ready
    $("#Info").text("The rule editor is loaded and ready");
};

// Client action method that loads rule from the storage.
// It has to have one parameter
function loadRule(ruleId) {
    // Call the LoadRule MVC action declared in the AjaxController
    post("/LoadRule", JSON.stringify({ ruleId: ruleId }), ruleLoaded);
};

// Handler of the successful loadRule() call
function ruleLoaded(ruleData) {
    // Load the rule into the Rule Area by passing Code Effects the json
    // string returned by the server.
    codeeffects.loadRule(ruleData);
    // Notify the user that the rule was loaded
    $("#Info").text("Rule is loaded");
};

// Client action method that deletes the current rule from the storage.
// It has to define one parameter
function deleteRule(ruleId) {
    // Call the DeleteRule MVC action declared in the AjaxController
    post("/DeleteRule", JSON.stringify({ ruleId: ruleId }), ruleDeleted);
};

// Handler of the successful deleteRule() call
function ruleDeleted(message) {
    if (message == null) {
        // Let the Code Effects know that there were no errors and
        // the rule of codeeffects.getRuleId() ID was deleted successfully
        codeeffects.deleted(codeeffects.getRuleId());
        // Clear the Rule Area (we just deleted the rule,
        // there is no point of displaying it anymore)
        codeeffects.clear();
        // Notify the user that the rule was deleted
        $("#Info").text("Rule was deleted successfully");
    }
    else $("#Info").text(message);
};

// Client action method that saves the new or existing rule in the storage.
// It has to define one parameter
function saveRule(ruleData) {
    // Call the SaveRule MVC action declared in the AjaxController
    post("/SaveRule", JSON.stringify({ ruleData: ruleData }), ruleSaved);
};

// Handler of the successful saveRule() call
function ruleSaved(data) {
    if (data.IsRuleEmpty) $("#Info").text("The rule is empty"); // The Rule Area is empty
    else if (!data.IsRuleValid) {
        // The rule is invalid. Pass the received invalid client data to Code Effects
        codeeffects.loadInvalids(data.ClientInvalidData);
        // If the Help String is enabled, Code Effects component does notify the user that the rule is invalid.
        // But here we also use the info label to do the same. Just for the sake of consistency :)
        $("#Info").text("The rule is not valid");
    }
    else {
        // Server returns rule ID using the Output property of ProcessingResult C# type.
        // Code Effects component needs this ID if the saved rule was a new rule. In any case, pass it to Code Effects.
        codeeffects.saved(data.Output);
        // Inform the user
        $("#Info").text("The rule was saved successfully");
    }
};

// Function that handles the evaluation of the current rule. It's attached to the Evaluate button
// as its "click" handler (see the init function on top of the script). Normally you wouldn't want
// to evaluate rules in a web application but we do this here simply to demonstrate how it can be done.
function evaluateRule() {
    // The codeeffects.extract() method returns rule's data; getSource() method is declared later in
    // this script; it returns values of the Patient's form controls.
    var data = JSON.stringify({ patient: getSource(), ruleData: codeeffects.extract() });
    // Call the EvaluateRule MVC action declared in the AjaxController
    post("/EvaluateRule", data, ruleEvaluated);
};

// Handler of the successful evaluateRule() call
function ruleEvaluated(data) {
    data = data.d || data;
    if (data.IsRuleEmpty) $("#Info").text("The rule is empty"); // The Rule Area was empty
    else if (!data.IsRuleValid) {
        // The rule is not valid, pass invalid data to Code Effects
        codeeffects.loadInvalids(data.ClientInvalidData);
        // Notify the user
        $("#Info").text("The rule is not valid");
    }
    else {
        // Set page controls in case value setters are used in the rule
        setControls(data.Patient);

        // Display the evaluation output returned by the server
        $("#Info").text(data.Output);
    }
};

// Utility wrapper that handles HTTP posts to the server.
// jQuery is used here as an example; you can use any framework
// that wraps calls to XmlHttpRequest
function post(url, data, delegate) {
    $.ajax({
        type: "POST",
        url: window.location.pathname + url,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: data,
        cache: false,
        success: delegate,
        error: error /*all errored calls will be delegated to the error() method declared below*/
    });
};

// Handler of errored server calls
function error(e) {
    if (!e) throw Eror("Generic server error.");
    else {
        // We are using properties of the complex "e" error object to notify
        // the user about the error. This error handling has nothing
        // to do with Code Effects and exists here only for the sake of completeness.
        if (e.responseText) {
            $("#Info").text("Server error");
            var div = document.createElement("DIV");
            div.innerHTML = e.responseText;
            div.className = "exception";
            document.body.appendChild(div);
        }
    }
};

// Function that returns the client representation of the MVC Patient model,
// filled with data from the Patient's form.
function getSource() {
    function Patient() { };
    function Address() { };

    var patient = new Patient();
    patient.FirstName = getText("FirstName");
    patient.LastName = getText("LastName");
    patient.Email = getText("Email");
    patient.Gender = $("#Gender option:selected").val();
    patient.EducationTypeID = $("#EducationTypeID option:selected").val();
    patient.PhysicianID = $("#PhysicianID option:selected").val();

    var d = getText("DOB");
    if (d != null) {
        try { patient.DOB = new Date(d); }
        catch (e) { patient.DOB = null; };
    }

    patient.Home = new Address();
    patient.Home.Street = getText("Home_Street");
    patient.Home.City = getText("Home_City");
    patient.Home.Zip = getText("Home_Zip");
    patient.Home.State = $("#Home_State option:selected").val();

    patient.Work = new Address();
    patient.Work.Street = getText("Work_Street");
    patient.Work.City = getText("Work_City");
    patient.Work.Zip = getText("Work_Zip");
    patient.Work.State = $("#Work_State option:selected").val();

    patient.Pulse = getText("Pulse");
    patient.SystolicPressure = getText("SystolicPressure");
    patient.DiastolicPressure = getText("DiastolicPressure");
    patient.Temperature = getText("Temperature");

    patient.Headaches = $("#Headaches").is(":checked");
    patient.Allergies = $("#Allergies").is(":checked");
    patient.Tobacco = $("#Tobacco").is(":checked");
    patient.Alcohol = $("#Alcohol").is(":checked");

    return patient;
};

// Sets page control values to values of the
// patient instence that came from the server
function setControls(patient) {
    if (patient.DOB) {
        try {
            var dob = new Date(parseInt(patient.DOB.replace("/Date(", "").replace(")/", "")));
            $("#DOB").val((dob.getMonth() + 1) + "/" + dob.getDate() + "/" + dob.getFullYear());
        }
        catch (e) { $("#DOB").val("= invalid date ="); };
    }
    else $("#DOB").val("");

    setText("FirstName", patient.FirstName);
    setText("LastName", patient.LastName);
    setText("Email", patient.Email);
    $("#Gender").val(patient.Gender);
    $("#EducationTypeID").val(patient.EducationTypeID);
    $("#PhysicianID").val(patient.PhysicianID);

    if (!patient.Home) patient.Home = new Address();
    setText("Home_Street", patient.Home.Street);
    setText("Home_City", patient.Home.City);
    setText("Home_Zip", patient.Home.Zip);
    $("#Home_State").val(patient.Home.State);

    if (!patient.Work) patient.Work = new Address();
    setText("Work_Street", patient.Work.Street);
    setText("Work_City", patient.Work.City);
    setText("Work_Zip", patient.Work.Zip);
    $("#Work_State").val(patient.Work.State);

    setText("Pulse", patient.Pulse);
    setText("SystolicPressure", patient.SystolicPressure);
    setText("DiastolicPressure", patient.DiastolicPressure);
    setText("Temperature", patient.Temperature);

    $("#Headaches").prop("checked", patient.Headaches);
    $("#Allergies").prop("checked", patient.Allergies);
    $("#Tobacco").prop("checked", patient.Tobacco);
    $("#Alcohol").prop("checked", patient.Alcohol);
};

// Utility method that makes sure that empty
// textbox values will be returned as nulls
function getText(id) {
    var v = $("#" + id).val();
    return v.length == 0 ? null : v;
};

function setText(id, val) {
    val = val || "";
    $("#" + id).val(val);
};