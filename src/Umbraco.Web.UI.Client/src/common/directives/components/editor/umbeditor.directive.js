(function () {
    'use strict';

    function EditorDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor.html',
            scope: {
                model: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbEditor', EditorDirective);

})();
