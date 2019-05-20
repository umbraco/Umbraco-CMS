function BlockEditorEditSettingsController($scope) {
    var vm = this;
    vm.submit = submit;
    vm.close = close;

    function init() {

    }

    function submit() {
        if($scope.model.submit) {
            $scope.model.submit($scope.model);
        }
    }

    function close() {
        if($scope.model.close) {
            $scope.model.close();
        }
    }

    init();
}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.EditSettingsController", BlockEditorEditSettingsController);
