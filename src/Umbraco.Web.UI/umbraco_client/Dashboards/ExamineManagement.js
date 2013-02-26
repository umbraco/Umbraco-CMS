Umbraco.Sys.registerNamespace("Umbraco.Dashboards");

(function ($) {

    Umbraco.Dashboards.ExamineManagement = base2.Base.extend({

        //private methods/variables
        _opts: null,
        _koViewModel: null,

        _mapDictionaryToArray: function (dictionary) {
            var result = [];
            for (var key in dictionary) {
                if (dictionary.hasOwnProperty(key)) {
                    result.push({ key: key, value: dictionary[key] });
                }
            }
            return result;
        },

        // Constructor
        constructor: function (opts) {
            // Merge options with default
            this._opts = $.extend({
                container: $("#examineManagement")
            }, opts);
        },

        //public methods/variables

        init: function () {
            var self = this;

            //The knockout js view model for the selected item
            self._koViewModel = {
                indexerDetails: ko.observable(""),
                loading: ko.observable(false),
                expandIndex: function () {

                }
                //publishAll: ko.observable(false),
                //includeUnpublished: ko.observable(false)
            };

            ko.applyBindings(self._koViewModel, self._opts.container.get(0));

            this.loadDetails();
        },

        loadDetails: function () {
            var self = this;
            self._koViewModel.loading(true);

            $.get(self._opts.restServiceLocation + "GetIndexerDetails",
                function (e) {
                    self._koViewModel.loading(false);

                    for (var item in e) {
                        //need to re-map the dictionary to an array so we can bind to it
                        e[item].IndexerProperties = self._mapDictionaryToArray(e[item].IndexerProperties);
                        //add a showProperties property to the object
                        e[item].showProperties = ko.observable(false);
                        //add a toggleProperties method
                        e[item].toggleProperties = function () {
                            this.showProperties(!this.showProperties());
                        };
                        //change the include/exclude node types to say something different if they are empty
                        e[item].IndexCriteria.IncludeNodeTypes = e[item].IndexCriteria.IncludeNodeTypes.join();
                        e[item].IndexCriteria.ExcludeNodeTypes = e[item].IndexCriteria.ExcludeNodeTypes.join();
                        if (e[item].IndexCriteria.IncludeNodeTypes == "")
                            e[item].IndexCriteria.IncludeNodeTypes = "Include all";
                        if (e[item].IndexCriteria.ExcludeNodeTypes == "")
                            e[item].IndexCriteria.ExcludeNodeTypes = "Exclude none";
                        //change the Standard and user fields to be an array so we can bind it
                        //e[item].IndexCriteria.StandardFields = self._mapDictionaryToArray(e[item].IndexerProperties);
                    }
                    
                    self._koViewModel.indexerDetails(e);
                }).fail(function (a, b, c) {
                    alert("error: " + b);
                });
        }

        //doSubmit: function () {
        //    /// <summary>Submits the data to the server for saving</summary>
        //    var codeVal = UmbClientMgr.contentFrame().UmbEditor.GetCode();
        //    var self = this;

        //    if (this._opts.editorType == "Template") {
        //        //saving a template view

        //        $.post(self._opts.restServiceLocation + "SaveTemplate",
        //            JSON.stringify({
        //                templateName: this._opts.nameTxtBox.val(),
        //                templateAlias: this._opts.aliasTxtBox.val(),
        //                templateContents: codeVal,
        //                templateId: this._opts.templateId,
        //                masterTemplateId: this._opts.masterPageDropDown.val()
        //            }),
        //            function (e) {
        //                if (e.success) {
        //                    self.submitSuccess(e.message, e.header);
        //                } else {
        //                    self.submitFailure(e.message, e.header);
        //                }
        //            });
        //    }
        //},
    });


    //Set defaults for jQuery ajax calls.
    $.ajaxSetup({
        dataType: 'json',
        cache: false,
        contentType: 'application/json; charset=utf-8'
    });

})(jQuery);