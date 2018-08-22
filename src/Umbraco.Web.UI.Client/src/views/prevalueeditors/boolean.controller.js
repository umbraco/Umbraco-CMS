angular.module("umbraco").controller("Umbraco.PrevalueEditors.BooleanController",
    function ($scope) {

        function updateToggleValue() {
            $scope.toggleValue = false;

            if ($scope.model && Object.toBoolean($scope.model.value)) {
                $scope.toggleValue = true;
            }
        }

        if($scope.model.value === null){
            $scope.model.value = "0";
        }

        updateToggleValue();

        $scope.toggle = function(){
            if (Object.toBoolean($scope.model.value)) {
                $scope.model.value = "0";
                updateToggleValue();

                return;
            }

            $scope.model.value = "1";

            updateToggleValue();
        }
    });
