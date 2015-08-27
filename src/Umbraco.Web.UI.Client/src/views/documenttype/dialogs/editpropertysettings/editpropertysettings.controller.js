/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function EditPropertySettingsController($scope, contentTypeResource) {

   var vm = this;

   vm.showValidationPattern = false;
   vm.focusOnPatternField = false;
   vm.selectedValidationType = {};
   vm.validationTypes = [
      {
         "name": "Validate as email",
         "key": "email",
         "pattern": "[a-zA-Z0-9_\.\+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-\.]+",
         "enableEditing": true
      },
      {
         "name": "Validate as a number",
         "key": "number",
         "pattern": "^[0-9]*$",
         "enableEditing": true
      },
      {
         "name": "Validate as a Url",
         "key": "url",
         "pattern": "https?\:\/\/[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,}",
         "enableEditing": true
      },
      {
         "name": "...or enter a custom validation",
         "key": "custom",
         "pattern": "",
         "enableEditing": true
      }
   ];

   vm.changeValidationType = changeValidationType;

   function activate() {

      matchValidationType();

   }

   function matchValidationType() {

      if($scope.model.property.validation.pattern !== null && $scope.model.property.validation.pattern !== "" && $scope.model.property.validation.pattern !== undefined) {

         var match = false;

         // find and show if a match from the list has been chosen
         angular.forEach(vm.validationTypes, function(validationType, index){
            if($scope.model.property.validation.pattern === validationType.pattern) {
               vm.selectedValidationType = vm.validationTypes[index];
               vm.showValidationPattern = true;
               match = true;
            }
         });

         // if there is no match - choose the custom validation option.
         if(!match) {
            angular.forEach(vm.validationTypes, function(validationType){
               if(validationType.key === "custom") {
                  vm.selectedValidationType = validationType;
                  vm.showValidationPattern = true;
               }
            });
         }
      }

   }

   function changeValidationType(selectedValidationType) {

      if(selectedValidationType) {
         $scope.model.property.validation.pattern = selectedValidationType.pattern;
         vm.showValidationPattern = true;

         // set focus on textarea
         if(selectedValidationType.key === "custom") {
            vm.focusOnPatternField = true;
         }

      } else {
         vm.showValidationPattern = false;
      }

   }

   activate();

}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditPropertySettingsController", EditPropertySettingsController);
