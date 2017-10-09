(function () {
    "use strict";

    function BackdropDirective($timeout, $http) {

        function link(scope, el, attr, ctrl) {

            var events = [];

            function onInit() {

                if (scope.element) {
                    setHighlight();
                }

            }

            function setHighlight () {

                $timeout(function () {

                    var element = angular.element(scope.element);
                    var offset = element.offset();
                    var width = element.outerWidth(true);
                    var height = element.outerHeight(true);

                    // Rounding numbers
                    var topDistance = offset.top.toFixed();
                    var topAndHeight = (offset.top + height).toFixed();
                    var leftDistance = offset.left.toFixed();
                    var leftAndWidth = (offset.left + width).toFixed();

                    angular.element(".rect-left").css({ "width": leftDistance });
                    angular.element(".rect-top").css({ "height": topDistance, "x": leftDistance });
                    angular.element(".rect-bot").css({ "height": "100%", "y": topAndHeight, "x": leftDistance });
                    angular.element(".rect-right").css({ "x": leftAndWidth, "y": topDistance, "height": height });
                });

            }

            function resize() {
                setHighlight();
            }

            events.push(scope.$watch("element", function (newValue, oldValue) {
                if(newValue === oldValue) {return;}
                setHighlight();
            }));

            $(window).on('resize.umbBackdrop', resize);

            scope.$on('$destroy', function () {
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
                element: "="
            }
        };

        return directive;

    }

    angular.module("umbraco.directives").directive("umbBackdrop", BackdropDirective);

})();
