(function() {
    'use strict';

    function TourStepCounterDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/application/umbtour/umb-tour-step-counter.html',
            scope: {
                currentStep: "=",
                totalSteps: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTourStepCounter', TourStepCounterDirective);

})();
