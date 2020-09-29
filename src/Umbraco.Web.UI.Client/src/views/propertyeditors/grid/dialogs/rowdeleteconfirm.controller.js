function DeleteRowConfirmController($scope) {

    $scope.close = function() {
        if($scope.model.close) {
            $scope.model.close();
        }
    }
    
    $scope.submit = function() {
        if($scope.model && $scope.model.submit) {
            $scope.model.submit($scope.model);
        }
    }
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.GridPrevalueEditor.DeleteRowConfirmController", DeleteRowConfirmController);
