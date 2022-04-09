angular.module("umbraco")
  .controller("Umbraco.Editors.Dictionary.ImportController",
    function ($scope, dictionaryResource, notificationsService, navigationService, Upload, umbRequestHelper) {
      var vm = this;

      vm.state = "upload";
      vm.model = {};
      vm.uploadStatus = "";

      vm.cancelButtonLabel = "cancel";

      $scope.handleFiles = function (files, event, invalidFiles) {
        if (files && files.length > 0) {
          $scope.upload(files[0]);
        }
      };

      $scope.upload = function (file) {
        Upload.upload({
          url: umbRequestHelper.getApiUrl("dictionaryApiBaseUrl", "Upload"),
          fields: {},
          file: file
        }).then(function (response) {

          vm.model = response.data;
          vm.state = "confirm";
          vm.uploadStatus = "done";

        }, function (err) {
          notificationsService.error(err.data.notifications[0].header, err.data.notifications[0].message);
        });
      };

      $scope.import = function () {
        dictionaryResource.importItem(vm.model.tempFileName).then(function (path) {
          navigationService.syncTree({ tree: "dictionary", path: path, forceReload: true, activate: false });

          vm.state = "done";
          vm.cancelButtonLabel = "general_close";
        });
      };

      $scope.close = function () {
        navigationService.hideDialog();
      };

    });
