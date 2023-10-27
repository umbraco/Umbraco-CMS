(function () {
    'use strict';

    function onDragLeaveDirective($timeout){

        return function (scope, elm, attrs) {
            var f = function (event) {
                var rect = this.getBoundingClientRect();
                var getXY = function getCursorPosition(event) {
                    var x, y;

                    if (typeof event.clientX === 'undefined') {
                        // try touch screen
                        x = event.pageX + document.documentElement.scrollLeft;
                        y = event.pageY + document.documentElement.scrollTop;
                    } else {
                        x = event.clientX + document.body.scrollLeft + document.documentElement.scrollLeft;
                        y = event.clientY + document.body.scrollTop + document.documentElement.scrollTop;
                    }

                    return { x: x, y: y };
                };

                var e = getXY(event.originalEvent);

                // Check the mouseEvent coordinates are outside of the rectangle
                if (e.x > rect.left + rect.width - 1 || e.x < rect.left || e.y > rect.top + rect.height - 1 || e.y < rect.top) {
                    scope.$apply(attrs.onDragLeave);
                }
            };

            elm.on("dragleave", f);
            scope.$on("$destroy", function () { elm.off("dragleave", f); });
        };

    }

    angular.module('umbraco.directives').directive('onDragLeave', onDragLeaveDirective);

})();
