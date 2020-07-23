(function () {
    'use strict';

    /**
     * A directive that renders a defined view with a view model and a the whole content model.
     **/
    function EditorSubViewDirective() {

        function link(scope) {
            //The model can contain: view, viewModel, name, alias, icon

            if (!scope.model.view) {
                throw "No view defined for the content app";
            }
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-sub-view.html',
            scope: {
                model: "=",
                variantContent: "=?",
                content: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbEditorSubView', EditorSubViewDirective);

})();
