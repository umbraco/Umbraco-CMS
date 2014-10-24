/*********************************************************************************************************/
/* Global function and variable for panel/page com */
/*********************************************************************************************************/

var currentTarget = undefined;

var refreshLayout = function (parameters) {

    // hide preview badget
    $("#umbracoPreviewBadge").hide();

    var string = "less.modifyVars({" + parameters.join(",") + "})";
    eval(string);
}

/* Fonts loaded in the Canvasdesigner panel need to be loaded independently in
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

var closeIntelCanvasdesigner = function (canvasdesignerModel) {

    if (canvasdesignerModel) {

        $.each(canvasdesignerModel.configs, function (indexConfig, config) {
            if (config.schema) {
                $(config.schema).unbind();
                $(config.schema).removeAttr("canvasdesigner-over");
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

var initIntelCanvasdesigner = function (canvasdesignerModel) {

    if (canvasdesignerModel) {

        // Add canvasdesigner-over attr for each schema from config
        $.each(canvasdesignerModel.configs, function (indexConfig, config) {
            var schema = config.selector ? config.selector : config.schema;
            if (schema) {
                $(schema).attr("canvasdesigner-over", config.schema);
                $(schema).attr("canvasdesigner-over-name", config.name);
                $(schema).css("cursor", "default");
            }
        });

        // Outline canvasdesigner-over
        $(document).mousemove(function (e) {

            e.stopPropagation();

            var target = $(e.target);
            while (target.length > 0 && (target.attr('canvasdesigner-over') == undefined || target.attr('canvasdesigner-over') == '')) {
                target = target.parent();
            }

            if (target.attr('canvasdesigner-over') != undefined && target.attr('canvasdesigner-over') != '') {
                target.unbind();
                outlinePosition(target);

                parent.onMouseoverCanvasdesignerItem(target.attr('canvasdesigner-over-name'), target);

                target.click(function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    //console.info(target.attr('canvasdesigner-over'));

                    currentTarget = target;
                    outlineSelected();

                    parent.onClickCanvasdesignerItem(target.attr('canvasdesigner-over'), target);
                    return false;
                });
            }
            else {
                outlinePositionHide();
            }
        });

    }

}

var refreshOutlinePosition = function(schema) {
    outlinePosition($(schema));
}

var outlinePosition = function (oTarget) {

    var target = oTarget;

    if (target.length > 0 && target.attr('canvasdesigner-over') != undefined && target.attr('canvasdesigner-over') != '') {

        var localname = target[0].localName;
        var height = $(target).outerHeight();
        var width = $(target).outerWidth();
        var position = $(target).offset();
        var posY = position.top ;
        //$(window).scrollTop();
        var posX = position.left;
        //+ $(window).scrollLeft();

        $(".canvasdesigner-overlay").css('display', 'block');
        $(".canvasdesigner-overlay").css('left', posX);
        $(".canvasdesigner-overlay").css('top', posY);
        $(".canvasdesigner-overlay").css('width', width + "px");
        $(".canvasdesigner-overlay").css('height', height + "px");

        //console.info("element select " + localname);
        $(".canvasdesigner-overlay span").html(target.attr('canvasdesigner-over-name'));

    }
    else {
        outlinePositionHide();
        //console.info("element not found select");
    }
}

var refreshOutlineSelected = function (schema) {
    outlineSelected($(schema));
}

var outlineSelected = function (oTarget) {

    var target = currentTarget;

    if (oTarget) {
        currentTarget = oTarget;
        target = oTarget;
    }

    if (target && target.length > 0 && target.attr('canvasdesigner-over') != undefined && target.attr('canvasdesigner-over') != '') {

        var localname = target[0].localName;
        var height = $(target).outerHeight();
        var width = $(target).outerWidth();
        var position = $(target).offset();
        var posY = position.top;
        //$(window).scrollTop();
        var posX = position.left;
        //+ $(window).scrollLeft();

        $(".canvasdesigner-overlay-selected").css('display', 'block');
        $(".canvasdesigner-overlay-selected").css('left', posX);
        $(".canvasdesigner-overlay-selected").css('top', posY);
        $(".canvasdesigner-overlay-selected").css('width', width + "px");
        $(".canvasdesigner-overlay-selected").css('height', height + "px");

        //console.info("element select " + localname);
        $(".canvasdesigner-overlay-selected span").html(target.attr('canvasdesigner-over-name'));

    }
    else {
        outlinePositionHide();
        //console.info("element not found select");
    }

}

var outlinePositionHide = function () {
    $(".canvasdesigner-overlay").css('display', "none");
}

var outlineSelectedHide = function () {
    currentTarget = undefined;
    $(".canvasdesigner-overlay-selected").css('display', "none");
}

var initCanvasdesignerPanel = function () {

    $('link[data-title="canvasdesignerCss"]').attr('disabled', 'disabled');

    // First load the canvasdesigner config from file
    if (!canvasdesignerConfig) {
        console.info("canvasdesigner config not found");
    }

    // Add canvasdesigner from HTML 5 data tags
    $("[data-canvasdesigner]").each(function (index, value) {
        var tagName = $(value).data("canvasdesigner") ? $(value).data("canvasdesigner") : $(value)[0].nodeName.toLowerCase();
        var tagSchema = $(value).data("schema") ? $(value).data("schema") : $(value)[0].nodeName.toLowerCase();
        var tagSelector = $(value).data("selector") ? $(value).data("selector") : tagSchema;
        var tagEditors = $(value).data("editors"); //JSON.parse(...);
        canvasdesignerConfig.configs.splice(canvasdesignerConfig.configs.length, 0, {
            name: tagName,
            schema: tagSchema,
            selector: tagSelector,
            editors: tagEditors
        });
    });

    // For each editor config create a composite alias
    $.each(canvasdesignerConfig.configs, function (configIndex, config) {
        if (config.editors) {
            $.each(config.editors, function (editorIndex, editor) {
                var clearSchema = config.schema.replace(/[^a-zA-Z0-9]+/g, '').toLowerCase();
                var clearEditor = JSON.stringify(editor).replace(/[^a-zA-Z0-9]+/g, '').toLowerCase();
                editor.alias = clearSchema + clearEditor;
            });
        }
    });

    // Create or update the less file
    $.ajax({
        url: "/Umbraco/Api/CanvasDesigner/Init",
        type: 'POST',
        dataType: "json",
        error: function (err) {
            alert(err.responseText)
        },
        data: {
            config: JSON.stringify(canvasdesignerConfig),
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
                    parent.setFrameIsLoaded(canvasdesignerConfig, canvasdesignerPalette);
                }
            });
        }
    });

}

$(function () {

    if (parent.setFrameIsLoaded) {

        // Overlay background-color: rgba(28, 203, 255, 0.05);
        $("body").append("<div class=\"canvasdesigner-overlay\" style=\"display:none; pointer-events: none; position: absolute; z-index: 9999; border: 1px solid #2ebdff; border-radius: 3px; \"><span style=\"position:absolute;background: #2ebdff; font-family: Helvetica, Arial, sans-serif; color: #fff; padding: 0 5px 0 6px; font-size: 10px; line-height: 17px; display: inline-block; border-radius: 0 0 3px 0;\"></span></div>");
        $("body").append("<div class=\"canvasdesigner-overlay-selected\" style=\"display:none; pointer-events: none; position: absolute; z-index: 9998; border: 2px solid #2ebdff; border-radius: 3px;\"><span style=\"position:absolute;background: #2ebdff; font-family: Helvetica, Arial, sans-serif; color: #fff; padding: 0 5px; font-size: 10px; line-height: 16px; display: inline-block; border-radius: 0 0 3px 0;\"></span></div>");

        // Set event for any body click
        initBodyClickEvent()

        // Init canvasdesigner panel
        initCanvasdesignerPanel();
    }

});
