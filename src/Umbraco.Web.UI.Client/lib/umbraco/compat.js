/* contains random bits and pieces we neede to make the U6 UI behave */
jQuery(document).ready(function () {
    scaleScrollables("body");
    
    jQuery(window).bind("resize", function () {
        scaleScrollables("body");
    });

    jQuery("body").click(function(event) {
        var el = event.target.nodeName;
        var pEl = event.target.parentElement.nodeName;

        if(el != "A" && el != "BUTTON" && pEl != "A" && pEl != "BUTTON"){
            UmbClientMgr.closeModalWindow(undefined);
        }
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