
angular.module("umbraco.directives")
    .directive('umbProperty', function (umbPropEditorHelper) {
        return {
            scope: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/directives/umb-property.html',
            link: function(scope, element, attrs, ctrl) {
                scope.propertyEditorView = umbPropEditorHelper.getViewPath(scope.model.view);
            }
        };
    });