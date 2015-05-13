angular.module("umbraco.directives.html")
    .directive('umbEditorFooter', function () {
        return {
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/components/editor/umb-editor-footer.html'
        };
    });