$(document).ready(function() {
    ReadOnlyEditor();
});

function ReadOnlyEditor() {

    //menu buttons
    $(".sl").remove();

    $("#designsurface .add").remove();
    $("#designsurface .delete").remove();
    $("#designsurface .copy").remove();
    $("#designsurface .update").remove();
    $("#designsurface .handle").remove();

    $("#designsurface .addfield").remove();
    
    //not possible to edit prevalues
    $("#designsurface .fieldeditprevalues").remove();

    $("#stepsnavigation").hide();
}