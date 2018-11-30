function ExamineManagementController($scope, umbRequestHelper, $http, $q, $timeout) {

    var vm = this;

    vm.indexerDetails = [];
    vm.searcherDetails = [];
    vm.loading = true;
    vm.viewState = "list";
    vm.selectedIndex = null;

    vm.showIndexInfo = showIndexInfo;
    vm.showSearcherInfo = showSearcherInfo;
    vm.search = search;
    vm.toggle = toggle;
    vm.rebuildIndex = rebuildIndex;
    vm.closeSearch = closeSearch;
    vm.setViewState = setViewState;
    

    vm.infoOverlay = null;

    function setViewState(state) {
        vm.viewState = state;
    }

    function showIndexInfo(index) {
        vm.selectedIndex = index;
        setViewState("index-details");
    }

    function showSearcherInfo(searcher) {
        vm.selectedSearcher = searcher;
        setViewState("searcher-details");
    }

    function checkProcessing(index, checkActionName) {
        umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("examineMgmtBaseUrl",
                    checkActionName,
                    { indexName: index.name })),
                'Failed to check index processing')
            .then(function(data) {

                if (data !== null && data !== "null") {

                    //copy all resulting properties
                    for (var k in data) {
                        index[k] = data[k];
                    }
                    index.isProcessing = false;
                } else {
                    $timeout(function() {
                            //don't continue if we've tried 100 times
                            if (index.processingAttempts < 100) {
                                checkProcessing(index, checkActionName);
                                //add an attempt
                                index.processingAttempts++;
                            } else {
                                //we've exceeded 100 attempts, stop processing
                                index.isProcessing = false;
                            }
                        },
                        1000);
                }
            });
    }

    function search(searcher, e) {
        if (e && e.keyCode !== 13) {
            return;
        }

        umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("examineMgmtBaseUrl",
                    "GetSearchResults",
                    {
                        searcherName: searcher.name,
                        query: encodeURIComponent(searcher.searchText),
                        queryType: searcher.searchType
                    })),
                'Failed to search')
            .then(function(searchResults) {
                searcher.isSearching = true;
                searcher.searchResults = searchResults;
            });
    }

    function toggle(provider, propName) {
        if (provider[propName] !== undefined) {
            provider[propName] = !provider[propName];
        } else {
            provider[propName] = true;
        }
    }

    function rebuildIndex(index) {
        if (confirm("This will cause the index to be rebuilt. " +
            "Depending on how much content there is in your site this could take a while. " +
            "It is not recommended to rebuild an index during times of high website traffic " +
            "or when editors are editing content.")) {

            index.isProcessing = true;
            index.processingAttempts = 0;

            umbRequestHelper.resourcePromise(
                    $http.post(umbRequestHelper.getApiUrl("examineMgmtBaseUrl",
                        "PostRebuildIndex",
                        { indexName: index.name })),
                    'Failed to rebuild index')
                .then(function() {

                    //rebuilding has started, nothing is returned accept a 200 status code.
                    //lets poll to see if it is done.
                    $timeout(function() {
                            checkProcessing(index, "PostCheckRebuildIndex");
                        },
                        1000);

                });
        }
    }

    function closeSearch(searcher) {
        searcher.isSearching = true;
    }

    function init() {
        //go get the data

        //combine two promises and execute when they are both done
        $q.all([

                //get the indexer details
                umbRequestHelper.resourcePromise(
                    $http.get(umbRequestHelper.getApiUrl("examineMgmtBaseUrl", "GetIndexerDetails")),
                    'Failed to retrieve indexer details')
                .then(function (data) {
                    vm.indexerDetails = data;
                }),

                //get the searcher details
                umbRequestHelper.resourcePromise(
                    $http.get(umbRequestHelper.getApiUrl("examineMgmtBaseUrl", "GetSearcherDetails")),
                    'Failed to retrieve searcher details')
                .then(function (data) {
                    vm.searcherDetails = data;
                    for (var s in vm.searcherDetails) {
                        vm.searcherDetails[s].searchType = "text";
                    }
                })
            ])
            .then(function () {
                //all init loading is complete
                vm.loading = false;
            });
    }

    init();
}

angular.module("umbraco").controller("Umbraco.Dashboard.ExamineManagementController", ExamineManagementController);
