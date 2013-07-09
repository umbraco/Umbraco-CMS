/* contains random bits and pieces we neede to make the U6 UI behave */

Umbraco.Sys.registerNamespace("Umbraco.Application.LegacyHelper");

(function ($) {
    

    $(document).ready(function () {
        Umbraco.Application.LegacyHelper.scaleScrollables("body");

        $(window).bind("resize", function () {
            Umbraco.Application.LegacyHelper.scaleScrollables("body");
        });

        $("body").click(function (event) {
            var el = event.target.nodeName;
            var pEl = event.target.parentElement.nodeName;

            if (el != "A" && el != "BUTTON" && pEl != "A" && pEl != "BUTTON") {
                UmbClientMgr.closeModalWindow(undefined);
            }
        });
    });     

    /** Static helper class  */
    Umbraco.Application.LegacyHelper = {

        scaleScrollables: function (selector) {
            $(".umb-scrollable").each(function() {
                var el = jQuery(this);
                var totalOffset = 0;
                var offsety = el.data("offset-y");

                if (offsety != undefined)
                    totalOffset += offsety;

                el.height($(window).height() - (el.offset().top + totalOffset));
            });
        }
    };
    
})(jQuery);