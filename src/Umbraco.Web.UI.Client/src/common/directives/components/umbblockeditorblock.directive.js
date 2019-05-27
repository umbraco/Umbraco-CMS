angular.module("umbraco.directives").directive('umbBlockEditorBlock', [
    function () {
        var link = function (scope, el, attr, ctrl) {
            scope.view = scope.block.view || attr.view;
        };
        return {
            restrict: "E",
            template: "<div ng-include='view'></div>",
            link: link
        };
    }
]);
