/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTreeSearchBox
* @function
* @element ANY
* @restrict E
**/
function treeSearchBox(localizationService) {
    return {
        scope: {
            searchFrom: "@",
            searchFromName: "@",
            showSearch: "@",
            hideSearchCallback: "=",
            searchCallback: "="
        },
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-tree-search-box.html',
        link: function (scope, element, attrs, ctrl) {

            scope.term = "";
            scope.hideSearch = function() {
                scope.term = "";
                scope.hideSearchCallback();
            };

            localizationService.localize("general_typeToSearch").then(function (value) {
                scope.searchPlaceholderText = value;
            });

            if (!scope.showSearch) {
                scope.showSearch = "false";
            }

            scope.$watch("term", _.debounce(function(newVal, oldVal) {
                scope.$apply(function() {
                    if (newVal !== null && newVal !== undefined && newVal !== oldVal) {
                        scope.searchCallback(newVal);
                    }
                });
            }, 200));

        }
    };
}
angular.module('umbraco.directives').directive("umbTreeSearchBox", treeSearchBox);