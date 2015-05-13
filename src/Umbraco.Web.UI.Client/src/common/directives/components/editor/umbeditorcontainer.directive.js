angular.module("umbraco.directives.html")
    .directive('umbEditorContainer', function () {
        return {
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/components/editor/umb-editor-container.html'
        };
    });