(function() {
    'use strict';

    function BadgeDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-badge.html',
            scope: {
                size: "@?",
                color: "@?"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBadge', BadgeDirective);

})();
