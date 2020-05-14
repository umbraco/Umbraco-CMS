function textboxController($scope, validationMessageService) {
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

    $scope.$on("formSubmitting", function() {
        if ($scope.isLengthValid()) {
            $scope.textboxFieldForm.textbox.$setValidity("maxChars", true);
        } else {
            $scope.textboxFieldForm.textbox.$setValidity("maxChars", false);
        }
    });

    $scope.isLengthValid = function() {
        return $scope.model.config.maxChars >= $scope.model.count;
    }

    $scope.model.change = function () {
        if ($scope.model.value) {
            $scope.model.count = $scope.model.value.length;
        }
    }
    $scope.model.change();

    // Set the message to use for when a mandatory field isn't completed.
    // Will either use the one provided on the property type or a localised default.
    validationMessageService.getMandatoryMessage($scope.model.validation).then(function(value) {
        $scope.mandatoryMessage = value;
    });    
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.textboxController", textboxController);
