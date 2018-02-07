(function () {
    'use strict';

    function EditorDirective() {

        function link(scope, el, attr, ctrl) {

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor.html',
            scope: {
                editor: "="
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbEditor', EditorDirective);

})();
