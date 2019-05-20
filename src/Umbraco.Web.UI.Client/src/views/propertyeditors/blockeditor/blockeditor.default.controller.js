function BlockEditorDefaultController($scope, contentResource, editorService) {
    console.log("Block editor default controller", $scope.model)
    // TODO: move this to a controller supporting the default blockeditor view
    $scope.sortableOptions = {
        axis: "y",
        cursor: "move",
        handle: ".handle",
        tolerance: 'pointer'
    };
}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.DefaultController", BlockEditorDefaultController);
