(function() {
    'use strict';

    function TourStepDirective() {

        function link(scope, element, attrs, ctrl) {

            scope.close = function() {
                if(scope.onClose) {
                    scope.onClose();
                }
            }

        }

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/application/umbtour/umb-tour-step.html',
            scope: {
                size: "@?",
                onClose: "&?",
                hideClose: "=?"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTourStep', TourStepDirective);

})();
