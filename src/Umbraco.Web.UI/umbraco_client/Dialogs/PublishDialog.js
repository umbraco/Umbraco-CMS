Umbraco.Sys.registerNamespace("Umbraco.Dialogs");

(function ($) {

    Umbraco.Dialogs.PublishDialog = base2.Base.extend({
        
        //private methods/variables
        _opts: null,
        
        // Constructor
        constructor: function () {
        },

        //public methods

        init: function (opts) {
            /// <summary>Initializes the class and any UI bindings</summary>

            // Merge options with default
            this._opts = $.extend({
                
            }, opts);

            var self = this;

            //The knockout js view model for the selected item
            var koViewModel = {
                publishAll: ko.observable(false),
                includeUnpublished: ko.observable(false),
                processStatus: ko.observable("init"),
                isSuccessful: ko.observable(false),
                closeDialog: function () {
                    UmbClientMgr.closeModalWindow();
                },
                startPublish: function() {
                    this.processStatus("publishing");
                    
                    $.post(self._opts.restServiceLocation + "PublishDocument",
                    JSON.stringify({
                        documentId: self._opts.documentId
                    }),
                    function (e) {
                        if (e.success) {
                            self.submitSuccess(e.message, e.header);
                        } else {
                            self.submitFailure(e.message, e.header);
                        }
                    });
                }
            };
            //ensure includeUnpublished is always false if publishAll is ever false
            koViewModel.publishAll.subscribe(function (newValue) {
                if (newValue === false) {
                    koViewModel.includeUnpublished(false);
                }
            });

            ko.applyBindings(koViewModel);
        }

        
    }, {
        //Static members

        //private methods/variables
        _instance: null,

        // Singleton accessor
        getInstance: function () {
            if (this._instance == null)
                this._instance = new Umbraco.Dialogs.PublishDialog();
            return this._instance;
        }
    });
    
    //Set defaults for jQuery ajax calls.
    $.ajaxSetup({
        dataType: 'json',
        cache: false,
        contentType: 'application/json; charset=utf-8'
    });

})(jQuery);