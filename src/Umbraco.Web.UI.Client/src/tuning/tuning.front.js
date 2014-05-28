/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

var refrechLayout = function (parameters) {

    // Disable links
    $('a').addClass("myDisable");
    $('a').bind("click.myDisable", function () {
            return false;
    });

    var string = "less.modifyVars({" + parameters.join(",") + "})";
    eval(string);
}


/* Fonts loaded in the tuning panel need to be loaded independently in
 * the content iframe to allow live previewing.
 */
var webFontScriptLoaded = false;
var getFont = function (font) {
    if (!webFontScriptLoaded) {
        $.getScript('http://ajax.googleapis.com/ajax/libs/webfont/1/webfont.js')
        .done(function () {
            webFontScriptLoaded = true;
            // Recursively call once webfont script is available.
            getFont(font);
        })
        .fail(function () {
            console.log('error loading webfont');
        });
    }
    else {
        WebFont.load({
            google: {
                families: [font.fontFamily + ":" + font.variant]
            },
            loading: function () {
                console.log('loading font' + font.fontFamily + ' in iframe');
            },
            active: function () {
                console.log('loaded font ' + font.fontFamily + ' in iframe');
            },
            inactive: function () {
                console.log('error loading font ' + font.fontFamily + ' in iframe');
            }
        });
    }
}

var setSelectedSchema = function (schema) {
    $("[tuning-over]").css('outline', 'none');
    $(schema).css('outline', '2px solid #a4c6fd');
}

var closeIntelTuning = function (tuningModel) {

    if (tuningModel) {

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

        // Add tuning-over attr for each schema from config
        $.each(tuningModel.categories, function (key, category) { 
            $.each(category.sections, function (key, section) {
                $.each(section.subSections, function (key, subSection) {
                    if (subSection.schema) {
                        $(subSection.schema).attr("tuning-over", subSection.schema);
                    }
                });
            });
        });

        // Outline tuning-over
        $(document).mousemove(function (e) {

            e.stopPropagation();

            $("[tuning-over]").css('outline', 'none');

            var target = $(e.target);
            while (target.length > 0 && (target.attr('tuning-over') == undefined || target.attr('tuning-over') == '')) {
                target = target.parent();
            }

            if (target.attr('tuning-over') != undefined || target.attr('tuning-over') != '') {
                target.unbind();
                target.css('outline', '2px solid #a4c6fd');
                target.click(function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    console.info(target.attr('tuning-over'));
                    parent.refrechIntelTuning(target.attr('tuning-over'));
                    return false;
                });
            }
        });

    }

}

var initTuningPanel = function () {

    // Init panel 
    if (parent.setFrameIsLoaded) {
        parent.setFrameIsLoaded(tuningParameterUrl);
    }

}

initTuningPanel();