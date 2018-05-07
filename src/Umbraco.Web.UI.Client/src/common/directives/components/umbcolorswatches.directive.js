(function () {
    'use strict';

    function ColorSwatchesDirective() {

        function link(scope, el, attr, ctrl) {

            scope.setColor = function (color) {
                //scope.selectedColor({color: color });
                scope.selectedColor = color;

                console.log("selectedColor", selectedColor);

                if (scope.onSelect) {
                    console.log("onselect", color);
                    scope.onSelect(color);
                }
            };
        }

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-color-swatches.html',
            scope: {
                colors: "=",
                selectedColor: "&",
                onSelect: '='
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbColorSwatches', ColorSwatchesDirective);

})();
