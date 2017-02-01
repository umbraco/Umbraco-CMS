(function() {
    'use strict';

    function MiniListViewDirective() {

        function link(scope, el, attr, ctrl) {

            scope.search = "";

            scope.openChild = function(event, child) {
                if(scope.onOpenChild) {
                    scope.onOpenChild({"child": child});
                    event.stopPropagation();
                }
            };

            scope.selectChild = function(child) {
                if(scope.onSelectChild) {
                    scope.onSelectChild({"child": child});
                }
            };

            scope.searchMiniListView = function() {
                searchMiniListView();
            };

            var searchMiniListView = _.debounce(function () {
			
                scope.$apply(function () {
                    if (scope.search !== null && scope.search !== undefined) {
                        if(scope.onSearch) {
                            scope.onSearch({"search": scope.search});
                        }
                    }
                });

		    }, 500);

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-mini-list-view.html',
            scope: {
                node: "=",
                children: "=",
                onSelectChild: "&",
                onOpenChild: "&",
                onSearch: "&",
                loadingChildren: "="
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbMiniListView', MiniListViewDirective);

})();
