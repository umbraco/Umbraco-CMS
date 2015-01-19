function examineMgmtController($scope, umbRequestHelper, $log, $http, $q, $timeout) {

    $scope.indexerDetails = [];
    $scope.searcherDetails = [];
    $scope.loading = true;

    function checkProcessing(indexer, checkActionName) {
        umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("examineMgmtBaseUrl", checkActionName, { indexerName: indexer.name })),
                'Failed to check index processing')
            .then(function(data) {

                if (data) {

                    //copy all resulting properties
                    for (var k in data) {
                        indexer[k] = data[k];
                    }
                    indexer.isProcessing = false;
                }
                else {
                    $timeout(function () {
                        //don't continue if we've tried 100 times
                        if (indexer.processingAttempts < 100) {
                            checkProcessing(indexer, checkActionName);
                            //add an attempt
                            indexer.processingAttempts++;
                        }
                        else {
                            //we've exceeded 100 attempts, stop processing
                            indexer.isProcessing = false;
                        }
                    }, 1000);
                }
            });
    }

    $scope.search = function (searcher, e) {
        if (e && e.keyCode !== 13) {
            return;
        }

        umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("examineMgmtBaseUrl", "GetSearchResults", {
                    searcherName: searcher.name,
                    query: searcher.searchText,
                    queryType: searcher.searchType
                })),
                'Failed to search')
            .then(function(searchResults) {
                searcher.isSearching = true;
                searcher.searchResults = searchResults;
            });
    }
    
    $scope.toggle = function(provider, propName) {
        if (provider[propName] !== undefined) {
            provider[propName] = !provider[propName];
        }
        else {
            provider[propName] = true;
        }
    }

    $scope.rebuildIndex = function(indexer) {
        if (confirm("This will cause the index to be rebuilt. " +
                        "Depending on how much content there is in your site this could take a while. " +
                        "It is not recommended to rebuild an index during times of high website traffic " +
                        "or when editors are editing content.")) {
            indexer.isProcessing = true;

            umbRequestHelper.resourcePromise(
                    $http.post(umbRequestHelper.getApiUrl("examineMgmtBaseUrl", "PostRebuildIndex", { indexerName: indexer.name })),
                    'Failed to rebuild index')
                .then(function () {

                    //rebuilding has started, nothing is returned accept a 200 status code.
                    //lets poll to see if it is done.
                    $timeout(function () {
                        checkProcessing(indexer, "PostCheckRebuildIndex");
                    }, 1000);

                });
        }
    }

    $scope.optimizeIndex = function(indexer) {
        if (confirm("This will cause the index to be optimized which will improve its performance. " +
                        "It is not recommended to optimize an index during times of high website traffic " +
                        "or when editors are editing content.")) {
            indexer.isProcessing = true;

            umbRequestHelper.resourcePromise(
                    $http.post(umbRequestHelper.getApiUrl("examineMgmtBaseUrl", "PostOptimizeIndex", { indexerName: indexer.name })),
                    'Failed to optimize index')
                .then(function () {

                    //optimizing has started, nothing is returned accept a 200 status code.
                    //lets poll to see if it is done.
                    $timeout(function () {
                        checkProcessing(indexer, "PostCheckOptimizeIndex");
                    }, 1000);

                });
        }
    }

    $scope.closeSearch = function(searcher) {
        searcher.isSearching = true;
    }

     
    //go get the data

    //combine two promises and execute when they are both done
    $q.all([

        //get the indexer details
        umbRequestHelper.resourcePromise(
            $http.get(umbRequestHelper.getApiUrl("examineMgmtBaseUrl", "GetIndexerDetails")),
            'Failed to retrieve indexer details')
        .then(function(data) {
            $scope.indexerDetails = data; 
        }),

        //get the searcher details
        umbRequestHelper.resourcePromise(
            $http.get(umbRequestHelper.getApiUrl("examineMgmtBaseUrl", "GetSearcherDetails")),
            'Failed to retrieve searcher details')
        .then(function(data) {
            $scope.searcherDetails = data;
            for (var s in $scope.searcherDetails) {
                $scope.searcherDetails[s].searchType = "text";
            }
        })

    ]).then(function () {
        //all init loading is complete
        $scope.loading = false;
    });


}
angular.module("umbraco").controller("Umbraco.Dashboard.ExamineMgmtController", examineMgmtController);