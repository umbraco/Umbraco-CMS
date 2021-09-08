angular.module("umbraco").controller("Umbraco.PrevalueEditors.MarkdownEditorController",
    function ($scope) {
        if (!$scope.model.value) {
            $scope.model.value = "small";
        }
    });
