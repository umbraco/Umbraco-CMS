(function() {
    'use strict';

    function TourStepContentDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/application/umbtour/umb-tour-step-content.html',
            scope: {
                content: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTourStepContent', TourStepContentDirective);

})();
