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
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-tree-search-results.html',
        link: function (scope, element, attrs, ctrl) {
            
        }
    };
}
angular.module('umbraco.directives').directive("umbTreeSearchResults", treeSearchResults);