Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function ($) {

    Umbraco.Controls.InsertMacroSplitButton = base2.Base.extend({       
        
        //private methods/variables
        _opts: null,
        
        // Constructor
        constructor: function (opts) {
            // Merge options with default
            this._opts = $.extend({
                // Default options go here
            }, opts);
        },
        
        //public methods/variables

        init: function () {
            var self = this;

            //the container is the 'scope' of which to find .sbPlaceHolder's, by default it is null so the scope is the whole page
            var container = this._opts.container != null ? this._opts.container : null;

            //setup the split buttons, find all .sbPlaceHolder's and assign the menu to it which should always
            //be the previous element with the class .sbMenu            
            var splitButtons = $('.sbPlaceHolder', container);
            splitButtons.each(function() {
                var menu = $(this).prev(".sbMenu");
                $(this).find("a.sbLink").splitbutton({ menu: menu });
            });
            
            //assign the click handler to each macro item drop down
            $(".sbMenu .macro-item").click(function () {
                var alias = $(this).attr("rel");
                if ($(this).attr("data-has-params") == "true") {
                    self._opts.openMacroModel.apply(self, [alias]);
                }
                else {
                    self._opts.insertMacroMarkup.apply(self, [alias]);
                }
            });
            
            //assign the callback for the regular insert macro button (not the drop down)
            $(".sbPlaceHolder a.sbLink").click(function() {
                self._opts.openMacroModel.apply(self, []); //call the callback with no alias
            });

            //a fix for scroll issues TODO: put this code in this class and make it flexible (i.e. NOT with ids)
            //applySplitButtonOverflow('mcontainer', 'innerc', 'macroMenu', '.macro-item', 'showMoreMacros');

        }
        
    });

})(jQuery);