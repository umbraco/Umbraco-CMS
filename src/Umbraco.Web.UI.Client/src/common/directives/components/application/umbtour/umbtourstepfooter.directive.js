(function() {
    'use strict';

    function TourStepFooterDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/application/umbtour/umb-tour-step-footer.html'
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTourStepFooter', TourStepFooterDirective);

})();
