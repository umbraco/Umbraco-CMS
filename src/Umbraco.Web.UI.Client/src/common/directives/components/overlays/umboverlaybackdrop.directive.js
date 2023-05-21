(function () {
    'use strict';

    function OverlayBackdropDirective(overlayHelper) {

        function link(scope, el, attr, ctrl) {

            scope.numberOfOverlays = 0;

            // TODO: this shouldn't be a watch, this should be based on an event handler
            scope.$watch(function () {
                return overlayHelper.getNumberOfOverlays();
            }, function (newValue) {
                scope.numberOfOverlays = newValue;
            });

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/overlays/umb-overlay-backdrop.html',
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbOverlayBackdrop', OverlayBackdropDirective);

})();
