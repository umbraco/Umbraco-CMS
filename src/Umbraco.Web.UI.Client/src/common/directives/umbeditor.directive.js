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
                model: "="
            },
            restrict: 'E',
            replace: true,      
            templateUrl: 'views/directives/umb-editor.html',
            link: function (scope, element, attrs, ctrl) {
                
                if(!scope.model.alias){
                   scope.model.alias = Math.random().toString(36).slice(2);
                }

                scope.propertyEditorView = umbPropEditorHelper.getViewPath(scope.model.view);
            }
        };
    });