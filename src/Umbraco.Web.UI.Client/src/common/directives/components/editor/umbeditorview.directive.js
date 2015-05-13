angular.module("umbraco.directives.html")
    .directive('umbEditorView', function () {
        return {
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/components/editor/umb-editor-view.html'
        };
    });