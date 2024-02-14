(function () {
    "use strict";

    function PickDynamicRootCustomStepController($scope, localizationService) {

        const vm = this;

        vm.submit = submit;
        vm.close = close;

        function onInit() {
            if (!$scope.model.title) {
                localizationService.localize("dynamicRoot_pickDynamicRootQueryStepTitle").then(value => {
                    $scope.model.title = value;
                });
            }
            if (!$scope.model.subtitle) {
                localizationService.localize("dynamicRoot_pickDynamicRootQueryStepDesc").then(value => {
                    $scope.model.subtitle = value;
                });
            }
        }
        
        function submit() {
          if ($scope.model.submit) {
              $scope.model.submit($scope.model);
          }
        }

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PickDynamicRootCustomStep", PickDynamicRootCustomStepController);
})();
