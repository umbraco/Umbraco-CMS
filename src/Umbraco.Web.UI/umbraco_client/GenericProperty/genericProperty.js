var activeDragId = "";
function expandCollapse(theId) {

    var edit = document.getElementById("edit" + theId);

    if (edit.style.display == 'none') {
        edit.style.display = 'block';
        document.getElementById("desc" + theId).style.display = 'none';
    }
    else {
        edit.style.display = 'none';
        document.getElementById("desc" + theId).style.display = 'block';
    }
}
function duplicatePropertyNameAsSafeAlias(theId, theAliasId) {
    jQuery('#' + theId).keyup(function(event) {
        var message = jQuery('#' + theId).val();
        jQuery('#' + theAliasId).val(safeAlias(message));
    });
}

function checkAlias(theId) {
    jQuery('#' + theId).keyup(function(event) {
        var currentAlias = jQuery('#' + theId).val();
        jQuery('#' + theId).toggleClass('aliasValidationError', !isSafeAlias(currentAlias));
    });

    jQuery('#' + theId).blur(function(event) {
        var currentAlias = jQuery('#' + theId).val();
        jQuery('#' + theId).val(safeAlias(currentAlias));
        jQuery('#' + theId).removeClass('aliasValidationError');
    });
}

// note: safeAlias(alias) and isSafeAlias(alias) now defined by UmbracoCasingRules.aspx along with constants

// provided for backward-compatibility, should anybody non-core be using it
function isValidAlias(alias) {
    return isSafeAlias(alias);
}
