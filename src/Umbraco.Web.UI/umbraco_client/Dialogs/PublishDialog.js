Umbraco.Sys.registerNamespace("Umbraco.Dialogs");

(function ($) {

    Umbraco.Dialogs.PublishDialog = base2.Base.extend({
        
        //private methods/variables
        _opts: null,
        _koViewModel: null,
        
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
            self._koViewModel = {
                publishAll: ko.observable(false),
                includeUnpublished: ko.observable(false),
                processStatus: ko.observable("init"),
                isSuccessful: ko.observable(false),
                resultMessages: ko.observableArray(),
                resultMessage: ko.observable(""), //if there's only one result message
                closeDialog: function () {
                    UmbClientMgr.closeModalWindow();
                },
                startPublish: function() {
                    this.processStatus("publishing");

                    var includeUnpublished = self._koViewModel.includeUnpublished();
                    $.post(self._opts.restServiceLocation + "PublishDocument",
                    JSON.stringify({
                        documentId: self._opts.documentId,
                        publishDescendants: self._koViewModel.publishAll(),
                        includeUnpublished: includeUnpublished
                    }),
                    function (e) {
                        self._koViewModel.processStatus("complete");
                        self._koViewModel.isSuccessful(e.success);
                        var msgs = e.message.trim().split("\r\n");
                        if (msgs.length > 1) {
                            for (var m in msgs) {
                                self._koViewModel.resultMessages.push({ message: msgs[m] });
                            }
                        }
                        else {
                            self._koViewModel.resultMessage(msgs[0]);
                        }

                        //sync the tree
                        UmbClientMgr.mainTree().setActiveTreeType('content');
                        UmbClientMgr.mainTree().syncTree(self._opts.documentPath, true)
                        if (includeUnpublished) {
                            var node = UmbClientMgr.mainTree().getActionNode();
                            if (node.expanded === true) {
                                UmbClientMgr.mainTree().reloadActionNode();
                            }
                        }
                    });
                }
            };
            //ensure includeUnpublished is always false if publishAll is ever false
            self._koViewModel.publishAll.subscribe(function (newValue) {
                if (newValue === false) {
                    self._koViewModel.includeUnpublished(false);
                }
            });

            ko.applyBindings(self._koViewModel);
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