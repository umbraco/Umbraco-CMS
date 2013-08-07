/**
* @ngdoc directive
* @function
* @name umbraco.directives.directive:umbEditor 
* @restrict E
**/
angular.module("umbraco.directives")
    .directive('umbEditor', function (umbPropEditorHelper) {
        return {
            scope: {
                view: "@",
                alias: "@",
                config: "@",
                value: "="
            },
            restrict: 'E',
            replace: true,      
            templateUrl: 'views/directives/umb-editor.html',
            link: function (scope, element, attrs, ctrl) {
                if(!scope.alias){
                   scope.alias = Math.random().toString(36).slice(2);
                }
                scope.propertyEditorView = umbPropEditorHelper.getViewPath(scope.view);
            }
        };
    });