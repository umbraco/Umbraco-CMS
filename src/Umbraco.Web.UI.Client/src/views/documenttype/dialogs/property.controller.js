/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function DocumentTypePropertyController($scope, contentTypeResource) {

	/*
	$scope.selectDataType = function(dataType, model){
		contentTypeResource.getPropertyTypeScaffold(dataType.id)
			.then(function(pt){
				model.property.config = pt.config;
				model.property.editor = pt.config;
				model.property.view = pt.view;
				$scope.closeOverLay();
		});
	};
	*/
}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PropertyController", DocumentTypePropertyController);
