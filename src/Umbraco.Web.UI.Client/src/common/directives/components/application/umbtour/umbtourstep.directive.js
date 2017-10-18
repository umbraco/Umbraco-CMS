(function() {
    'use strict';

    function TourStepDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/application/umbtour/umb-tour-step.html'
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTourStep', TourStepDirective);

})();
