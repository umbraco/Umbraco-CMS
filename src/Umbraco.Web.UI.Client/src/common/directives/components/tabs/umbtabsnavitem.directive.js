/**
@ngdoc directive
@name umbraco.directives.directive:umbTabsNavItem
@restrict E
@scope
**/

(function() {
    'use strict';

    function TabsNavItemDirective() {

        function UmbTabsNavItemController($scope) {

            var vm = this;

            vm.tab = $scope.tab;
            vm.onClick = $scope.onClick;

        }

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: "views/components/tabs/umb-tabs-nav-item.html",
            controller: UmbTabsNavItemController,
            controllerAs: 'vm',
            scope: {
                tab: "<",
                onClick: "&",
                hideBadge: "<"
            }
        };
        return directive;
    }

    angular.module('umbraco.directives').directive('umbTabsNavItem', TabsNavItemDirective);

})();
