angular.module("umbraco.directives").directive('umbBlockEditorBlock', [
    function () {
        return {
            restrict: "E",
            template: "<div ng-include='block.settings.view'></div>",
        };
    }
]);
