function emailController($scope, validationMessageService) {
    
    // Set the message to use for when a mandatory field isn't completed.
    // Will either use the one provided on the property type or a localised default.
    validationMessageService.getMandatoryMessage($scope.model.validation).then(function(value) {
        $scope.mandatoryMessage = value;
    }); 
    
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.EmailController", emailController);
