/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataTypeSettingsController
 * @function
 *
 * @description
 * The controller for the content type editor data type settings dialog
 */

(function () {
  "use strict";

  function DataTypeSettingsController($scope, dataTypeResource, dataTypeHelper,
                                      localizationService, notificationsService, overlayService, formHelper, eventsService) {

    var vm = this;

    vm.dataType = {};
    vm.loadingDataType = false;
    vm.saveButtonState = "init";

    vm.close = close;
    vm.submit = submit;

    function onInit() {

      setTitle();

      if($scope.model.create) {
        createNewDataType();
      } else {
        getDataType();
      }
    }

    function setTitle() {
      if(!$scope.model.title) {
        localizationService.localize("contentTypeEditor_editorSettings")
          .then(function(data){
            $scope.model.title = data;
          });
      }
    }

    function createNewDataType() {

      vm.loadingDataType = true;

      var parentId = -1;
      var newDataType = {};

      dataTypeResource.getScaffold(parentId).then(function(dataType) {

        newDataType = dataType;

        // set alias
        newDataType.selectedEditor = $scope.model.propertyEditor.alias;

        // set name
        var nameArray = [];

        if ($scope.model.contentTypeName) {
          nameArray.push($scope.model.contentTypeName);
        }

        if ($scope.model.property.label) {
          nameArray.push($scope.model.property.label);
        }

        if ($scope.model.propertyEditor.name) {
          nameArray.push($scope.model.propertyEditor.name);
        }

        // make name
        newDataType.name = nameArray.join(" - ");

        // get pre values
        dataTypeResource.getPreValues(newDataType.selectedEditor).then(function(preValues) {
          newDataType.preValues = preValues;
          vm.dataType = newDataType;
          vm.loadingDataType = false;
        });

      });

    }

    function getDataType() {
      vm.loadingDataType = true;
      dataTypeResource.getById($scope.model.id).then(function (dataType) {
        vm.dataType = dataType;
        vm.loadingDataType = false;
      });
    }

    function close() {
      if ($scope.model && $scope.model.close) {
        $scope.model.close();
      }
    }

    function submit() {
      if (!formHelper.submitForm({ scope: $scope })) {
        return;
      }

      vm.saveButtonState = "busy";

      var preValues = dataTypeHelper.createPreValueProps(vm.dataType.preValues);

      dataTypeResource.save(vm.dataType, preValues, $scope.model.create).then(
        function(newDataType) {
          $scope.model.dataType = newDataType;

          var args = { dataType: newDataType };
          eventsService.emit("editors.dataTypeSettings.saved", args);

          vm.saveButtonState = "success";

          if ($scope.model && $scope.model.submit) {
            $scope.model.submit($scope.model);
          }
        }, function(err) {
          vm.saveButtonState = "error";

          if(err.status === 400) {
            if (err.data && (err.data.ModelState)) {

              formHelper.handleServerValidation(err.data.ModelState);

              for (var e in err.data.ModelState) {
                notificationsService.error("Validation", err.data.ModelState[e][0]);
              }
            }
          }
        }
      );

    }

    onInit();

  }

  angular.module("umbraco").controller("Umbraco.Editors.DataTypeSettingsController", DataTypeSettingsController);

})();
