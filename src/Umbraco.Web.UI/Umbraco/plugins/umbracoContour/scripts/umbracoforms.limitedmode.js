// Can't add fields
// Can't delete fields
// Can't delete pages that have fields
// Can't delete fieldsets that have fields
// Can't crud prevalues
$(document).ready(function() {
    LimitEditor();


myDesignSurface.addEventListener("UpdateFieldsetSortOrder", "updatefieldsetsortorder", function(id, sortorder) {
    LimitEditor();
});

myDesignSurface.addEventListener("UpdateFieldSortOrder", "updatefieldsortorder", function(id, sortorder) {
    LimitEditor();
});

myDesignSurface.addEventListener("AddFieldset", "addfieldset", function(pageid, id, name) {
    LimitEditor();
});

myDesignSurface.addEventListener("AddPage", "addpage", function(id, name) {
    LimitEditor();
});

myDesignSurface.addEventListener("UpdateField", "updatefield", function(id,name,type,mandatory,regex) {
    $(".fieldeditprevalues").hide();
});

myDesignSurface.addEventListener("ShowUpdateFieldDialog", "showupdatefielddialog", function (field) {

    if ($('#' + field).attr('fieldmandatory').toLowerCase() == 'true' || $('#' + field).attr('fieldmandatory') == '1') {
        $("#fieldmandatory").attr('disabled', true);
    }
    else {
        $("#fieldmandatory").attr('disabled', false);
    }

});

});

function LimitEditor() {

    //remove page add/navigation
    $("#stepsnavigation").hide();
    
    // remove field delete
    $("#designsurface .field .delete").remove();
    
    //remove field copy
    $("#designsurface .field .copy").remove();

    //fieldset delete
    $("#designsurface .fieldset").each(function() {
    
            if($(this).children(".fieldcontainer").children(".field").size() > 0){
            
                 $(this).children(".fieldsetheader").children(".delete").hide();
            }
            else{
                $(this).children(".fieldsetheader").children(".delete").show();
            }
        });
    
    //page delete
     $("#designsurface .page").each(function() {
         if ($("#" + $(this).attr('id') + " .field").children().size() > 0) {

             $(this).children(".delete").hide();
         }
         else {
             $(this).children(".delete").show();
         }
    
    });
    
    
    //field add
    //remove menu addfield button
    $("#" + addfield_id).remove();
    //remove add field on fieldsets
    $("#designsurface .fieldset .add").remove();
    $("#designsurface .fieldset .addfield").remove();
    

    //can't change fieldtype on field
    $("#fieldtype").attr('disabled', true);

    //not possible to edit prevalues
    $(".fieldeditprevalues").hide();


    //disable prevaluetype select
    $("#prevaluestype").attr("disabled", "disabled");
}
