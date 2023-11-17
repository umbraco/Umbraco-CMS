(function () {
  "use strict";
  function DetailsController() {
    const vm = this;

    vm.formatData = formatData;

    function formatData(data) {

      let obj = null;

      if (data.detectIsJson()) {
        try {
          obj = Utilities.fromJson(data)
        } catch (err) {
          obj = data;
        }
      }

      return obj;
    }

  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.DetailsController", DetailsController);
})();
