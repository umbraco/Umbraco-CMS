(function () {
    'use strict';

    function EditorSubViewsDirective() {

        function link(scope, el, attr, ctrl) {

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-sub-views.html',
            scope: {
                subViews: "=",
                model: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbEditorSubViews', EditorSubViewsDirective);

})();
