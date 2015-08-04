angular.module("umbraco.directives")
    .directive('delayedMouseleave', function ($timeout, $parse) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs, ctrl) {
                var active = false;
                var fn = $parse(attrs.delayedMouseleave);

                function mouseLeave(event) {
                    var callback = function () {
                        fn(scope, { $event: event });
                    };

                    active = false;
                    $timeout(function () {
                        if (active === false) {
                            scope.$apply(callback);
                        }
                    }, 650);
                }

                function mouseEnter(event, args){
                    active = true;
                }

                element.on("mouseleave", mouseLeave);
                element.on("mouseenter", mouseEnter);

                //unbind!!
                scope.$on('$destroy', function () {
                    element.off("mouseleave", mouseLeave);
                    element.off("mouseenter", mouseEnter);
                });
            }
        };
    });
