/**
@ngdoc directive
@name umbraco.directives.directive:umbTourStepFooter
@restrict E
@scope

@description
<b>Added in Umbraco 7.8</b>. The tour step footer component is a component that can be used in custom views for tour steps. It's meant to be used in the umb-tour-step directive.
All markup in the body of the directive will be shown as the footer of the tour step


**/
(function () {
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
