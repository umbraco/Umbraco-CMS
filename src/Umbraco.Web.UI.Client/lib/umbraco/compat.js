/* contains random bits and pieces we neede to make the U6 UI behave */

(function ($) {
    

    $(document).ready(function () {
        scaleScrollables("body");

        $(window).bind("resize", function () {
            scaleScrollables("body");
        });

        $("body").click(function (event) {
            var el = event.target.nodeName;
            var pEl = event.target.parentElement.nodeName;

            if (el != "INPUT" && el != "A" && el != "BUTTON" && pEl != "A" && pEl != "BUTTON") {
                UmbClientMgr.closeModalWindow(undefined);
            }
        });
    });     

    function scaleScrollables(selector) {
        $(".umb-scrollable").each(function() {
            var el = jQuery(this);
            var totalOffset = 0;
            var offsety = el.data("offset-y");

            if (offsety != undefined)
                totalOffset += offsety;

            el.height($(window).height() - (el.offset().top + totalOffset));
        });
    }
    
})(jQuery);