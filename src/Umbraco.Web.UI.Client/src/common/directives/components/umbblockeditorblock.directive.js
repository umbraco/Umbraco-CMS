angular.module("umbraco.directives").directive('umbBlockEditorBlock', [
    function () {
        var link = function (scope, el, attr, ctrl) {
            scope.view = attr.view || "views/propertyeditors/blockeditor/blockeditor.block.html";
        };

        return {
            restrict: "E",
            template: "<div ng-include='view'></div>",
            link: link
        };
    }
]);