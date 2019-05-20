angular.module("umbraco.directives").directive('umbBlockEditorBlocks', [
    function () {
        var link = function (scope, el, attr, ctrl) {
        };

        return {
            restrict: "E",
            replace: true,
            templateUrl: "views/propertyeditors/blockeditor/blockeditor.blocks.html",
            link: link
        };
    }
]);
