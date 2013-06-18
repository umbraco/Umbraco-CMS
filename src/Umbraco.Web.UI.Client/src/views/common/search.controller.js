/**
 * @ngdoc controller
 * @name SearchController
 * @function
 * 
 * @description
 * Controls the search functionality in the site
 *  
 */
function SearchController($scope, searchService, $log, navigationService) {
    var currentTerm = "";
    $scope.deActivateSearch = function () {
        currentTerm = "";
    };

    $scope.performSearch = function (term) {
        if (term != undefined && term != currentTerm) {
            if (term.length > 3) {
                $scope.ui.selectedSearchResult = -1;
                navigationService.showSearch();
                currentTerm = term;
                $scope.ui.searchResults = searchService.search(term, $scope.currentSection);
            } else {
                $scope.ui.searchResults = [];
            }
        }
    };

    $scope.hideSearch = navigationService.hideSearch;

    $scope.iterateResults = function (direction) {
        if (direction == "up" && $scope.ui.selectedSearchResult < $scope.ui.searchResults.length)
            $scope.ui.selectedSearchResult++;
        else if ($scope.ui.selectedSearchResult > 0)
            $scope.ui.selectedSearchResult--;
    };

    $scope.selectResult = function () {
        navigationService.showMenu($scope.ui.searchResults[$scope.ui.selectedSearchResult], undefined);
    };
}
//register it
angular.module('umbraco').controller("SearchController", SearchController);