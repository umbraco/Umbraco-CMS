(function () {
  "use strict";
  function HeaderController($scope) {
    var vm = this;
    $scope.headerModel = { key: "", value: "" };
    vm.submit = submit;
    vm.close = close;

    function submit () {
      if ($scope.headerModel.key && $scope.headerModel.value) {
        $scope.model.submit($scope.headerModel);
      }
    }

    function close () {
      $scope.model.close();
    }
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.HeaderController", HeaderController);
})();
