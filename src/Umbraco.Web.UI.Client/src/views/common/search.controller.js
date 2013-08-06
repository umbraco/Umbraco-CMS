/**
 * @ngdoc controller
 * @name Umbraco.SearchController
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
                navigationService.ui.selectedSearchResult = -1;
                navigationService.showSearch();
                currentTerm = term;
                navigationService.ui.searchResults = searchService.search(term, navigationService.currentSection);
            } else {
                navigationService.ui.searchResults = [];
            }
        }
    };

    $scope.hideSearch = navigationService.hideSearch;

    $scope.iterateResults = function (direction) {
        if (direction == "up" && navigationService.ui.selectedSearchResult < navigationService.ui.searchResults.length)
            navigationService.ui.selectedSearchResult++;
        else if (navigationService.ui.selectedSearchResult > 0)
            navigationService.ui.selectedSearchResult--;
    };

    $scope.selectResult = function () {
        navigationService.showMenu(navigationService.ui.searchResults[navigationService.ui.selectedSearchResult], undefined);
    };
}
//register it
angular.module('umbraco').controller("Umbraco.SearchController", SearchController);