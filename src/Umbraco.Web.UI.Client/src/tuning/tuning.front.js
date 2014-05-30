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
    outlinePosition($(schema));
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

            var target = $(e.target);
            while (target.length > 0 && (target.attr('tuning-over') == undefined || target.attr('tuning-over') == '')) {
                target = target.parent();
            }

            if (target.attr('tuning-over') != undefined && target.attr('tuning-over') != '') {
                target.unbind();

                outlinePosition(target);

                target.click(function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    console.info(target.attr('tuning-over'));
                    parent.refrechIntelTuning(target.attr('tuning-over'));
                    return false;
                });
            }
            else {
                outlineHide();
            }
        });

    }

}

var outlinePosition = function (target) {

    var localname = target[0].localName;
    var height = $(target).height();
    var width = $(target).width();
    var position = $(target).offset();

    console.info("element select " + localname);

    $("#outline-data").html(localname);
    $("#outline-data").css('position', 'fixed');
    $("#outline-data").css('top', position.top);
    $("#outline-data").css('left', position.left);
    $("#outline-data").css('display', 'block');
    $("#outline-data").css('position', 'fixed');
    $("#outline-data").css('background-color', 'rgb(164, 198, 253)');
    $("#outline-data").css('color', '#000000');
    $("#outline-data").css('padding', '0px 5px 0px 5px');
    $("#outline-data").css('font-size', '11px');
    $("#outline-data").css('transition', 'all .05s ease-in-out');
    $("#outline-data").css('-moz-transition', 'all .05s ease-in-out');
    $("#outline-data").css('-webkit-transition', 'all .05s ease-in-out');

    $("#outline-sup").css('display', "block");
    $("#outline-sup").css('height', "2px");
    $("#outline-sup").css('width', width + "px");
    $("#outline-sup").css('position', 'fixed');
    $("#outline-sup").css('top', position.top);
    $("#outline-sup").css('left', position.left);
    $("#outline-sup").css('background-color', '#a4c6fd');
    $("#outline-sup").css('transition', 'all .05s ease-in-out');
    $("#outline-sup").css('-moz-transition', 'all .05s ease-in-out');
    $("#outline-sup").css('-webkit-transition', 'all .05s ease-in-out');

    $("#outline-inf").css('display', "block");
    $("#outline-inf").css('height', "2px");
    $("#outline-inf").css('width', Number(width + 2) + "px");
    $("#outline-inf").css('position', 'fixed');
    $("#outline-inf").css('top', position.top + height);
    $("#outline-inf").css('left', position.left);
    $("#outline-inf").css('background-color', '#a4c6fd');
    $("#outline-inf").css('transition', 'all .05s ease-in-out');
    $("#outline-inf").css('-moz-transition', 'all .05s ease-in-out');
    $("#outline-inf").css('-webkit-transition', 'all .05s ease-in-out');

    $("#outline-left").css('display', "block");
    $("#outline-left").css('height', height + "px");
    $("#outline-left").css('width', "2px");
    $("#outline-left").css('position', 'fixed');
    $("#outline-left").css('top', position.top);
    $("#outline-left").css('left', position.left);
    $("#outline-left").css('background-color', '#a4c6fd');
    $("#outline-left").css('transition', 'all .05s ease-in-out');
    $("#outline-left").css('-moz-transition', 'all .05s ease-in-out');
    $("#outline-left").css('-webkit-transition', 'all .05s ease-in-out');

    $("#outline-right").css('display', "block");
    $("#outline-right").css('height', height + "px");
    $("#outline-right").css('width', "2px");
    $("#outline-right").css('position', 'fixed');
    $("#outline-right").css('top', position.top);
    $("#outline-right").css('left', position.left + width);
    $("#outline-right").css('background-color', '#a4c6fd');
    $("#outline-right").css('transition', 'all .05s ease-in-out');
    $("#outline-right").css('-moz-transition', 'all .05s ease-in-out');
    $("#outline-right").css('-webkit-transition', 'all .05s ease-in-out');

}

var outlineHide = function () {

    $("#outline-data").css('display', "none");
    $("#outline-sup").css('display', "none");
    $("#outline-inf").css('display', "none");
    $("#outline-right").css('display', "none");
    $("#outline-left").css('display', "none");

}

var initTuningPanel = function () {

    // Looking for grid row
    var tuningGridList = []
    $("div[class^='gridrow_']").each(function (index, value) {
        tuningGridList.splice(tuningGridList.length + 1, 0, $(value).attr("class"))
    });

    // Init panel 
    if (parent.setFrameIsLoaded) {
        parent.setFrameIsLoaded(tuningParameterUrl, tuningGridList);
    }
}

$(function () {

    // Init ouline layer
    $("body").append("<div id=\"outline-data\"></div><div id=\"outline-sup\"></div><div id=\"outline-inf\"></div><div id=\"outline-left\"></div><div id=\"outline-right\"></div>");

    // Init tuning panel
    initTuningPanel();

});

