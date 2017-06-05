(function () {
    'use strict';

    function UmbSkipLinkComponent() {

        function link(scope, element, attrs) {

            scope.setFocus = function () {
                if (scope.element) {
                    $(scope.element).focus();
                }
            };

        }

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-skip-link.html',
            scope: {
                element: "@"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbSkipLink', UmbSkipLinkComponent);

})();
