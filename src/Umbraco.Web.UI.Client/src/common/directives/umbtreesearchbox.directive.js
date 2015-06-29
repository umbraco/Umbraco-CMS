/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTreeSearchBox
* @function
* @element ANY
* @restrict E
**/
function treeSearchBox(localizationService, searchService) {
    return {
        scope: {
            searchFromId: "@",
            searchFromName: "@",
            showSearch: "@",
            section: "@",
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

            function performSearch() {
                if (scope.term) {
                    scope.results = [];

                    var searchArgs = {
                        term: scope.term
                    };
                    //append a start node context if there is one
                    if (scope.searchFromId) {
                        searchArgs["searchFrom"] = scope.searchFromId;
                    }
                    searcher(searchArgs).then(function (data) {
                        scope.searchCallback(data);
                    });
                }
            }

            scope.$watch("term", _.debounce(function(newVal, oldVal) {
                scope.$apply(function() {
                    if (newVal !== null && newVal !== undefined && newVal !== oldVal) {
                        performSearch();
                    }
                });
            }, 200));

            var searcher = searchService.searchContent;
            //search
            if (scope.section === "member") {
                searcher = searchService.searchMembers;             
            }
            else if (scope.section === "media") {
                searcher = searchService.searchMedia;
            }
        }
    };
}
angular.module('umbraco.directives').directive("umbTreeSearchBox", treeSearchBox);