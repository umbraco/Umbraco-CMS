/*********************************************************************************************************/
/* Global function and variable for panel/page com */
/*********************************************************************************************************/

var currentTarget = undefined;

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
                //console.log('loading font' + font + ' in iframe');
            },
            active: function () {
                //console.log('loaded font ' + font + ' in iframe');
            },
            inactive: function () {
                //console.log('error loading font ' + font + ' in iframe');
            }
        });
    }
}

var closeIntelTuning = function (tuningModel) {

    if (tuningModel) {

        $.each(tuningModel.configs, function (indexConfig, config) {
            if (config.schema) {
                $(config.schema).unbind();
                $(config.schema).removeAttr("tuning-over");
            }
        });

        initBodyClickEvent();

    }

}

var initBodyClickEvent = function () {
    $("body").on("click", function () {
        if (parent.iframeBodyClick) {
            parent.iframeBodyClick();
        }
    });
}

var initIntelTuning = function (tuningModel) {

    if (tuningModel) {

        // Add tuning-over attr for each schema from config
        $.each(tuningModel.configs, function (indexConfig, config) {
            var schema = config.selector ? config.selector : config.schema;
            if (schema) {
                $(schema).attr("tuning-over", config.name);
                $(schema).css("cursor", "default");
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
                    //console.info(target.attr('tuning-over'));
                    
                    currentTarget = target;
                    outlineSelected();

                    parent.refrechIntelTuning(target.attr('tuning-over'), target);
                    return false;
                });
            }
            else {
                outlinePositionHide();
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
        var posY = position.top ;
        //$(window).scrollTop();
        var posX = position.left;
        //+ $(window).scrollLeft();

        $(".tuning-overlay").css('display', 'block');
        $(".tuning-overlay").css('left', posX);
        $(".tuning-overlay").css('top', posY);
        $(".tuning-overlay").css('width', width + "px");
        $(".tuning-overlay").css('height', height + "px");

        //console.info("element select " + localname);
        $(".tuning-overlay span").html(target.attr('tuning-over'));

    }
    else {
        outlinePositionHide();
        //console.info("element not found select");
    }
}

var outlineSelected = function () {

    var target = currentTarget;

    if (target && target.length > 0 && target.attr('tuning-over') != undefined && target.attr('tuning-over') != '') {

        var localname = target[0].localName;
        var height = $(target).outerHeight();
        var width = $(target).outerWidth();
        var position = $(target).offset();
        var posY = position.top;
        //$(window).scrollTop();
        var posX = position.left;
        //+ $(window).scrollLeft();

        $(".tuning-overlay-selected").css('display', 'block');
        $(".tuning-overlay-selected").css('left', posX);
        $(".tuning-overlay-selected").css('top', posY);
        $(".tuning-overlay-selected").css('width', width + "px");
        $(".tuning-overlay-selected").css('height', height + "px");

        //console.info("element select " + localname);
        $(".tuning-overlay-selected span").html(target.attr('tuning-over'));

    }
    else {
        outlinePositionHide();
        //console.info("element not found select");
    }

}

var outlinePositionHide = function () {
    $(".tuning-overlay").css('display', "none");
}

var outlineSelectedHide = function () {
    currentTarget = undefined;
    $(".tuning-overlay-selected").css('display', "none");
}

var initTuningPanel = function () {

    // First load the tuning config from file
    if (tuningConfig) {
        //console.info("Tuning config from file is loaded");
    }
    else {
        //console.info("tuning config not found");
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
    //console.info("HTML5 tags");

    // For each editor config create a composite alias
    $.each(tuningConfig.configs, function (configIndex, config) {
        $.each(config.editors, function (editorIndex, editor) {
            var clearSchema = config.schema.replace(/[^a-zA-Z0-9]+/g, '').toLowerCase();
            var clearEditor = JSON.stringify(editor).replace(/[^a-zA-Z0-9]+/g, '').toLowerCase();
            editor.alias = clearSchema + clearEditor;
        });
    });
    //console.info("Alias tags");

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
            //console.info("Less styles are loaded");

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

        // Overlay background-color: rgba(28, 203, 255, 0.05); 
        $("body").append("<div class=\"tuning-overlay\" style=\"display:none; pointer-events: none; position: absolute; z-index: 9999; border: 1px solid #2ebdff; border-radius: 3px; \"><span style=\"position:absolute;background: #2ebdff; font-family: Helvetica, Arial, sans-serif; color: #fff; padding: 0 5px; font-size: 10px; line-height: 16px; display: inline-block; border-radius: 0 0 3px 0;\"></span></div>");
        $("body").append("<div class=\"tuning-overlay-selected\" style=\"display:none; pointer-events: none; position: absolute; z-index: 9998; border: 2px solid #2ebdff; border-radius: 3px;\"><span style=\"position:absolute;background: #2ebdff; font-family: Helvetica, Arial, sans-serif; color: #fff; padding: 0 5px; font-size: 10px; line-height: 16px; display: inline-block; border-radius: 0 0 3px 0;\"></span></div>");

        // Set event for any body click
        initBodyClickEvent()

        // Init tuning panel
        initTuningPanel();
    }

});

