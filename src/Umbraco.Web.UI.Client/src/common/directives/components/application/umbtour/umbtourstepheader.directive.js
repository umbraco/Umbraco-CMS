(function() {
    'use strict';

    function TourStepHeaderDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/application/umbtour/umb-tour-step-header.html',
            scope: {
                title: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTourStepHeader', TourStepHeaderDirective);

})();
