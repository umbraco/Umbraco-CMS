(function () {
    "use strict";

    function BackdropDirective($timeout, $http) {

        function link(scope, el, attr, ctrl) {

            var events = [];

            scope.clickBackdrop = function (event) {
                if (scope.disableEventsOnClick === true) {
                    event.preventDefault();
                    event.stopPropagation();
                }
            };

            function onInit() {

                if (scope.highlightElement) {
                    setHighlight();
                }

            }

            function setHighlight() {

                scope.loading = true;

                $timeout(function () {

                    // The element to highlight
                    var highlightElement = $(scope.highlightElement);

                    if (highlightElement && highlightElement.length > 0) {

                        var offset = highlightElement.offset();
                        var width = highlightElement.outerWidth();
                        var height = highlightElement.outerHeight();

                        // Rounding numbers
                        var topDistance = offset.top.toFixed();
                        var topAndHeight = (offset.top + height).toFixed();
                        var leftDistance = offset.left.toFixed();
                        var leftAndWidth = (offset.left + width).toFixed();

                        // The four rectangles
                        var rectTop = el.find(".umb-backdrop__rect--top");
                        var rectRight = el.find(".umb-backdrop__rect--right");
                        var rectBottom = el.find(".umb-backdrop__rect--bottom");
                        var rectLeft = el.find(".umb-backdrop__rect--left");

                        // Add the css
                        scope.rectTopCss = { "height": topDistance, "left": leftDistance + "px", opacity: scope.backdropOpacity };
                        scope.rectRightCss = { "left": leftAndWidth + "px", "top": topDistance + "px", "height": height, opacity: scope.backdropOpacity };
                        scope.rectBottomCss = { "height": "100%", "top": topAndHeight + "px", "left": leftDistance + "px", opacity: scope.backdropOpacity };
                        scope.rectLeftCss = { "width": leftDistance, opacity: scope.backdropOpacity };

                        // Prevent interaction in the highlighted area
                        if (scope.highlightPreventClick) {
                            var preventClickElement = el.find(".umb-backdrop__highlight-prevent-click");
                            preventClickElement.css({ "width": width, "height": height, "left": offset.left, "top": offset.top });
                        }

                    }

                    scope.loading = false;

                });

            }

            function resize() {
                setHighlight();
            }

            events.push(scope.$watch("highlightElement", function (newValue, oldValue) {
                if (!newValue) { return; }
                if (newValue === oldValue) { return; }
                setHighlight();
            }));

            $(window).on("resize.umbBackdrop", resize);

            scope.$on("$destroy", function () {
                // unbind watchers
                for (var e in events) {
                    events[e]();
                }
                $(window).off("resize.umbBackdrop");
            });

            onInit();

        }

        var directive = {
            transclude: true,
            restrict: "E",
            replace: true,
            templateUrl: "views/components/application/umb-backdrop.html",
            link: link,
            scope: {
                backdropOpacity: "=?",
                highlightElement: "=?",
                highlightPreventClick: "=?",
                disableEventsOnClick: "=?"
            }
        };

        return directive;

    }

    angular.module("umbraco.directives").directive("umbBackdrop", BackdropDirective);

})();
