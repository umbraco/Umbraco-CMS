/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function DocumentTypePropertyController($scope, dataTypeResource) {

	$scope.dataTypes = {
		"userConfigured": [],
		"userPropertyEditors": [],
		"system": []
	};

	//getAllUserConfiguredDataTypes();
	//getAllUserPropertyEditors();
	getAllDatatypes();


	function getAllDatatypes() {

		dataTypeResource.getAllTypesAndEditors().then(function(data){
			$scope.dataTypes.system = data;
		});

//		dataTypeResource.getAll().then(function(data){
//			$scope.dataTypes.system = data;
//		});

	}

	function getAllUserConfiguredDataTypes() {

		dataTypeResource.getAllUserConfigured().then(function(data){
			$scope.dataTypes.userConfigured = data;
		});

	}

	function getAllUserPropertyEditors() {

		dataTypeResource.getAllUserPropertyEditors().then(function(data){
			$scope.dataTypes.userPropertyEditors = data;
		});

	}

}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PropertyController", DocumentTypePropertyController);
