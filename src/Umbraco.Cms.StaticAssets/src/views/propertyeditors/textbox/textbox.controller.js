function textboxController($scope, validationMessageService) {
    // macro parameter editor doesn't contains a config object,
    // so we create a new one to hold any properties
    if (!$scope.model.config) {
        $scope.model.config = {};
    }

    // 512 is the maximum number that can be stored
    // in the database, so set it to the max, even
    // if no max is specified in the config
    $scope.maxChars = Math.min($scope.model.config.maxChars || 512, 512);
    $scope.charsCount = 0;
    $scope.nearMaxLimit = false;
    $scope.validLength = true;

    $scope.$on("formSubmitting", function() {
        if ($scope.validLength === true) {
            $scope.textboxFieldForm.textbox.$setValidity("maxChars", true);
        } else {
            $scope.textboxFieldForm.textbox.$setValidity("maxChars", false);
        }
    });

    function checkLengthVadility() {
        $scope.validLength = $scope.charsCount <= $scope.maxChars;
    }

    $scope.change = function () {
        if ($scope.model.value) {
            $scope.charsCount = $scope.model.value.length;
            checkLengthVadility();
            $scope.nearMaxLimit = $scope.validLength && $scope.charsCount > Math.max($scope.maxChars*.8, $scope.maxChars-25);
        }
        else {
            $scope.charsCount = 0;
            checkLengthVadility();
        }
    }
    $scope.model.onValueChanged = $scope.change;
    $scope.change();

    // Set the message to use for when a mandatory field isn't completed.
    // Will either use the one provided on the property type or a localised default.
    validationMessageService.getMandatoryMessage($scope.model.validation).then(function(value) {
        $scope.mandatoryMessage = value;
    });
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.textboxController", textboxController);
