(function () {
    'use strict';

    function onDelayedMouseleaveDirective($timeout, $parse){

        function link(scope, element, attrs, ctrl) {
            var active = false;
            var fn = $parse(attrs.onDelayedMouseleave);

            var leave_f = function (event) {
                var callback = function () {
                    fn(scope, { $event: event });
                };

                active = false;
                $timeout(function () {
                    if (active === false) {
                        scope.$apply(callback);
                    }
                }, 650);
            };

            var enter_f = function (event, args) {
                active = true;
            };


            element.on("mouseleave", leave_f);
            element.on("mouseenter", enter_f);

            //unsub events
            scope.$on("$destroy", function () {
                element.off("mouseleave", leave_f);
                element.off("mouseenter", enter_f);
            });
        }

        var directive = {
            restrict: 'A',
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('onDelayedMouseleave', onDelayedMouseleaveDirective);

})();
