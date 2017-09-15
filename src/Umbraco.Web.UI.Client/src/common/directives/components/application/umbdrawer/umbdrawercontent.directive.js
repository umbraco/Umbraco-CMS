(function() {
    'use strict';

    function DrawerContentDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/application/umbdrawer/umb-drawer-content.html'
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDrawerContent', DrawerContentDirective);

})();
