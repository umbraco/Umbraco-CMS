/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function DocumentTypePropertyController($scope, dataTypeResource) {

	$scope.showTabs = false;
	var tabsLoaded = 0;

	$scope.tabs = [
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

	function activate() {

		getAllUserConfiguredDataTypes();
		getAllTypesAndEditors();

	}

	function getAllTypesAndEditors() {

		dataTypeResource.getAllTypesAndEditors().then(function(data){
			$scope.tabs[0].typesAndEditors = data;
			tabsLoaded = tabsLoaded + 1;
			checkIfTabContentIsLoaded();
		});

	}

	function getAllUserConfiguredDataTypes() {

		dataTypeResource.getAllUserConfigured().then(function(data){
			$scope.tabs[1].userConfigured = data;
			tabsLoaded = tabsLoaded + 1;
			checkIfTabContentIsLoaded();
		});

	}

	function checkIfTabContentIsLoaded() {
		if(tabsLoaded === 2) {
			$scope.showTabs = true;
		}
	}

	activate();

}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PropertyController", DocumentTypePropertyController);
