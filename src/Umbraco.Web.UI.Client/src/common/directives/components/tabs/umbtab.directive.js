/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTab
* @restrict E
**/
angular.module("umbraco.directives")
.directive('umbTab', function ($parse, $timeout) {
    return {
		restrict: 'E',
		replace: true,
        transclude: 'true',
        templateUrl: 'views/components/tabs/umb-tab.html',
        link: function (scope, el, attr) {
            scope.tab["active"] = false;
            scope.tab["loaded"] = false;
            if (typeof (scope.tab) != "undefined") {
                scope.$watch(function () {
                    return $(el).hasClass("active")
                },
                function () {
                    scope.tab.visible = $(el).hasClass("active");
                    scope.tab.loaded = scope.tab.visible || scope.tab.loaded;
                });
            }
        }
    };
});
