function textAreaController($scope, validationMessageService) {

    // macro parameter editor doesn't contains a config object,
    // so we create a new one to hold any properties 
    if (!$scope.model.config) {
        $scope.model.config = {};
    }
    $scope.model.count = 0;

    if (!$scope.model.config.maxChars) {
        $scope.model.config.maxChars = false;
    }

    $scope.model.maxlength = false;
    if ($scope.model.config && $scope.model.config.maxChars) {
        $scope.model.maxlength = true;
    }
    
    $scope.model.change = function () {
        if ($scope.model.value) {
            $scope.model.count = $scope.model.value.length;
        }
    }
    $scope.model.change();

    // Set the message to use for when a mandatory field isn't completed.
    // Will either use the one provided on the property type or a localised default.
    validationMessageService.getMandatoryMessage($scope.model.validation).then(function (value) {
        $scope.mandatoryMessage = value;
    });
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.textAreaController", textAreaController);
