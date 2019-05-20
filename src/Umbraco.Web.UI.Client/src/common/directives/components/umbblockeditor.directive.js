angular.module("umbraco.directives").directive('umbBlockEditor', [
    function () {
        var link = function (scope, el, attr, ctrl) {
            scope.view = attr.view || "views/propertyeditors/blockeditor/blockeditor.simplelist.html";
        };

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
            link: link
        };
    }
]);
