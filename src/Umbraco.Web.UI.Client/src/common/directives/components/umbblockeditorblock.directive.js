angular.module("umbraco.directives").directive('umbBlockEditorBlock', [
    function () {
        var link = function (scope, el, attr, ctrl) {
        };

        return {
            restrict: "E",
            replace: true,
            templateUrl: "views/propertyeditors/blockeditor/blockeditor.block.html",
            link: link
        };
    }
]);