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
    })
}

function checkAlias(theId) {
    jQuery('#' + theId).keyup(function(event) {
        var currentAlias = jQuery('#' + theId).val();
        jQuery('#' + theId).toggleClass('aliasValidationError', !isValidAlias(currentAlias));
    })

    jQuery('#' + theId).blur(function(event) {
        var currentAlias = jQuery('#' + theId).val();
        jQuery('#' + theId).val(safeAlias(currentAlias));
        jQuery('#' + theId).removeClass('aliasValidationError');
    })

}

function safeAlias(alias) {
    if (UMBRACO_FORCE_SAFE_ALIAS) {
        var safeAlias = '';
        var aliasLength = alias.length;
        for (var i = 0; i < aliasLength; i++) {
            currentChar = alias.substring(i, i + 1);
            if (UMBRACO_FORCE_SAFE_ALIAS_VALIDCHARS.indexOf(currentChar.toLowerCase()) > -1) {
                // check for camel (if previous character is a space, we'll upper case the current one
                if (safeAlias == '' && UMBRACO_FORCE_SAFE_ALIAS_INVALID_FIRST_CHARS.indexOf(currentChar.toLowerCase()) > 0) { 
                    currentChar = '';
                } else {
                    // first char should always be lowercase (camel style)
                    if (safeAlias.length == 0)
                        currentChar = currentChar.toLowerCase();

                    if (i < aliasLength - 1 && safeAlias != '' && alias.substring(i - 1, i) == ' ')
                        currentChar = currentChar.toUpperCase();

                    safeAlias += currentChar;
                }
            }
        }

        return safeAlias;

    } else {
        return alias;
    }
}

function isValidAlias(alias) {
    return alias == safeAlias(alias);
}
