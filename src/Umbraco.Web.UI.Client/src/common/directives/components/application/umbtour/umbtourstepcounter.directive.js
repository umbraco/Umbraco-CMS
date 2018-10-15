/**
@ngdoc directive
@name umbraco.directives.directive:umbTourStepCounter
@restrict E
@scope

@description
<b>Added in Umbraco 7.8</b>. The tour step counter component is a component that can be used in custom views for tour steps.
It's meant to be used in the umb-tour-step-footer directive. It will show the progress you have made in a tour eg. step 2/12


@param {int} currentStep The current step the tour is on
@param {int} totalSteps The current step the tour is on
**/
(function () {
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
