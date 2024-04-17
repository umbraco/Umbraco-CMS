(function () {
  "use strict";
  function HeaderController($scope) {
    const vm = this;

    vm.submit = submit;
    vm.close = close;

    vm.headers = ["Accept", "Content-Type", "User-Agent", "Content-Length"];

    $scope.headerModel = { key: "", value: "" };

    function submit() {
      if ($scope.headerModel.key && $scope.headerModel.value) {
        $scope.model.submit($scope.headerModel);
      }
    }

    function close() {
      $scope.model.close();
    }
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.HeaderController", HeaderController);
})();
