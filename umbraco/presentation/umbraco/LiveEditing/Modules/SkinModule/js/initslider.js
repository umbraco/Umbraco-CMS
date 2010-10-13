jQuery(".skinningslider").each(function () {

    var vals = jQuery(this).attr("rel").split(",");

    var minimum = vals[0];
    var maximum = vals[1];
    var initial = vals[2];
    var target = vals[3];

    jQuery(this).slider({
        change: function (event, ui) { jQuery("#" + target).val(ui.value); jQuery("#" + target).trigger("change"); },
        slide: function (event, ui) { jQuery("#" + target).val(ui.value); jQuery("#" + target).trigger("change"); },
        min: minimum,
        max: maximum,
        value: initial
    });

});