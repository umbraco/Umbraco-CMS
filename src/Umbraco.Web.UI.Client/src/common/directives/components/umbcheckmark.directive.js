(function() {
    'use strict';

    function CheckmarkDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-checkmark.html',
            scope: {
                size: "@?"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbCheckmark', CheckmarkDirective);

})();
