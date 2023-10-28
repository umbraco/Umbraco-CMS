(function () {
    'use strict';

    function onDragEnterDirective(){

        function link(scope, elm, attrs) {
            var f = function () {
                scope.$apply(attrs.onDragEnter);
            };
            elm.on("dragenter", f);
            scope.$on("$destroy", function () { elm.off("dragenter", f); });
        }

        var directive = {
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('onDragEnter', onDragEnterDirective);

})();
