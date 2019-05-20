function BlockEditorEditSettingsController($scope) {

    function init() {

    }

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

    init();
}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.EditSettingsController", ['$scope', BlockEditorEditSettingsController]);
