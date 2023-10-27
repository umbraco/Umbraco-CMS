(function () {
    'use strict';

    function onDragStartDirective($timeout){

        function link(scope, elm, attrs) {
            var f = function () {
                scope.$apply(attrs.onDragStart);
            };
            elm.on("dragstart", f);
            scope.$on("$destroy", function () { elm.off("dragstart", f); });
        }

        var directive = {
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('onDragStart', onDragStartDirective);

})();
