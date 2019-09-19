function textboxController($scope) {
    // macro parameter editor doesn't contains a config object,
    // so we create a new one to hold any properties
    if (!$scope.model.config) {
        $scope.model.config = {};
    }

    $scope.model.maxlength = false;
    if ($scope.model.config && $scope.model.config.maxChars) {
        $scope.model.maxlength = true;
    }

    if (!$scope.model.config.maxChars) {
        // 500 is the maximum number that can be stored
        // in the database, so set it to the max, even
        // if no max is specified in the config
        $scope.model.config.maxChars = 500;
    }

    if ($scope.model.maxlength) {
        if ($scope.model.value === undefined) {
            $scope.model.count = ($scope.model.config.maxChars * 1);
        } else {
            $scope.model.count = ($scope.model.config.maxChars * 1) - $scope.model.value.length;
        }
    }

    $scope.model.change = function () {
        if ($scope.model.config && $scope.model.config.maxChars) {
            if ($scope.model.value === undefined) {
                $scope.model.count = ($scope.model.config.maxChars * 1);
            } else {
                $scope.model.count = ($scope.model.config.maxChars * 1) - $scope.model.value.length;
            }
            if ($scope.model.count < 0) {
                $scope.model.value = $scope.model.value.substring(0, ($scope.model.config.maxChars * 1));
                $scope.model.count = 0;
            }
        }
    }
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.textboxController", textboxController);
