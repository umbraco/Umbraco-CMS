/**
@ngdoc directive
@name umbraco.directives.directive:umbTourStepHeader
@restrict E
@scope

@description
<b>Added in Umbraco 7.8</b>. The tour step header component is a component that can be used in custom views for tour steps. It's meant to be used in the umb-tour-step directive.


@param {string} title The title that needs to be shown
**/
(function () {
    'use strict';

    function TourStepHeaderDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/application/umbtour/umb-tour-step-header.html',
            scope: {
                title: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTourStepHeader', TourStepHeaderDirective);

})();
