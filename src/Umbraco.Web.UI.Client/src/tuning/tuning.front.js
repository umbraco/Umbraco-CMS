
/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

var refrechLayout = function (parameters) {
    var string = "less.modifyVars({" + parameters.join(",") + "})";
    eval(string);
}

var closeIntelTuning = function (tuningModel) {

    if (tuningModel) {

        $('a').removeClass("myDisable");
        $('a').unbind("click.myDisable");

        $("[tuning-over]").css('outline', 'none');
        $.each(tuningModel.categories, function (key, category) {
            $.each(category.sections, function (key, section) {
                $.each(section.subSections, function (key, subSection) {
                    if (subSection.schema) {
                        $(subSection.schema).unbind();
                        $(subSection.schema).removeAttr("tuning-over");
                    }
                });
            });
        });
    }

}

var initIntelTuning = function (tuningModel) {

    if (tuningModel) {

        $('a').addClass("myDisable");
        $('a').bind("click.myDisable", function () { return false; });

        $.each(tuningModel.categories, function (key, category) {
            $.each(category.sections, function (key, section) {
                $.each(section.subSections, function (key, subSection) {
                    if (subSection.schema) {
                        $(subSection.schema).attr("tuning-over", subSection.schema);
                    }
                });
            });
        });

        $(document).mousemove(function (e) {

            e.stopPropagation();

            $("[tuning-over]").css('outline', 'none');

            var target = $(e.target);
            while (target.length > 0 && (target.attr('tuning-over') == undefined || target.attr('tuning-over') == '')) {
                target = target.parent();
            }

            if (target.attr('tuning-over') != undefined || target.attr('tuning-over') != '') {
                target.unbind();
                target.css('outline', '2px solid blue');
                target.click(function (e) {
                    e.stopPropagation();
                    console.info(target.attr('tuning-over'));
                    parent.refrechIntelTuning(target.attr('tuning-over'));
                });
            }
        });

    }

}