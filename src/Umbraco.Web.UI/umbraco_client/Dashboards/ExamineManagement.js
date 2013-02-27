Umbraco.Sys.registerNamespace("Umbraco.Dashboards");

(function ($) {

    Umbraco.Dashboards.ExamineManagement = base2.Base.extend({

        //private methods/variables
        _opts: null,
        _koViewModel: null,

        _mapSearcherModelProperties : function(indexerModel) {
            var self = this;
            
            //do the ko auto-mapping
            var viewModel = ko.mapping.fromJS(indexerModel);
            
            //need to re-map the dictionary to an array so we can bind to it
            //we don't need to have an observable array since we're never adding/removing, just setting the object
            viewModel.ProviderProperties = ko.observable(self._mapDictionaryToArray(indexerModel.ProviderProperties));

            //add toggle and show properties 
            //(NOTE that 'this' inside of these functions is actually the knockoutjs model that is bound)
            viewModel.showProperties = ko.observable(false);
            viewModel.toggleProperties = function () {
                this.showProperties(!this.showProperties());
            };

            return viewModel;
        },

        _mapIndexerModelProperties: function (indexerModel, viewModel) {
            var self = this;

            //if we are updating the view model or creating it for the first time.
            var isUpdate = viewModel != null;

            if (!isUpdate) {
                //do the ko auto-mapping to the new object and create additional properties
                
                viewModel = self._mapSearcherModelProperties(indexerModel);
                
                //add a hasDeletions prop
                viewModel.hasDeletions = ko.observable(indexerModel.DeletionCount > 0);
                //add toggle and show properties 
                //(NOTE that 'this' inside of these functions is actually the knockoutjs model that is bound)            
                viewModel.showSystemFields = ko.observable(false);
                viewModel.toggleSystemFields = function () {
                    this.showSystemFields(!this.showSystemFields());
                };
                viewModel.showUserFields = ko.observable(false);
                viewModel.toggleUserFields = function () {
                    this.showUserFields(!this.showUserFields());
                };
                viewModel.showNodeTypes = ko.observable(false);
                viewModel.toggleNodeTypes = function () {
                    this.showNodeTypes(!this.showNodeTypes());
                };
                viewModel.showProviderProperties = ko.observable(false);
                viewModel.toggleProviderProperties = function () {
                    this.showProviderProperties(!this.showProviderProperties());
                };
                viewModel.showIndexTools = ko.observable(false);
                viewModel.toggleIndexTools = function () {
                    this.showIndexTools(!this.showIndexTools());
                };
                //add flag properties to determine if either button has been pressed
                viewModel.isProcessing = ko.observable(false);
                //add the button methods
                viewModel.rebuildIndex = function () {
                    if (confirm("This will cause the index to be rebuilt. " +
                        "Depending on how much content there is in your site this could take a while. " +
                        "It is not recommended to rebuild an index during times of high website traffic " +
                        "or when editors are editing content.")) {

                    }
                };
                viewModel.optimizeIndex = function () {
                    if (confirm("This will cause the index to be optimized which will improve its performance. " +
                        "It is not recommended to optimize an index during times of high website traffic " +
                        "or when editors are editing content.")) {
                        //NOTE: 'this' is the knockoutjs model that is bound
                        self._optimizeIndex(indexerModel.Name, this);
                    }
                };
            }
            else {
                //update it with new data
                ko.mapping.fromJS(indexerModel, viewModel);
            }
            
            //whether we are updating or creating we always execute this logic...

            //change the include/exclude node types to say something different if they are empty
            viewModel.IndexCriteria.IncludeNodeTypes(indexerModel.IndexCriteria.IncludeNodeTypes.join());
            viewModel.IndexCriteria.ExcludeNodeTypes(indexerModel.IndexCriteria.ExcludeNodeTypes.join());
            if (viewModel.IndexCriteria.IncludeNodeTypes() == "")
                viewModel.IndexCriteria.IncludeNodeTypes("Include all");
            if (viewModel.IndexCriteria.ExcludeNodeTypes() == "")
                viewModel.IndexCriteria.ExcludeNodeTypes("Exclude none");

            return viewModel;
        },

        _optimizeIndex: function (name, viewModel) {
            var self = this;
            viewModel.isProcessing(true); //set the model processing

            $.post(self._opts.restServiceLocation + "PostOptimizeIndex?indexerName=" + name,
                function (data) {
                    
                    //optimization has started, nothing is returned accept a 200 status code.
                    //lets poll to see if it is done.
                    setTimeout(function() {
                        self._checkOptimizeIndex(name, viewModel);
                    }, 1000);

                }).fail(function (a, b, c) {
                    alert(b + ": " + a.responseText);
                });
        },
        
        _checkOptimizeIndex: function (name, viewModel) {
            var self = this;

            $.post(self._opts.restServiceLocation + "PostCheckOptimizeIndex?indexerName=" + name,
                function (data) {
                    if (data) {
                        //success! now, we need to re-update the whole indexer model
                        self._mapIndexerModelProperties(data, viewModel);
                        viewModel.isProcessing(false);
                    }
                    else {
                        setTimeout(function () {
                            self._checkOptimizeIndex(name);
                        }, 1000);
                    }
                }).fail(function (a, b, c) {
                    alert(b + ": " + a.responseText);
                });
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

        _loadDetails: function () {
            var self = this;
            self._koViewModel.loading(true);

            var loadingCount = 2;

            $.get(self._opts.restServiceLocation + "GetIndexerDetails",
                function (e) {
                    if (--loadingCount == 0) {
                        self._koViewModel.loading(false);
                    }

                    //loop through each indexer in the array and remap its properties
                    for (var item in e) {
                        e[item] = self._mapIndexerModelProperties(e[item]);                        
                    }
                    
                    self._koViewModel.indexerDetails(e);
                }).fail(function (a, b, c) {
                    alert(b + ": " + a.responseText);
                });
            
            $.get(self._opts.restServiceLocation + "GetSearcherDetails",
                function (e) {
                    if (--loadingCount == 0) {
                        self._koViewModel.loading(false);
                    }

                    for (var item in e) {
                        e[item] = self._mapSearcherModelProperties(e[item]);
                    }

                    self._koViewModel.searcherDetails(e);
                }).fail(function (a, b, c) {
                    alert(b + ": " + a.responseText);
                });
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

            this._loadDetails();
        }

    });


    //Set defaults for jQuery ajax calls.
    $.ajaxSetup({
        dataType: 'json',
        cache: false,
        contentType: 'application/json; charset=utf-8'
    });

})(jQuery);