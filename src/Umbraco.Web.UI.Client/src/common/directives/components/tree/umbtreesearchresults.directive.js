/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTreeSearchResults
* @function
* @element ANY
* @restrict E
**/
function treeSearchResults() {
    return {
        scope: {
            results: "=",
            selectResultCallback: "="
        },
        restrict: "E",  
        replace: true, 
        templateUrl: 'views/components/tree/umb-tree-search-results.html',
        link: function (scope, element, attrs, ctrl) {

        }
    };
}
angular.module('umbraco.directives').directive("umbTreeSearchResults", treeSearchResults);
