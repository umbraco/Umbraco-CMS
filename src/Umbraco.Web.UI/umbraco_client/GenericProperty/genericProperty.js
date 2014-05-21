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
        inputName.on('input blur', function (event) {
            getSafeAlias(inputAlias, inputName.val(), false, function (alias) {
                if (!inputAlias.data('dirty'))
                    inputAlias.val(alias);
            });
        });
        inputAlias.on('input', function(event) {
            inputName.off('input blur');
        });
    });
}

function checkAlias(aliasSelector) {
    $(aliasSelector).on('input', function (event) {
        var input = $(this);
        input.data('dirty', true);
        var value = input.val();
        validateSafeAlias(input, value, false, function (isSafe) {
            input.toggleClass('highlight-error', !isSafe);
        });
    }).on('blur', function(event) {
        var input = $(this);
        if (!input.data('dirty')) return;
        input.removeData('dirty');
        var value = input.val();
        getSafeAlias(input, value, true, function (alias) {
            if (value.toLowerCase() != alias.toLowerCase())
                input.val(alias);
            input.removeClass('highlight-error');
        });
    });
}

// validateSafeAlias and getSafeAlias are defined by UmbracoCasingRules.aspx
