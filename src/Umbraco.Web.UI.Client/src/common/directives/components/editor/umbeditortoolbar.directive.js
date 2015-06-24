angular.module("umbraco.directives")
    .directive('umbEditorToolbar', function () {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-toolbar.html',
            scope: {
              tools: "="
            }
        };
    });