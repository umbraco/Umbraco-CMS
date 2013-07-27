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
                label: "@",
                description: "@",
                value: "="
            },
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/directives/umb-property.html',
            link: function (scope, element, attrs, ctrl) {
                
                scope.model = {};
                scope.model.view = scope.view;
                scope.model.alias = scope.alias || Math.random().toString(36).slice(2);
                scope.model.description = scope.description;
                scope.model.value = scope.value;
                scope.model.label = scope.label;

                scope.propertyEditorView = umbPropEditorHelper.getViewPath(scope.view);   
            }
        };
    });