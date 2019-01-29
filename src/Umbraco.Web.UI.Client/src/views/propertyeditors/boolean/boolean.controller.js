function booleanEditorController($scope, angularHelper) {

    function setupViewModel() {
        $scope.renderModel = {
            value: false
        };

        if ($scope.model.config && $scope.model.config.default && $scope.model.config.default.toString() === "1" && $scope.model && !$scope.model.value) {
            $scope.renderModel.value = true;
        }

        if ($scope.model && $scope.model.value && ($scope.model.value.toString() === "1" || angular.lowercase($scope.model.value) === "true")) {
            $scope.renderModel.value = true;
        }
    }

    setupViewModel();

    if( $scope.model && !$scope.model.value ) {
        $scope.model.value = ($scope.renderModel.value === true) ? '1' : '0';
    }

    //here we declare a special method which will be called whenever the value has changed from the server
    //this is instead of doing a watch on the model.value = faster
    $scope.model.onValueChanged = function (newVal, oldVal) {
        //update the display val again if it has changed from the server
        setupViewModel();
    };

    // Update the value when the toggle is clicked
    $scope.toggle = function () {
        angularHelper.getCurrentForm($scope).$setDirty();
        if($scope.renderModel.value){
            $scope.model.value = "0";
            setupViewModel();
            return;
        }

        $scope.model.value = "1";
        setupViewModel();
    };

}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BooleanController", booleanEditorController);
