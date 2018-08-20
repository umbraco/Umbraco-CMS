angular.module("umbraco").controller("Umbraco.PrevalueEditors.BooleanController",
    function ($scope) {

        function updateToggleValue() {
            $scope.toggleValue = false;

            if ($scope.model && $scope.model.value && ($scope.model.value.toString() === "1" || angular.lowercase($scope.model.value) === "true")) {
                $scope.toggleValue = true;
            }
        }

        if($scope.model.value === null){
            $scope.model.value = "0";
        }

        updateToggleValue();

        $scope.toggle = function(){
            if($scope.model.value === 1 || $scope.model.value === "1"){
                $scope.model.value = "0";
                updateToggleValue();

                return;
            }

            $scope.model.value = "1";

            updateToggleValue();
        }
    });
