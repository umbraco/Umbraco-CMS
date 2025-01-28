(function () {
    "use strict";

    function PickDynamicRootOriginController($scope, localizationService, editorService, udiParser) {

        const vm = this;

        vm.chooseRoot = chooseRoot;
        vm.chooseParent = chooseParent;
        vm.chooseCurrent = chooseCurrent;
        vm.chooseSite = chooseSite;
        vm.chooseByKey = chooseByKey;

        vm.submit = submit;
        vm.close = close;

        function onInit() {

            if (!$scope.model.title) {
                localizationService.localize("dynamicRoot_pickDynamicRootOriginTitle").then(value => {
                    $scope.model.title = value;
                });
            }
            if (!$scope.model.subtitle) {
                localizationService.localize("dynamicRoot_pickDynamicRootOriginDesc").then(value => {
                    $scope.model.subtitle = value;
                });
            }

        }

        function chooseRoot() {
          $scope.model.value.originAlias = "Root";
          $scope.model.value.originKey = null;
          vm.submit($scope.model);
        }

        function chooseParent() {
          $scope.model.value.originAlias = "Parent";
          $scope.model.value.originKey = null;
          vm.submit($scope.model);
        }

        function chooseCurrent() {
          $scope.model.value.originAlias = "Current";
          $scope.model.value.originKey = null;
          vm.submit($scope.model);
        }

        function chooseSite() {
          $scope.model.value.originAlias = "Site";
          $scope.model.value.originKey = null;
          vm.submit($scope.model);
        }

        function chooseByKey() {
          const editor = {
            idType: "udi",
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

          switch ($scope.model.contentType) {
            case "content":
              editorService.contentPicker(editor);
              break;
            case "media":
              editorService.mediaPicker(editor);
              break;
            case "member":
              editorService.memberPicker(editor);
              break;
          }
        }
        
        function submit(model) {
          if ($scope.model.submit) {
              $scope.model.submit(model);
          }
        }
        
        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PickDynamicRootOrigin", PickDynamicRootOriginController);
})();
