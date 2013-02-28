Umbraco.Sys.registerNamespace("Umbraco.Dashboards");

(function ($) {

    Umbraco.Dashboards.ExamineManagement = base2.Base.extend({

        //private methods/variables
        _opts: null,
        _koViewModel: null,

        _mapSearcherModelProperties: function (indexerModel) {
            var self = this;
            
            var viewModel = self._mapBaseProviderModelProperties(indexerModel);

            //add custom searcher props
            viewModel.searchText = ko.observable("");
            viewModel.searchType = ko.observable("text");
            viewModel.isSearching = ko.observable(false);
            viewModel.closeSearch = function() {
                this.isSearching(false);
            };
            //add flag properties to determine if either button has been pressed
            viewModel.isProcessing = ko.observable(false);
            //don't need an observable array since it does not change, just need an observable to hold an array.
            viewModel.searchResults = ko.observable([]);
            viewModel.search = function () {
                //NOTE: 'this' is the ko view model
                this.isSearching(true);
                self._doSearch(this);
            };
            viewModel.handleEnter = function(vm, event) {
                var keyCode = (event.which ? event.which : event.keyCode);
                if (keyCode === 13) {
                    vm.search();
                    return false;
                }
                return true;
            };

            return viewModel;
        },

        _mapBaseProviderModelProperties : function(indexerModel) {
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
            viewModel.showProviderProperties = ko.observable(false);
            viewModel.toggleProviderProperties = function () {
                this.showProviderProperties(!this.showProviderProperties());
            };
            viewModel.showTools = ko.observable(false);
            viewModel.toggleTools = function () {
                this.showTools(!this.showTools());
            };

            return viewModel;
        },

        _mapIndexerModelProperties: function (indexerModel, viewModel) {
            var self = this;

            //if we are updating the view model or creating it for the first time.
            var isUpdate = viewModel != null;

            if (!isUpdate) {
                //do the ko auto-mapping to the new object and create additional properties
                
                viewModel = self._mapBaseProviderModelProperties(indexerModel);
                
                //property to track how many attempts have been made to check if the index is optimized or rebuilt
                viewModel.processingAttempts = ko.observable(0);
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
                //add flag properties to determine if either button has been pressed
                viewModel.isProcessing = ko.observable(false);
                //add the button methods
                viewModel.rebuildIndex = function () {
                    if (confirm("This will cause the index to be rebuilt. " +
                        "Depending on how much content there is in your site this could take a while. " +
                        "It is not recommended to rebuild an index during times of high website traffic " +
                        "or when editors are editing content.")) {
                        //NOTE: 'this' is the knockoutjs model that is bound
                        self._doProcessing(this.Name(), this, "PostRebuildIndex", "PostCheckRebuildIndex");
                    }
                };
                viewModel.optimizeIndex = function () {
                    if (confirm("This will cause the index to be optimized which will improve its performance. " +
                        "It is not recommended to optimize an index during times of high website traffic " +
                        "or when editors are editing content.")) {
                        //NOTE: 'this' is the knockoutjs model that is bound
                        self._doProcessing(this.Name(), this, "PostOptimizeIndex", "PostCheckOptimizeIndex");
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
        
        _doSearch: function(viewModel) {
            var self = this;
            viewModel.isProcessing(true);
            $.get(self._opts.restServiceLocation + "GetSearchResults?searcherName=" + viewModel.Name() + "&query=" + viewModel.searchText() + "&queryType=" + viewModel.searchType(),
                function(searchResults) {
                    viewModel.isProcessing(false);
                    //re-map the fields dictionary to array
                    for (var s in searchResults) {
                        searchResults[s].Fields = self._mapDictionaryToArray(searchResults[s].Fields);
                    }                    
                    viewModel.searchResults(searchResults);
                }).fail(function(a, b, c) {
                    alert(b + ": " + a.responseText);
                });
        },

        _doProcessing: function (name, viewModel, processingActionName, pollActionName) {
            var self = this;
            viewModel.isProcessing(true); //set the model processing

            $.post(self._opts.restServiceLocation + processingActionName + "?indexerName=" + name,
                function (data) {
                    
                    //optimization has started, nothing is returned accept a 200 status code.
                    //lets poll to see if it is done.
                    setTimeout(function() {
                        self._checkProcessing(name, viewModel, pollActionName);
                    }, 1000);

                }).fail(function (a, b, c) {
                    alert(b + ": " + a.responseText);
                });
        },
        
        _checkProcessing: function (name, viewModel, actionName) {
            var self = this;

            $.post(self._opts.restServiceLocation + actionName + "?indexerName=" + name,
                function (data) {
                    if (data) {
                        //success! now, we need to re-update the whole indexer model
                        self._mapIndexerModelProperties(data, viewModel);
                        viewModel.isProcessing(false);
                    }
                    else {
                        //copy local from closure
                        var vm = viewModel;
                        var an = actionName;
                        setTimeout(function () {
                            //don't continue if we've tried 100 times
                            if (vm.processingAttempts() < 100) {
                                self._checkProcessing(name, vm, an);
                                //add an attempt
                                vm.processingAttempts(vm.processingAttempts() + 1);
                            }
                            else {
                                //we've exceeded 100 attempts, stop processing
                                viewModel.isProcessing(false);
                            }
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