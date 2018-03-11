/**
 * @ngdoc controller
 * @name Umbraco.SearchController
 * @function
 * 
 * @description
 * Controls the search functionality in the site
 *  
 */
function SearchController($scope, searchService, $log, $location, navigationService, $q) {

    $scope.searchTerm = null;
    $scope.searchResults = [];
    $scope.isSearching = false;
    $scope.selectedResult = -1;

    $scope.navigateResults = function (ev) {
        //38: up 40: down, 13: enter

        switch (ev.keyCode) {
            case 38:
                iterateResults(true);
                break;
            case 40:
                iterateResults(false);
                break;
            case 13:
                if ($scope.selectedItem) {
                    $location.path($scope.selectedItem.editorPath);
                    navigationService.hideSearch();
                }
                break;
        }
    };

    var group = undefined;
    var groupNames = [];
    var groupIndex = -1;
    var itemIndex = -1;
    $scope.selectedItem = undefined;

    $scope.clearSearch = function () {
        $scope.searchTerm = null;
    };
    function iterateResults(up) {
        //default group
        if (!group) {

            for (var g in $scope.groups) {
                if ($scope.groups.hasOwnProperty(g)) {
                    groupNames.push(g);

                }
            }

            //Sorting to match the groups order
            groupNames.sort();

            group = $scope.groups[groupNames[0]];
            groupIndex = 0;
        }

        if (up) {
            if (itemIndex === 0) {
                if (groupIndex === 0) {
                    gotoGroup(Object.keys($scope.groups).length - 1, true);
                } else {
                    gotoGroup(groupIndex - 1, true);
                }
            } else {
                gotoItem(itemIndex - 1);
            }
        } else {
            if (itemIndex < group.results.length - 1) {
                gotoItem(itemIndex + 1);
            } else {
                if (groupIndex === Object.keys($scope.groups).length - 1) {
                    gotoGroup(0);
                } else {
                    gotoGroup(groupIndex + 1);
                }
            }
        }
    }

    function gotoGroup(index, up) {
        groupIndex = index;
        group = $scope.groups[groupNames[groupIndex]];

        if (up) {
            gotoItem(group.results.length - 1);
        } else {
            gotoItem(0);
        }
    }

    function gotoItem(index) {
        itemIndex = index;
        $scope.selectedItem = group.results[itemIndex];
    }

    //used to cancel any request in progress if another one needs to take it's place
    var canceler = null;

    $scope.$watch("searchTerm", _.debounce(function (newVal, oldVal) {
        $scope.$apply(function () {
            $scope.hasResults = false;
            if ($scope.searchTerm) {
                if (newVal !== null && newVal !== undefined && newVal !== oldVal) {

                    //Resetting for brand new search
                    group = undefined;
                    groupNames = [];
                    groupIndex = -1;
                    itemIndex = -1;

                    $scope.isSearching = true;
                    navigationService.showSearch();
                    $scope.selectedItem = undefined;

                    //a canceler exists, so perform the cancelation operation and reset
                    if (canceler) {
                        canceler.resolve();
                        canceler = $q.defer();
                    }
                    else {
                        canceler = $q.defer();
                    }

                    searchService.searchAll({ term: $scope.searchTerm, canceler: canceler }).then(function (result) {

                        //result is a dictionary of group Title and it's results
                        var filtered = {};
                        _.each(result, function (value, key) {
                            if (value.results.length > 0) {
                                filtered[key] = value;
                            }
                        });
                        $scope.groups = filtered;
                        // check if search has results
                        $scope.hasResults = Object.keys($scope.groups).length > 0;
                        //set back to null so it can be re-created
                        canceler = null;
                        $scope.isSearching = false;
                    });
                }
            }
            else {
                $scope.isSearching = false;
                navigationService.hideSearch();
                $scope.selectedItem = undefined;
            }
        });
    }, 200));

}
//register it
angular.module('umbraco').controller("Umbraco.SearchController", SearchController);