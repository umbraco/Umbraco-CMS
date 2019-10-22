(function () {
    'use strict';

    function onDragOverDirective(){

        function link(scope, elm, attrs) {
            var f = function () {
                scope.$apply(attrs.onDragOver);
            };
            elm.on("dragover", f);
            scope.$on("$destroy", function () { elm.off("dragover", f); });
        }

        var directive = {
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('onDragOver', onDragOverDirective);

})();
