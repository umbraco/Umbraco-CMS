/* contains random bits and pieces we neede to make the U6 UI behave */
jQuery(document).ready(function () {
    scaleScrollables("body");
    
    jQuery(window).bind("resize", function () {
        scaleScrollables("body");
    });
});


function scaleScrollables(selector){
	jQuery(".umb-scrollable").each(function () {
            var el = jQuery(this);
            var totalOffset = 0;
            var offsety = el.data("offset-y");
            
            if (offsety != undefined)
                totalOffset += offsety;

            el.height($(window).height() - (el.offset().top + totalOffset));
        });
}