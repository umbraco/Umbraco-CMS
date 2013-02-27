Umbraco.Sys.registerNamespace("Umbraco.Dashboards");

(function ($) {

    Umbraco.Dashboards.ExamineManagement = base2.Base.extend({

        //private methods/variables
        _opts: null,
        _koViewModel: null,

        _updateProviderProperties : function(provider) {
            //need to re-map the dictionary to an array so we can bind to it
            provider.ProviderProperties = this._mapDictionaryToArray(provider.ProviderProperties);
            //add toggle and show properties
            provider.showProperties = ko.observable(false);
            provider.toggleProperties = function () {
                this.showProperties(!this.showProperties());
            };
            provider.showSystemFields = ko.observable(false);
            provider.toggleSystemFields = function () {
                this.showSystemFields(!this.showSystemFields());
            };
            provider.showUserFields = ko.observable(false);
            provider.toggleUserFields = function () {
                this.showUserFields(!this.showUserFields());
            };
            provider.showNodeTypes = ko.observable(false);
            provider.toggleNodeTypes = function () {
                this.showNodeTypes(!this.showNodeTypes());
            };
            provider.showProviderProperties = ko.observable(false);
            provider.toggleProviderProperties = function () {
                this.showProviderProperties(!this.showProviderProperties());
            };
        },

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
                indexerDetails: ko.observable(null),
                searcherDetails: ko.observable(null),
                loading: ko.observable(false)
            };

            ko.applyBindings(self._koViewModel, self._opts.container.get(0));

            this.loadDetails();
        },

        loadDetails: function () {
            var self = this;
            self._koViewModel.loading(true);

            var loadingCount = 2;

            $.get(self._opts.restServiceLocation + "GetIndexerDetails",
                function (e) {
                    if (--loadingCount == 0) {
                        self._koViewModel.loading(false);
                    }

                    for (var item in e) {
                        self._updateProviderProperties(e[item]);
                        //change the include/exclude node types to say something different if they are empty
                        e[item].IndexCriteria.IncludeNodeTypes = e[item].IndexCriteria.IncludeNodeTypes.join();
                        e[item].IndexCriteria.ExcludeNodeTypes = e[item].IndexCriteria.ExcludeNodeTypes.join();
                        if (e[item].IndexCriteria.IncludeNodeTypes == "")
                            e[item].IndexCriteria.IncludeNodeTypes = "Include all";
                        if (e[item].IndexCriteria.ExcludeNodeTypes == "")
                            e[item].IndexCriteria.ExcludeNodeTypes = "Exclude none";
                    }
                    
                    self._koViewModel.indexerDetails(e);
                }).fail(function (a, b, c) {
                    alert("error: " + b);
                });
            
            $.get(self._opts.restServiceLocation + "GetSearcherDetails",
                function (e) {
                    if (--loadingCount == 0) {
                        self._koViewModel.loading(false);
                    }

                    for (var item in e) {
                        self._updateProviderProperties(e[item]);
                    }

                    self._koViewModel.searcherDetails(e);
                }).fail(function (a, b, c) {
                    alert("error: " + b);
                });
        }

    });


    //Set defaults for jQuery ajax calls.
    $.ajaxSetup({
        dataType: 'json',
        cache: false,
        contentType: 'application/json; charset=utf-8'
    });

})(jQuery);