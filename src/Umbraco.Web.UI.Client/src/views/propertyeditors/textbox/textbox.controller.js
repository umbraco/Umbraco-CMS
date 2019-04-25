function textboxController($scope) {
    // macro parameter editor doesn't contains a config object,
    // so we create a new one to hold any properties
    if (!$scope.model.config) {
        $scope.model.config = {};
    }
    $scope.model.count = 0;
    if (!$scope.model.config.maxChars) {
        // 500 is the maximum number that can be stored
        // in the database, so set it to the max, even
        // if no max is specified in the config
        $scope.model.config.maxChars = 500;
    }
    
    $scope.model.change = function () {
        if ($scope.model.value) {
            $scope.model.count = $scope.model.value.length;
        }
    }
    $scope.model.change();
    
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.textboxController", textboxController);
