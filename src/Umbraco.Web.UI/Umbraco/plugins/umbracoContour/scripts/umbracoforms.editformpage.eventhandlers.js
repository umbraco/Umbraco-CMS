myDesignSurface.addEventListener("SaveDesign", "savedesign", function (formguid, design) {


    var formname = $("#" + formname_id).val();
    var formmanualapproval = $("#" + formmanualapproval_id).is(':checked');
    var formsubmitmessage = $("#" + formsubmitmessage_id).val();
    var formsubmitpage = $("#" + formsubmitpage_id + " .picker input").val();

    if (formsubmitpage == "" || formsubmitpage == null) {
        formsubmitpage = $("#" + formsubmitpage_id + " .xpath input").val();
    }

    var formshowvalsum = $("#" + formshowvalidationsum_id).is(':checked');
    var formhidefieldval = $("#" + formhidefieldvalidation_id).is(':checked');

    var formrequirederrormessage = $("#" + formrequirederrormessage_id).val();
    var forminvaliderrormessage = $("#" + forminvaliderrormessage_id).val();

    var formfieldindicationtype = 0;

    if ($("#" + formmandatoryonly_id).is(':checked')) {
        formfieldindicationtype = 1;
    }
    if ($("#" + formoptionalonly_id).is(':checked')) {
        formfieldindicationtype = 2;
    }

    var formindicator = $("#" + formindicator_id).val();
    var formdisablestylesheet = $("#" + formdisablestylesheet_id).is(':checked');

    var formsaverecordlocaly = true;

    if (formstorerecordslocally_id != "") {
        formsaverecordlocaly = $("#" + formstorerecordslocally_id).is(':checked');
    }
    //Call save method of designer webservice
    UmbracoContour.Webservices.Designer.Save(formguid, formname, formmanualapproval, formsubmitmessage, formsubmitpage, formrequirederrormessage, forminvaliderrormessage, formshowvalsum, formhidefieldval, design, formfieldindicationtype, formindicator, formdisablestylesheet, formsaverecordlocaly, SaveSucces, SaveFailure);
});



function SaveSucces(retVal) {
    if (retVal.toString() == "true") {
        setDirty(false);
        //top.UmbSpeechBubble.ShowMessage('save', lang_formsaveok, '');
        ShowChanges();
    }
    else {
        top.UmbSpeechBubble.ShowMessage('error', lang_formsavefailed, '');
    }
}
function SaveFailure(retVal) {
    top.UmbSpeechBubble.ShowMessage('error', lang_formsavefailed, '');
}


