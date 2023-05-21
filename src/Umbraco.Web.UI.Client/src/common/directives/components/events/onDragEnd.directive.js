(function () {
    'use strict';

    function onDragEndDirective(){

        function link(scope, elm, attrs) {
            var f = function () {
                scope.$apply(attrs.onDragEnd);
            };
            elm.on("dragend", f);
            scope.$on("$destroy", function () { elm.off("dragend", f); });
        }

        var directive = {
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('onDragEnd', onDragEndDirective);

})();
