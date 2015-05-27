/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function EditDataTypeController($scope, dataTypeResource) {

    getDataType($scope.model.property.dataTypeId);

    function getDataType(dataTypeId) {
        dataTypeResource.getById(dataTypeId)
            .then(function(dataType) {
                $scope.model.dataType = dataType;
            });
    }

}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditDataTypeController", EditDataTypeController);
