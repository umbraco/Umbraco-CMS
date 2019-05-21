angular.module("umbraco.directives").directive('umbBlockEditor', [
    function () {
        return {
            restrict: "E", 
            templateUrl: "views/propertyeditors/blockeditor/blockeditor.directive.html",
            scope: {
                config: "=",
                view: "=?",
                blocks: "="
            },
            controller: "Umbraco.PropertyEditors.BlockEditor.DirectiveController",
            controllerAs: "vm",
        };
    } 
]); 
 