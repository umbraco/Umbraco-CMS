/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function EditDataTypeController($scope, dataTypeResource) {

    var dataTypeCopy = {};

    getDataType($scope.model.property.dataTypeId);

    function getDataType(dataTypeId) {
        dataTypeResource.getById(dataTypeId)
            .then(function(dataType) {
                dataTypeCopy = angular.copy(dataType);
                $scope.model.dataType = dataType;
            });
    }

    $scope.dateTypeNameChange = function() {

      // change default button to save as new when data type name changes
      if( $scope.model.dataType.name !== dataTypeCopy.name) {
        setDefaultMultiAction($scope.model.multiActions, "saveAsNew");
      } else {
        setDefaultMultiAction($scope.model.multiActions, "save");
      }

    };

    function setDefaultMultiAction(array, key) {

      angular.forEach(array, function(arrayItem){
        if(arrayItem.key === key) {
          arrayItem.defaultAction = true;
        } else {
          arrayItem.defaultAction = false;
        }
      });

    }

}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditDataTypeController", EditDataTypeController);
