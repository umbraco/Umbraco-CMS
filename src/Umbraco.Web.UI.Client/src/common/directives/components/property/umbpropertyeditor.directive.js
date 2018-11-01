/**
* @ngdoc directive
* @function
* @name umbraco.directives.directive:umbPropertyEditor 
* @requires formController
* @restrict E
**/

//share property editor directive function
function umbPropEditor(umbPropEditorHelper) {
        return {
            scope: {
                model: "=",
                isPreValue: "@",
                preview: "<"
            },
            
            require: "^^form",
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

                scope.propertyEditorView = umbPropEditorHelper.getViewPath(scope.model.view, scope.isPreValue);
                
            }
        };
    };

angular.module("umbraco.directives").directive('umbPropertyEditor', umbPropEditor);
