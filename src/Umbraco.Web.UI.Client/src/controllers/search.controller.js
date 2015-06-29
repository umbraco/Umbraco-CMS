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


    $scope.navigateResults = function(ev){
        //38: up 40: down, 13: enter

        switch(ev.keyCode){
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
    var groupIndex = -1;
    var itemIndex = -1;
    $scope.selectedItem = undefined;
        

    function iterateResults(up){
        //default group
        if(!group){
            group = $scope.groups[0];
            groupIndex = 0;
        }

        if(up){
            if(itemIndex === 0){
                if(groupIndex === 0){
                    gotoGroup($scope.groups.length-1, true);
                }else{
                    gotoGroup(groupIndex-1, true);
                }
            }else{
                gotoItem(itemIndex-1);
            }
        }else{
            if(itemIndex < group.results.length-1){
                gotoItem(itemIndex+1);
            }else{
                if(groupIndex === $scope.groups.length-1){
                    gotoGroup(0);
                }else{
                    gotoGroup(groupIndex+1);
                }
            }
        }
    }

    function gotoGroup(index, up){
        groupIndex = index;
        group = $scope.groups[groupIndex];
        
        if(up){
            gotoItem(group.results.length-1);
        }else{
            gotoItem(0); 
        }
    }

    function gotoItem(index){
        itemIndex = index;
        $scope.selectedItem = group.results[itemIndex];
    }

    //used to cancel any request in progress if another one needs to take it's place
    var canceler = null;

    $scope.$watch("searchTerm", _.debounce(function (newVal, oldVal) {
        $scope.$apply(function() {
            if ($scope.searchTerm) {
                if (newVal !== null && newVal !== undefined && newVal !== oldVal) {
                    $scope.isSearching = true;
                    navigationService.showSearch();
                    $scope.selectedItem = undefined;

                    //a canceler exists, so perform the cancelation operation and reset
                    if (canceler) {
                        console.log("CANCELED!");
                        canceler.resolve();
                        canceler = $q.defer();
                    }
                    else {
                        canceler = $q.defer();
                    }

                    searchService.searchAll({ term: $scope.searchTerm, canceler: canceler }).then(function(result) {
                        $scope.groups = _.filter(result, function (group) { return group.results.length > 0; });
                        //set back to null so it can be re-created
                        canceler = null;
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