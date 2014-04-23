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
function duplicatePropertyNameAsSafeAlias(nameId, aliasId) {
    var inputName = $('#' + nameId);
    var inputAlias = $('#' + aliasId);
    inputName.on('input', function (event) {
        getSafeAlias(inputAlias, inputName.val(), false, function (alias) {
            inputAlias.val(alias);
        });
    }).on('blur', function (event) {
        $(this).off('input');
    });
}

function checkAlias(aliasId) {
    var input = $('#' + aliasId);
    
    input.keyup(function(event) {
        var value = $(this).val();
        validateSafeAlias(aliasId, value, false, function (isSafe) {
            input.toggleClass('aliasValidationError', !isSafe);
        });
    });

    input.blur(function(event) {
        var value = $(this).val();
        getSafeAlias(aliasId, value, true, function (alias) {
            if (value.toLowerCase() != alias.toLowerCase())
                input.val(alias);
            input.removeClass('aliasValidationError');
        });
    });
}

// validateSafeAlias and getSafeAlias are defined by UmbracoCasingRules.aspx
