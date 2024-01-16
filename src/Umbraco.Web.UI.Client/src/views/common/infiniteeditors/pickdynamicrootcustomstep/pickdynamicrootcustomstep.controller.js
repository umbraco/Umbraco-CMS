(function () {
    "use strict";

    function PickDynamicRootCustomStepController($scope, localizationService) {

        var vm = this;

        function onInit() {
            if(!$scope.model.title) {
                localizationService.localize("dynamicRoot_pickDynamicRootQueryStepTitle").then(function(value){
                    $scope.model.title = value;
                });
            }
            if(!$scope.model.subtitle) {
                localizationService.localize("dynamicRoot_pickDynamicRootQueryStepDesc").then(function(value){
                    $scope.model.subtitle = value;
                });
            }
        }

        vm.submit = submit;
        function submit() {
          if ($scope.model.submit) {
              $scope.model.submit($scope.model);
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

    angular.module("umbraco").controller("Umbraco.Editors.PickDynamicRootCustomStep", PickDynamicRootCustomStepController);
})();
