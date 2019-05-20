function BlockEditorDefaultController($scope, contentResource, editorService) {
    $scope.sortableOptions = {
        axis: "y",
        cursor: "move",
        handle: ".handle",
        tolerance: 'pointer'
    };
}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.DefaultController", BlockEditorDefaultController);
