function BlockEditorEditSettingsController($scope) {

    this.submit = () => {
        if($scope.model.submit) {
            $scope.model.submit($scope.model);
        }
    }

    this.close = () => {
        if($scope.model.close) {
            $scope.model.close();
        }
    } 
}
angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.BlockEditor.EditSettingsController", ['$scope', BlockEditorEditSettingsController]);
