/**
@ngdoc directive
@name umbraco.directives.directive:umbTourStepContent
@restrict E
@scope

@description
<b>Added in Umbraco 7.8</b>. The tour step content component is a component that can be used in custom views for tour steps.
It's meant to be used in the umb-tour-step directive.
All markup in the body of the directive will be shown after the content attribute

@param {string} content The content that needs to be shown
**/
(function () {
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
