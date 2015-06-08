/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function EditPropertySettingsController($scope, contentTypeResource) {

    $scope.propertySettings = {};
    $scope.propertySettings.validationTypes = [];
    $scope.propertySettings.showValidationPattern = false;


    $scope.validationTypes = [
        {
            "name": "Validate as email",
            "pattern": "[a-zA-Z0-9_\.\+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-\.]+",
            "enableEditing": true
        },
        {
            "name": "Validate as a number",
            "pattern": "^[1-9]\d*$",
            "enableEditing": true
        },
        {
            "name": "Validate as a Url",
            "pattern": "https?\:\/\/[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,}",
            "enableEditing": true
        },
        {
            "name": "...or enter a custom validation",
            "pattern": "",
            "enableEditing": true
        }
    ];


    $scope.changeValidationType = function(selectedValidationType) {

        if($scope.model.property.validation === undefined) {
            $scope.model.property.validation = {};
        }

        $scope.model.property.validation.pattern = selectedValidationType.pattern;

        $scope.propertySettings.showValidationPattern = true;

    }


}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditPropertySettingsController", EditPropertySettingsController);
