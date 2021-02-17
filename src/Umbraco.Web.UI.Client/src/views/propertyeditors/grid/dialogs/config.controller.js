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

    $scope.showEmptyState = function() {
        return $scope.model.config.length === 0
            && Object.keys($scope.model.config).length === 0
            && $scope.model.styles.length === 0;
    }

    $scope.showConfig = function() {
        return $scope.model.config &&
            ($scope.model.config.length > 0 || Object.keys($scope.model.config).length > 0);
    }

    $scope.showStyles = function() {
        return $scope.model.styles &&
            ($scope.model.styles.length > 0 || Object.keys($scope.model.styles).length > 0);
    }

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.GridPrevalueEditor.ConfigController", ConfigController);
