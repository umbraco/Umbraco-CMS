/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */

 (function() {
 	"use strict";

   function PropertySettingsOverlay($scope, contentTypeResource, dataTypeResource, dataTypeHelper, localizationService) {

      var vm = this;

      vm.showValidationPattern = false;
      vm.focusOnPatternField = false;
      vm.focusOnMandatoryField = false;
      vm.selectedValidationType = {};
      vm.validationTypes = [
         {
            "name": localizationService.localize("validation_validateAsEmail"),
            "key": "email",
            "pattern": "[a-zA-Z0-9_\.\+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-\.]+",
            "enableEditing": true
         },
         {
            "name": localizationService.localize("validation_validateAsNumber"),
            "key": "number",
            "pattern": "^[0-9]*$",
            "enableEditing": true
         },
         {
            "name": localizationService.localize("validation_validateAsUrl"),
            "key": "url",
            "pattern": "https?\:\/\/[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,}",
            "enableEditing": true
         },
         {
            "name": localizationService.localize("validation_enterCustomValidation"),
            "key": "custom",
            "pattern": "",
            "enableEditing": true
         }
      ];

      vm.changeValidationType = changeValidationType;
      vm.changeValidationPattern = changeValidationPattern;
      vm.openEditorPickerOverlay = openEditorPickerOverlay;
      vm.openEditorSettingsOverlay = openEditorSettingsOverlay;

      function activate() {

         matchValidationType();

      }

      function changeValidationPattern() {
         matchValidationType();
      }

      function openEditorPickerOverlay(property) {

         vm.focusOnMandatoryField = false;

         vm.editorPickerOverlay = {};
         vm.editorPickerOverlay.property = $scope.model.property;
         vm.editorPickerOverlay.contentTypeName = $scope.model.contentTypeName;
         vm.editorPickerOverlay.view = "views/common/overlays/contenttypeeditor/editorpicker/editorpicker.html";
         vm.editorPickerOverlay.show = true;

         vm.editorPickerOverlay.submit = function(model) {

            $scope.model.updateSameDataTypes = model.updateSameDataTypes;

            vm.focusOnMandatoryField = true;

            // update property
            property.config = model.property.config;
            property.editor = model.property.editor;
            property.view = model.property.view;
            property.dataTypeId = model.property.dataTypeId;
            property.dataTypeIcon = model.property.dataTypeIcon;
            property.dataTypeName = model.property.dataTypeName;

            vm.editorPickerOverlay.show = false;
            vm.editorPickerOverlay = null;
         };

         vm.editorPickerOverlay.close = function(model) {
            vm.editorPickerOverlay.show = false;
            vm.editorPickerOverlay = null;
         };

      }

      function openEditorSettingsOverlay(property) {

         vm.focusOnMandatoryField = false;

         // get data type
         dataTypeResource.getById(property.dataTypeId).then(function(dataType) {

            vm.editorSettingsOverlay = {};
            vm.editorSettingsOverlay.title = "Editor settings";
            vm.editorSettingsOverlay.view = "views/common/overlays/contenttypeeditor/editorsettings/editorsettings.html";
            vm.editorSettingsOverlay.dataType = dataType;
            vm.editorSettingsOverlay.show = true;

            vm.editorSettingsOverlay.submit = function(model) {

               var preValues = dataTypeHelper.createPreValueProps(model.dataType.preValues);

               dataTypeResource.save(model.dataType, preValues, false).then(function(newDataType) {

                  contentTypeResource.getPropertyTypeScaffold(newDataType.id).then(function(propertyType) {

                     // update editor
                     property.config = propertyType.config;
                     property.editor = propertyType.editor;
                     property.view = propertyType.view;
                     property.dataTypeId = newDataType.id;
                     property.dataTypeIcon = newDataType.icon;
                     property.dataTypeName = newDataType.name;

                     // set flag to update same data types
                     $scope.model.updateSameDataTypes = true;

                     vm.focusOnMandatoryField = true;

                     vm.editorSettingsOverlay.show = false;
                     vm.editorSettingsOverlay = null;

                  });

               });

            };

            vm.editorSettingsOverlay.close = function(oldModel) {
               vm.editorSettingsOverlay.show = false;
               vm.editorSettingsOverlay = null;
            };

         });

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
            $scope.model.property.validation.pattern = "";
            vm.showValidationPattern = false;
         }

      }

      activate();

   }

   angular.module("umbraco").controller("Umbraco.Overlay.PropertySettingsOverlay", PropertySettingsOverlay);

})();
