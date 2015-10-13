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

	function EditorPickerOverlay($scope, dataTypeResource, dataTypeHelper, contentTypeResource) {

		var vm = this;

		vm.searchTerm = "";
		vm.showTabs = false;
		vm.tabsLoaded = 0;
		vm.tabs = [
			{
				active: true,
				id: 1,
				label: "Default",
				alias: "Default",
				typesAndEditors: []
			},
			{
				active: false,
				id: 2,
				label: "Reuse",
				alias: "Reuse",
				userConfigured: []
			}
		];

      vm.filterItems = filterItems;
		vm.showDetailsOverlay = showDetailsOverlay;
		vm.hideDetailsOverlay = hideDetailsOverlay;
      vm.pickEditor = pickEditor;

		function activate() {

			getAllUserConfiguredDataTypes();
			getAllTypesAndEditors();

		}

		function getAllTypesAndEditors() {

			dataTypeResource.getAllTypesAndEditors().then(function(data){
				vm.tabs[0].typesAndEditors = data;
				vm.tabsLoaded = vm.tabsLoaded + 1;
				checkIfTabContentIsLoaded();
			});

		}

		function getAllUserConfiguredDataTypes() {

			dataTypeResource.getAllUserConfigured().then(function(data){
				vm.tabs[1].userConfigured = data;
				vm.tabsLoaded = vm.tabsLoaded + 1;
				checkIfTabContentIsLoaded();
			});

		}

		function checkIfTabContentIsLoaded() {
			if(vm.tabsLoaded === 2) {
				vm.showTabs = true;
			}
		}

      function filterItems() {
         // clear item details
         $scope.model.itemDetails = null;
      }

		function showDetailsOverlay(property) {

			var propertyDetails = {};
			propertyDetails.icon = property.icon;
			propertyDetails.title = property.name;

			$scope.model.itemDetails = propertyDetails;

		}

		function hideDetailsOverlay() {
			$scope.model.itemDetails = null;
		}

      function pickEditor(editor) {

         if(editor.id === null) {

            dataTypeResource.getScaffold().then(function(dataType) {

              // set alias
              dataType.selectedEditor = editor.alias;

              // set name
              var nameArray = [];

              if($scope.model.contentTypeName) {
                nameArray.push($scope.model.contentTypeName);
              }

              if($scope.model.property.label) {
                nameArray.push($scope.model.property.label);
              }

              if(editor.name) {
                nameArray.push(editor.name);
              }

              // make name
              dataType.name = nameArray.join(" - ");

              // get pre values
              dataTypeResource.getPreValues(dataType.selectedEditor).then(function(preValues) {

                dataType.preValues = preValues;

                openEditorSettingsOverlay(dataType, true);

              });

            });

         } else {

            dataTypeResource.getById(editor.id).then(function(dataType) {

               contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType) {

                  submitOverlay(dataType, propertyType, false);

               });

            });

         }

      }

      function openEditorSettingsOverlay(dataType, isNew) {
         vm.editorSettingsOverlay = {};
         vm.editorSettingsOverlay.title = "Editor settings";
         vm.editorSettingsOverlay.dataType = dataType;
         vm.editorSettingsOverlay.view = "views/common/overlays/contenttypeeditor/editorsettings/editorsettings.html";
         vm.editorSettingsOverlay.show = true;

         vm.editorSettingsOverlay.submit = function(model) {

            var preValues = dataTypeHelper.createPreValueProps(model.dataType.preValues);

            dataTypeResource.save(model.dataType, preValues, isNew).then(function(newDataType) {

               contentTypeResource.getPropertyTypeScaffold(newDataType.id).then(function(propertyType) {

                  submitOverlay(newDataType, propertyType, true);

                  vm.editorSettingsOverlay.show = false;
                  vm.editorSettingsOverlay = null;

               });

            });

         };

         vm.editorSettingsOverlay.close = function(oldModel) {
            vm.editorSettingsOverlay.show = false;
            vm.editorSettingsOverlay = null;
         };

      }

      function submitOverlay(dataType, propertyType, isNew) {

         // update property
         $scope.model.property.config = propertyType.config;
         $scope.model.property.editor = propertyType.editor;
         $scope.model.property.view = propertyType.view;
         $scope.model.property.dataTypeId = dataType.id;
         $scope.model.property.dataTypeIcon = dataType.icon;
         $scope.model.property.dataTypeName = dataType.name;

         $scope.model.updateSameDataTypes = isNew;

         $scope.model.submit($scope.model);

      }

		activate();

	}

	angular.module("umbraco").controller("Umbraco.Overlays.EditorPickerOverlay", EditorPickerOverlay);

})();
