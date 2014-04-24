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
function duplicatePropertyNameAsSafeAlias(propertySelector) {
    $(propertySelector).each(function() {
        var prop = $(this);
        var inputName = prop.find('.prop-name');
        var inputAlias = prop.find('.prop-alias');
        inputName.on('input', function (event) {
            getSafeAlias(inputAlias, inputName.val(), false, function (alias) {
                inputAlias.val(alias);
            });
        }).on('blur', function (event) {
            $(this).off('input');
        });
    });
}

function checkAlias(aliasSelector) {
    $(aliasSelector).keyup(function (event) {
        var input = $(this);
        var value = input.val();
        validateSafeAlias(input, value, false, function (isSafe) {
            input.toggleClass('highlight-error', !isSafe);
        });
    }).blur(function(event) {
        var input = $(this);
        var value = input.val();
        getSafeAlias(input, value, true, function (alias) {
            if (value.toLowerCase() != alias.toLowerCase())
                input.val(alias);
            input.removeClass('highlight-error');
        });
    });
}

// validateSafeAlias and getSafeAlias are defined by UmbracoCasingRules.aspx
