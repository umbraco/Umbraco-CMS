(function () {
    'use strict';

    function onRightClickDirective($parse){

        document.oncontextmenu = function (e) {
            if (e.target.hasAttribute('on-right-click')) {
                e.preventDefault();
                e.stopPropagation();
                return false;
            }
        };

        return function (scope, el, attrs) {
            el.on('contextmenu', function (e) {
                e.preventDefault();
                e.stopPropagation();
                var fn = $parse(attrs.onRightClick);
                scope.$apply(function () {
                    fn(scope, { $event: e });
                });
                return false;
            });
        };

    }

    angular.module('umbraco.directives').directive('onRightClick', onRightClickDirective);

})();
