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

	function DocumentTypePropertyController($scope, dataTypeResource) {

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

		vm.showDetailsOverlay = showDetailsOverlay;
		vm.hideDetailsOverlay = hideDetailsOverlay;

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

		function showDetailsOverlay(property) {

			var propertyDetails = {};
			propertyDetails.icon = property.icon;
			propertyDetails.title = property.name;

			$scope.model.itemDetails = propertyDetails;

		};

		function hideDetailsOverlay() {
			$scope.model.itemDetails = null;
		}

		activate();

	}

	angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PropertyController", DocumentTypePropertyController);

})();
