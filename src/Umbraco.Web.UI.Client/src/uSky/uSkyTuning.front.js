
/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

var refrechLayout = function (parameters) {
    var string = "less.modifyVars({" + parameters.join(",") + "})";
    eval(string);
}

var closeIntelTuning = function (tunningModel) {

    $('a').removeClass("myDisable");
    $('a').unbind("click.myDisable");

    $("[uSky-over]").css('outline', 'none');
    $.each(tunningModel.categories, function (key, category) {
        $.each(category.sections, function (key, section) {
            $.each(section.subSections, function (key, subSection) {
                if (subSection.schema) {
                    $(subSection.schema).unbind();
                    $(subSection.schema).removeAttr("uSky-over");
                }
            });
        });
    });

}

var initIntelTuning = function (tunningModel) {

    $('a').addClass("myDisable");
    $('a').bind("click.myDisable", function () { return false; });

    $.each(tunningModel.categories, function (key, category) {
        $.each(category.sections, function (key, section) {
            $.each(section.subSections, function (key, subSection) {
                if (subSection.schema) {
                    $(subSection.schema).attr("uSky-over", subSection.schema);
                }
            });
        });
    });

    $(document).mousemove(function (e) {

        e.stopPropagation();

        $("[uSky-over]").css('outline', 'none');

        var target = $(e.target);
        while (target.length > 0 && (target.attr('uSky-over') == undefined || target.attr('uSky-over') == '')) {
            target = target.parent();
        }

        if (target.attr('uSky-over') != undefined || target.attr('uSky-over') != '') {
            target.unbind();
            target.css('outline', '2px solid blue');
            target.click(function (e) {
                e.stopPropagation();
                console.info(target.attr('uSky-over'));
                parent.uSkyRefrechIntelTuning(target.attr('uSky-over'));
            });
        }
    });

}