function textAreaController($scope, validationMessageService) {

    // macro parameter editor doesn't contains a config object,
    // so we create a new one to hold any properties
    if (!$scope.model.config) {
        $scope.model.config = {};
    }

    $scope.maxChars = $scope.model.config.maxChars || 0;
    $scope.maxCharsLimit = ($scope.model.config && $scope.model.config.maxChars > 0);
    $scope.charsCount = 0;
    $scope.nearMaxLimit = false;
    $scope.validLength = true;

    $scope.$on("formSubmitting", function() {
        if ($scope.validLength) {
            $scope.textareaFieldForm.textarea.$setValidity("maxChars", true);
        } else {
            $scope.textareaFieldForm.textarea.$setValidity("maxChars", false);
        }
    });

    function checkLengthVadility() {
        $scope.validLength = !($scope.maxCharsLimit === true && $scope.charsCount > $scope.maxChars);
    }

    $scope.change = function () {
        if ($scope.model.value) {
            $scope.charsCount = $scope.model.value.length;
            checkLengthVadility();
            $scope.nearMaxLimit = $scope.maxCharsLimit === true && $scope.validLength === true && $scope.charsCount > Math.max($scope.maxChars*.8, $scope.maxChars-50);
        }
    }
    $scope.model.onValueChanged = $scope.change;
    $scope.change();

    // Set the message to use for when a mandatory field isn't completed.
    // Will either use the one provided on the property type or a localised default.
    validationMessageService.getMandatoryMessage($scope.model.validation).then(function (value) {
        $scope.mandatoryMessage = value;
    });
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.textAreaController", textAreaController);
