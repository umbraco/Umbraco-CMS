
jQuery(document).ready(function () {

    jQuery("#prevalues .text").editable(function (value, settings) { $(this).html(value); ResetValues(); }, { onblur: 'submit', tooltip: 'Click to edit', cssclass: 'inlineEditor' });

    jQuery("#prevalues tbody").sortable({
        items: "tr:not(.header)",
        handle: '.handle',
        update: function () {

            ResetValues();

        }
    });



});


function ResetValues() {

    var val = "";
   
    jQuery("#prevalues .row").each(function () {

        var text = jQuery(".text", this).html();
        var value = jQuery(".value", this).html();


        val += value + "|" + text + "¶";

    });

    jQuery(".valuesHiddenInput").val(val);
}

function ConfirmPrevalueDelete() {
    return confirm("Are you sure you want to delete");
}