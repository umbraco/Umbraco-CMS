(function() {
    'use strict';

    function DrawerHeaderDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/application/umbdrawer/umb-drawer-header.html',
            scope: {
                "title": "@",
                "description": "@"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDrawerHeader', DrawerHeaderDirective);

})();
