/*********************************************************************************************************/
/* Global function and variable for panel/page com */
/*********************************************************************************************************/

var refrechLayout = function (parameters) {

    // hide preview badget
    $("#umbracoPreviewBadge").hide();

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
                families: [font]
            },
            loading: function () {
                console.log('loading font' + font + ' in iframe');
            },
            active: function () {
                console.log('loaded font ' + font + ' in iframe');
            },
            inactive: function () {
                console.log('error loading font ' + font + ' in iframe');
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
        $.each(tuningModel.configs, function (indexConfig, config) {
            if (config.schema) {
                $(config.schema).unbind();
                $(config.schema).removeAttr("tuning-over");
            }
        });
    }

}

var initIntelTuning = function (tuningModel) {

    if (tuningModel) {

        // Add tuning-over attr for each schema from config
        $.each(tuningModel.configs, function (indexConfig, config) {
            var schema = config.selector ? config.selector : config.schema;
            if (schema) {
                $(schema).attr("tuning-over", config.name);
            }
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

    if (target.length > 0 && target.attr('tuning-over') != undefined && target.attr('tuning-over') != '') {

        var localname = target[0].localName;
        var height = $(target).outerHeight();
        var width = $(target).outerWidth();
        var position = $(target).offset();
        var posY = position.top - $(window).scrollTop();
        var posX = position.left - $(window).scrollLeft();

        console.info("element select " + localname);

        $("#outline-data").html(target.attr('tuning-over'));
        $("#outline-data").css('position', 'fixed');
        $("#outline-data").css('top', posY);
        $("#outline-data").css('left', posX);
        $("#outline-data").css('display', 'block');
        $("#outline-data").css('position', 'fixed');
        $("#outline-data").css('background-color', 'rgb(164, 198, 253)');
        $("#outline-data").css('color', '#000000');
        $("#outline-data").css('padding', '0px 5px 0px 5px');
        $("#outline-data").css('font-size', '11px');
        $("#outline-data").css('transition', 'all .05s ease-in-out');
        $("#outline-data").css('-moz-transition', 'all .05s ease-in-out');
        $("#outline-data").css('-webkit-transition', 'all .05s ease-in-out');
        $("#outline-data").css('z-index', '9999999999999999999999999');

        $("#outline-sup").css('display', "block");
        $("#outline-sup").css('height', "2px");
        $("#outline-sup").css('width', width + "px");
        $("#outline-sup").css('position', 'fixed');
        $("#outline-sup").css('top', posY);
        $("#outline-sup").css('left', posX);
        $("#outline-sup").css('background-color', '#a4c6fd');
        $("#outline-sup").css('transition', 'all .05s ease-in-out');
        $("#outline-sup").css('-moz-transition', 'all .05s ease-in-out');
        $("#outline-sup").css('-webkit-transition', 'all .05s ease-in-out');
        $("#outline-sup").css('z-index', '9999999999999999999999999');

        $("#outline-inf").css('display', "block");
        $("#outline-inf").css('height', "2px");
        $("#outline-inf").css('width', Number(width + 2) + "px");
        $("#outline-inf").css('position', 'fixed');
        $("#outline-inf").css('top', posY + height);
        $("#outline-inf").css('left', posX);
        $("#outline-inf").css('background-color', '#a4c6fd');
        $("#outline-inf").css('transition', 'all .05s ease-in-out');
        $("#outline-inf").css('-moz-transition', 'all .05s ease-in-out');
        $("#outline-inf").css('-webkit-transition', 'all .05s ease-in-out');
        $("#outline-inf").css('z-index', '9999999999999999999999999');

        $("#outline-left").css('display', "block");
        $("#outline-left").css('height', height + "px");
        $("#outline-left").css('width', "2px");
        $("#outline-left").css('position', 'fixed');
        $("#outline-left").css('top', posY);
        $("#outline-left").css('left', posX);
        $("#outline-left").css('background-color', '#a4c6fd');
        $("#outline-left").css('transition', 'all .05s ease-in-out');
        $("#outline-left").css('-moz-transition', 'all .05s ease-in-out');
        $("#outline-left").css('-webkit-transition', 'all .05s ease-in-out');
        $("#outline-left").css('z-index', '9999999999999999999999999');

        $("#outline-right").css('display', "block");
        $("#outline-right").css('height', height + "px");
        $("#outline-right").css('width', "2px");
        $("#outline-right").css('position', 'fixed');
        $("#outline-right").css('top', posY);
        $("#outline-right").css('left', posX + width);
        $("#outline-right").css('background-color', '#a4c6fd');
        $("#outline-right").css('transition', 'all .05s ease-in-out');
        $("#outline-right").css('-moz-transition', 'all .05s ease-in-out');
        $("#outline-right").css('-webkit-transition', 'all .05s ease-in-out');
        $("#outline-right").css('z-index', '9999999999999999999999999');

    }
    else {
        outlineHide();
        console.info("element not found select");
    }
}

var outlineHide = function () {

    $("#outline-data").css('display', "none");
    $("#outline-sup").css('display', "none");
    $("#outline-inf").css('display', "none");
    $("#outline-right").css('display', "none");
    $("#outline-left").css('display', "none");

}

var initTuningPanel = function () {

    // First load the tuning config from file
    if (tuningConfig) {
        console.info("Tuning config from file is loaded");
    }
    else {
        console.info("tuning config not found");
    }

    // Add tuning from HTML 5 data tags
    $("[data-tuning]").each(function (index, value) {
        var tagName = $(value).data("tuning") ? $(value).data("tuning") : $(value)[0].nodeName.toLowerCase();
        var tagSchema = $(value).data("schema") ? $(value).data("schema") : $(value)[0].nodeName.toLowerCase();
        var tagSelector = $(value).data("selector") ? $(value).data("selector") : tagSchema;
        var tagEditors = $(value).data("editors"); //JSON.parse(...);

        tuningConfig.configs.splice(tuningConfig.configs.length, 0, {
            name: tagName,
            schema: tagSchema,
            selector: tagSelector,
            editors: tagEditors
        });
    });
    console.info("HTML5 tags");

    // For each editor config create a composite alias
    $.each(tuningConfig.configs, function (configIndex, config) {
        $.each(config.editors, function (editorIndex, editor) {
            var clearSchema = config.schema.replace(/[^a-zA-Z0-9]+/g, '').toLowerCase();
            var clearEditor = JSON.stringify(editor).replace(/[^a-zA-Z0-9]+/g, '').toLowerCase();
            editor.alias = clearSchema + clearEditor;
        });
    });
    console.info("Alias tags");

    // Create or update the less file
    $.ajax({
        url: "/Umbraco/Api/tuning/Init",
        type: 'POST',
        dataType: "json",
        error: function (err) {
            alert(err.responseText)
        },
        data: {
            config: JSON.stringify(tuningConfig),
            pageId: pageId
        },
        success: function (data) {

            // Add Less link in head
            $("head").append("<link>");
            css = $("head").children(":last");
            css.attr({
                rel: "stylesheet/less",
                type: "text/css",
                href: data
            });
            console.info("Less styles are loaded");

            // Init Less.js
            $.getScript("/Umbraco/lib/Less/less-1.7.0.min.js", function (data, textStatus, jqxhr) {

                // Init panel 
                if (parent.setFrameIsLoaded) {
                    parent.setFrameIsLoaded(tuningConfig, tuningPalette);
                }
            });
        }
    });

}

$(function () {

    if (parent.setFrameIsLoaded) {

        // Init ouline layer
        $("body").append("<div id=\"outline-data\"></div><div id=\"outline-sup\"></div><div id=\"outline-inf\"></div><div id=\"outline-left\"></div><div id=\"outline-right\"></div>");

        // Init tuning panel
        initTuningPanel();
    }

});

