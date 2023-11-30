(function () {
  "use strict";
  function DetailsController($scope) {
    const vm = this;

    vm.close = close;
    vm.formatData = formatData;

    function formatData(data) {

      let obj = data;

      if (data.detectIsJson()) {
        try {
          obj = Utilities.fromJson(data)
        } catch (err) {
          obj = data;
        }
      }

      return obj;
    }

    function close() {
      if ($scope.model && $scope.model.close) {
        $scope.model.close();
      }
    }

  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.DetailsController", DetailsController);
})();
