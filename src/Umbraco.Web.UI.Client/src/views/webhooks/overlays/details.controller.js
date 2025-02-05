(function () {
  "use strict";
  function DetailsController($scope) {
    const vm = this;

    vm.close = close;
    vm.formatData = formatData;
    vm.detectLanguage = detectLanguage;

    function formatData(data) {

      let obj = data;

      if (data.detectIsJson()) {
        try {
          obj = JSON.stringify(Utilities.fromJson(data), null, 2);
        } catch (err) {
          obj = data;
        }
      }

      return obj;
    }

    function detectLanguage(headers, defaultLanguage) {
      const matches = headers.match(/^Content-Type:\s*(?<type>[a-z\/+.-]+)(\;?.*?)$/mi)
      if (matches) {
        const contentType = matches.groups["type"];
        if (contentType === "application/json")
          return "JSON";
        if (contentType === "text/html")
          return "HTML";
        if (contentType === "application/xml" || contentType === "text/xml")
          return "XML";
      }

      return defaultLanguage || "TEXT";
    }

    function close() {
      if ($scope.model && $scope.model.close) {
        $scope.model.close();
      }
    }

  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.DetailsController", DetailsController);
})();
