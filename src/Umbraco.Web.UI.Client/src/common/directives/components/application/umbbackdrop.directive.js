(function () {
    "use strict";

    function BackdropDirective($timeout, $http) {

        function link(scope, el, attr, ctrl) {

            var events = [];

            scope.clickBackdrop = function(event) {
                if(scope.disableEventsOnClick === true) {
                    event.preventDefault();
                    event.stopPropagation();
                }
            };

            function onInit() {

                if (scope.element) {
                    setHighlight();
                }

            }

            function setHighlight () {

                $timeout(function () {

                    // The element to highlight
                    var highlightElement = angular.element(scope.element);

                    if(highlightElement) {

                        var offset = highlightElement.offset();
                        var width = highlightElement.outerWidth(true);
                        var height = highlightElement.outerHeight(true);

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
                        rectTop.css({ "height": topDistance, "x": leftDistance });
                        rectRight.css({ "x": leftAndWidth, "y": topDistance, "height": height });
                        rectBottom.css({ "height": "100%", "y": topAndHeight, "x": leftDistance });                    
                        rectLeft.css({ "width": leftDistance });

                    }

                });

            }

            function resize() {
                setHighlight();
            }

            events.push(scope.$watch("element", function (newValue, oldValue) {
                if(!newValue) {return;}
                if(newValue === oldValue) {return;}
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
                element: "=",
                disableEventsOnClick: "="
            }
        };

        return directive;

    }

    angular.module("umbraco.directives").directive("umbBackdrop", BackdropDirective);

})();
