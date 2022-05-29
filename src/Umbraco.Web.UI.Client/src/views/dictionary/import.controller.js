angular.module("umbraco")
  .controller("Umbraco.Editors.Dictionary.ImportController",
    function ($scope, dictionaryResource, notificationsService, navigationService, Upload, umbRequestHelper) {
      var vm = this;

      vm.state = "upload";
      vm.model = {};
      vm.uploadStatus = "";

      vm.cancelButtonLabel = "cancel";

      $scope.dialogTreeApi = {};

      function nodeSelectHandler(args) {
        args.event.preventDefault();
        args.event.stopPropagation();

        if ($scope.target) {
          //un-select if there's a current one selected
          $scope.target.selected = false;
        }
        $scope.target = args.node;
        $scope.target.selected = true;
      }

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
        var parentId = $scope.target !== undefined ? $scope.target.id : 0;

        dictionaryResource.importItem(vm.model.tempFileName, parentId).then(function (path) {
          navigationService.syncTree({ tree: "dictionary", path: path, forceReload: true, activate: false });

          vm.state = "done";
          vm.cancelButtonLabel = "general_close";
        });
      };

      $scope.onTreeInit = function () {
        $scope.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
      };

      $scope.close = function () {
        navigationService.hideDialog();
      };

    });
