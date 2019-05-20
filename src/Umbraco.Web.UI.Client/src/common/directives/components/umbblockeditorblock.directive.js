angular.module("umbraco.directives").directive('umbBlockEditorBlock', [
    function () {
        var link = function (scope, el, attr, ctrl) {
            scope.view = attr.view;
        };

        return {
            restrict: "E",
            templateUrl: "views/propertyeditors/blockeditor/blockeditor.block.html",
            link: link
        };
    }
]);
