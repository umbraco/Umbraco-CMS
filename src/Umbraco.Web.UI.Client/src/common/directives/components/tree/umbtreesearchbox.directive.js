﻿/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTreeSearchBox
* @function
* @element ANY
* @restrict E
**/
function treeSearchBox(localizationService, searchService, $q) {
    return {
        scope: {
            searchFromId: "@",
            searchFromName: "@",
            showSearch: "@",
            section: "@",
            ignoreUserStartNodes: "@",
            hideSearchCallback: "=",
            searchCallback: "="
        },
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/components/tree/umb-tree-search-box.html',
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


            //used to cancel any request in progress if another one needs to take it's place
            var canceler = null;

            function performSearch() {
                if (scope.term) {
                    scope.results = [];

                    //a canceler exists, so perform the cancelation operation and reset
                    if (canceler) {
                        canceler.resolve();
                        canceler = $q.defer();
                    }
                    else {
                        canceler = $q.defer();
                    }

                    var searchArgs = {
                        term: scope.term,
                        canceler: canceler
                    };

                    //append a start node context if there is one
                    if (scope.searchFromId) {
                        searchArgs["searchFrom"] = scope.searchFromId;
                    }

                    //append ignoreUserStartNodes value if there is one
                    if (scope.ignoreUserStartNodes) {                        
                        searchArgs["ignoreUserStartNodes"] = scope.ignoreUserStartNodes;
                    }                   

                    searcher(searchArgs).then(function (data) {
                        scope.searchCallback(data);
                        //set back to null so it can be re-created
                        canceler = null;
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
