myDesignSurface.addEventListener("SaveDesign", "savedesign", function(formguid, design) {


    var formname = $("#" + formname_id).val();
    var formmanualapproval = $("#" + formmanualapproval_id).attr('checked');
    var formsubmitmessage = $("#" + formsubmitmessage_id).val();
    var formsubmitpage = $("#" + formsubmitpage_id + " .picker input").val();
    var formshowvalsum = $("#" + formshowvalidationsum_id).attr('checked');
    var formhidefieldval = $("#" + formhidefieldvalidation_id).attr('checked');

    var formrequirederrormessage = $("#" + formrequirederrormessage_id).val();
    var forminvaliderrormessage = $("#" + forminvaliderrormessage_id).val();

    var formfieldindicationtype = 0;

    if ($("#" + formmandatoryonly_id).attr('checked')) {
        formfieldindicationtype = 1;
    }
    if ($("#" + formoptionalonly_id).attr('checked')) {
        formfieldindicationtype = 2;
    }

    var formindicator = $("#" + formindicator_id).val();
    var formdisablestylesheet = $("#" + formdisablestylesheet_id).attr('checked');

    //Call save method of designer webservice
    Umbraco.Forms.UI.Webservices.Designer.Save(formguid, formname, formmanualapproval, formsubmitmessage, formsubmitpage, formrequirederrormessage, forminvaliderrormessage, formshowvalsum, formhidefieldval, design, formfieldindicationtype, formindicator, formdisablestylesheet, SaveSucces, SaveFailure);
});



function SaveSucces(retVal) {
    jQuery("#submitbutton").attr("disabled", false);
}
function SaveFailure(retVal) {
    alert("Failed to save form");
}