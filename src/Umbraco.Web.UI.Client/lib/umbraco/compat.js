/* contains random bits and pieces we neede to make the U6 UI behave */

(function ($) {

    $(document).ready(function () {
        scaleScrollables("body");

        $(window).bind("resize", function () {
            scaleScrollables("body");
        });

        $("body").click(function (event) {
            var el = event.target.nodeName;
            var els = ["INPUT","A","BUTTON"];

            if(els.indexOf(el) >= 0){return;}

            var parents = $(event.target).parents("a,button");
            if(parents.length > 0){
                return;
            }

            var click = $(event.target).attr('onClick');
            if(click){
                return;
            }


            UmbClientMgr.closeModalWindow(undefined);
        });

        $.ctrl("S", function(){
            var link = $(".umb-panel-header .btn-primary");
           
            //this is made of bad, to work around webforms horrible wiring
            if(!link.hasClass("client-side") && link.attr("href").indexOf("javascript:") == 0){
                eval(link.attr('href').replace('javascript:',''));
            }else{
                link.click();
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
    
    $.ctrl = function(key, callback, args) {
        var isMac = navigator.platform.toUpperCase().indexOf('MAC')>=0;
        var isCtrl = false;

        $(document).keydown(function(e) {
            if(!args) args=[]; // IE barks when args is null
            var modKey = isMac ? e.metaKey : e.ctrlKey;
            //if(modKey){
            //  isCtrl = true;  
            //} 

            if(modKey && e.keyCode == key.charCodeAt(0)) {
                callback.apply(this, args);
                return false;
            }

        });

        /*.keyup(function(e) {
            var modKey = isMac ? e.metaKey : e.ctrlKey;
            if(modKey){
                isCtrl = false;
            }
        }); */       
    };
})(jQuery);