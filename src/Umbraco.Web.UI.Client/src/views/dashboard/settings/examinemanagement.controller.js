function ExamineManagementController($scope, umbRequestHelper, $http, $q, $timeout) {

    var vm = this;

    vm.indexerDetails = [];
    vm.searcherDetails = [];
    vm.loading = true;
    vm.viewState = "list";
    vm.selectedIndex = null;
    vm.selectedSearcher = null;
    vm.searchResults = null;

    vm.showSearchResultDialog = showSearchResultDialog;
    vm.showIndexInfo = showIndexInfo;
    vm.showSearcherInfo = showSearcherInfo;
    vm.search = search;
    vm.toggle = toggle;
    vm.rebuildIndex = rebuildIndex;
    vm.setViewState = setViewState;
    vm.nextSearchResultPage = nextSearchResultPage;
    vm.prevSearchResultPage = prevSearchResultPage;
    vm.goToPageSearchResultPage = goToPageSearchResultPage;

    vm.infoOverlay = null;

    function showSearchResultDialog(values) {
        if (vm.searchResults) {
            vm.searchResults.overlay = {
                title: "Field values",
                searchResultValues: values,
                view: "views/dashboard/settings/examinemanagementresults.html",
                close: function () {
                    vm.searchResults.overlay = null;
                }
            };
        }
    }

    function nextSearchResultPage(pageNumber) {
        search(vm.selectedIndex ? vm.selectedIndex : vm.selectedSearcher, null, pageNumber);
    }
    function prevSearchResultPage(pageNumber) {
        search(vm.selectedIndex ? vm.selectedIndex : vm.selectedSearcher, null, pageNumber);
    }
    function goToPageSearchResultPage(pageNumber) {
        search(vm.selectedIndex ? vm.selectedIndex : vm.selectedSearcher, null, pageNumber);
    }

    function setViewState(state) {
        vm.searchResults = null;
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
                    $timeout(() => {
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

    function search(searcher, e, pageNumber) {

        //deal with accepting pressing the enter key
        if (e && e.keyCode !== 13) {
            return;
        }

        if (!searcher) {
            throw "searcher parameter is required";
        }

        searcher.isProcessing = true;

        umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("examineMgmtBaseUrl",
                    "GetSearchResults",
                    {
                        searcherName: searcher.name,
                        query: encodeURIComponent(vm.searchText),
                        pageIndex: pageNumber ? (pageNumber - 1) : 0
                    })),
                'Failed to search')
            .then(searchResults => {
                searcher.isProcessing = false;
                vm.searchResults = searchResults
                vm.searchResults.pageNumber = pageNumber ? pageNumber : 1;
                //20 is page size
                vm.searchResults.totalPages = Math.ceil(vm.searchResults.totalRecords / 20);
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
                    $timeout(() => { checkProcessing(index, "PostCheckRebuildIndex"), 1000 });

                });
        }
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
                .then(data => {
                    vm.searcherDetails = data;
                })
            ])
            .then(() => { vm.loading = false });
    }

    init();
}

angular.module("umbraco").controller("Umbraco.Dashboard.ExamineManagementController", ExamineManagementController);
