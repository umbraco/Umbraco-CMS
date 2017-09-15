(function() {
    'use strict';

    function DrawerFooterDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/application/umbdrawer/umb-drawer-footer.html'
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDrawerFooter', DrawerFooterDirective);

})();
