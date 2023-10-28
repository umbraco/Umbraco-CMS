/**
* @ngdoc directive
* @name umbraco.directives.directive:autoScale
* @element div
* @function
* @description
* Resize div's automatically to fit to the bottom of the screen, as an optional parameter an y-axis offset can be set
* So if you only want to scale the div to 70 pixels from the bottom you pass "70"

* @example
* <example module="umbraco.directives">
*    <file name="index.html">
*        <div auto-scale="70" class="input-block-level"></div>
*    </file>
* </example>
**/

angular.module("umbraco.directives")
    .directive('autoScale', function ($window, $timeout, windowResizeListener) {
        return function (scope, el, attrs) {

            var totalOffset = 0;
            var offsety = parseInt(attrs.autoScale, 10);
            var window = $($window);
            if (offsety !== undefined) {
                totalOffset += offsety;
            }

            $timeout(function () {
                setElementSize();
            });

            function setElementSize() {
                el.height(window.height() - (el.offset().top + totalOffset));
            }

            var resizeCallback = function () {
                setElementSize();
            };

            windowResizeListener.register(resizeCallback);

            //ensure to unregister from all events and kill jquery plugins
            scope.$on('$destroy', function () {
                windowResizeListener.unregister(resizeCallback);
            });

        };
    });
