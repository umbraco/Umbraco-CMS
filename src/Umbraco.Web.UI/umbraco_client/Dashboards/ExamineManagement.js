Umbraco.Sys.registerNamespace("Umbraco.Dashboards");

(function ($) {

    Umbraco.Dashboards.ExamineManagement = base2.Base.extend({
        
        //private methods/variables
        _opts: null,
        _koViewModel: null,

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
                summary: ko.observable(""),
                loading: ko.observable(false)
                //publishAll: ko.observable(false),
                //includeUnpublished: ko.observable(false)
            };
            
            ko.applyBindings(self._koViewModel, self._opts.container.get(0));

            this.loadSummary();
        },
        
        loadSummary: function() {
            var self = this;
            self._koViewModel.loading(true);

            $.get(self._opts.restServiceLocation + "Index",
                function(e) {
                    self._koViewModel.loading(false);
                    self._koViewModel.summary(e);
                }, "html").fail(function(a, b, c) {
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