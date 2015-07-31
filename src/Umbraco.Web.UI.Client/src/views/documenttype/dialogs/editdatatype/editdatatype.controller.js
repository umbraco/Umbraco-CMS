/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function EditDataTypeController($scope, dataTypeResource, dataTypeHelper) {

    var dataTypeNameCopy = "";

    $scope.model.multiActions = [
      {
        key: "save",
        label: "Save",
        defaultAction: true,
        action: function(model) {
          saveDataType($scope.model.dataType, false);
        }
      },
      {
        key: "saveAsNew",
        label: "Save as new",
        action: function(model) {
          saveDataType($scope.model.dataType, true);
        }
      }
    ];

    function activate() {
      makeDataTypeNameCopy()
    }

    function makeDataTypeNameCopy() {
      dataTypeNameCopy = angular.copy($scope.model.dataType.name);
    }

    $scope.dateTypeNameChange = function() {

      // change default button to save as new when data type name changes
      if( $scope.model.dataType.name !== dataTypeNameCopy) {
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

    function saveDataType(dataType, isNew) {

      var preValues = dataTypeHelper.createPreValueProps(dataType.preValues);

      dataTypeResource.save(dataType, preValues, isNew).then(function(dataType) {

        $scope.model.dataType = dataType;

        $scope.model.submit($scope.model, isNew);

      });

    }

    activate();

}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditDataTypeController", EditDataTypeController);
