(function () {
    "use strict";

    function PickDynamicRootOriginController($scope, localizationService, editorService, udiParser) {

        var vm = this;

        function onInit() {

            if(!$scope.model.title) {
                localizationService.localize("dynamicRoot_pickDynamicRootOriginTitle").then(function(value){
                    $scope.model.title = value;
                });
            }
            if(!$scope.model.subtitle) {
                localizationService.localize("dynamicRoot_pickDynamicRootOriginDesc").then(function(value){
                    $scope.model.subtitle = value;
                });
            }

        }

        vm.chooseRoot = function() {
          $scope.model.value.originAlias = "Root";
          $scope.model.value.originKey = null;
          vm.submit($scope.model);
        }
        vm.chooseParent = function() {
          $scope.model.value.originAlias = "Parent";
          $scope.model.value.originKey = null;
          vm.submit($scope.model);
        }
        vm.chooseCurrent = function() {
          $scope.model.value.originAlias = "Current";
          $scope.model.value.originKey = null;
          vm.submit($scope.model);
        }
        vm.chooseSite = function() {
          $scope.model.value.originAlias = "Site";
          $scope.model.value.originKey = null;
          vm.submit($scope.model);
        }
        vm.chooseByKey = function() {
          var treePicker = {
            idType: "udi",
            section: $scope.model.contentType,
            treeAlias: $scope.model.contentType,
            multiPicker: false,
            submit: function(model) {
              var item = model.selection[0];
              $scope.model.value.originAlias = "ByKey";
              $scope.model.value.originKey = udiParser.parse(item.udi).value;
              editorService.close();
              vm.submit($scope.model);
            },
            close: function() {
              editorService.close();
            }
          };
          editorService.treePicker(treePicker);
        }

        vm.submit = submit;
        function submit(model) {
          if ($scope.model.submit) {
              $scope.model.submit(model);
          }
        }

        vm.close = close;
        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PickDynamicRootOrigin", PickDynamicRootOriginController);
})();
