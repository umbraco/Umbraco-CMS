function ConfigController($scope) {

    var vm = this;

    vm.submit = submit;
    vm.close = close;

    function submit() {
        if ($scope.model && $scope.model.submit) {
            $scope.model.submit($scope.model);
        }
    }

    function close() {
        if ($scope.model.close) {
            $scope.model.close();
        }
    }

    vm.showEmptyState = false;
    vm.showConfig = false;
    vm.showStyles = false;

    $scope.$watchCollection('model.config', onWatch);
    $scope.$watchCollection('model.styles', onWatch);

    function onWatch() {

        vm.showConfig = $scope.model.config &&
            ($scope.model.config.length > 0 || Object.keys($scope.model.config).length > 0);
        vm.showStyles = $scope.model.styles &&
            ($scope.model.styles.length > 0 || Object.keys($scope.model.styles).length > 0);

        vm.showEmptyState = vm.showConfig === false && vm.showStyles === false;

    }

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.GridPrevalueEditor.ConfigController", ConfigController);
