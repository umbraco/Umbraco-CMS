/**
* @ngdoc directive
* @function
* @name umbraco.directives.directive:umbPropertyEditor 
* @requires formController
* @restrict E
**/

//share property editor directive function
var _umbPropertyEditor = function (umbPropEditorHelper) {
        return {
            scope: {
                model: "=",
                isPreValue: "@",
                preview: "@"
            },
            
            require: "^form",
            restrict: 'E',
            replace: true,      
            templateUrl: 'views/components/property/umb-property-editor.html',
            link: function (scope, element, attrs, ctrl) {

                //we need to copy the form controller val to our isolated scope so that
                //it get's carried down to the child scopes of this!
                //we'll also maintain the current form name.
                scope[ctrl.$name] = ctrl;

                if(!scope.model.alias){
                   scope.model.alias = Math.random().toString(36).slice(2);
                }

                scope.$watch("model.view", function(val){
                    scope.propertyEditorView = umbPropEditorHelper.getViewPath(scope.model.view, scope.isPreValue);
                });
            }
        };
    };

//Preffered is the umb-property-editor as its more explicit - but we keep umb-editor for backwards compat
angular.module("umbraco.directives").directive('umbPropertyEditor', _umbPropertyEditor);
angular.module("umbraco.directives").directive('umbEditor', _umbPropertyEditor);
