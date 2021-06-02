/**
@ngdoc directive
@name umbraco.directives.directive:umbTourStep
@restrict E
@scope

@description
<b>Added in Umbraco 7.8</b>. The tour step component is a component that can be used in custom views for tour steps.

@param {callback} onClose The callback which should be performened when the close button of the tour step is clicked
@param {boolean=} hideClose A boolean indicating if the close button needs to be shown

**/
(function () {
    'use strict';

    function TourStepDirective() {

        function link(scope, element, attrs, ctrl) {

            scope.close = function () {
                if (scope.onClose) {
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
