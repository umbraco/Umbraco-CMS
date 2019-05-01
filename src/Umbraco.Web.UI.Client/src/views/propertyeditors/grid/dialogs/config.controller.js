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
    
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.GridPrevalueEditor.ConfigController", ConfigController);
