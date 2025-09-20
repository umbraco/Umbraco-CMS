(function () {
  "use strict";

  function OverviewController($location, $routeParams, localizationService) {

    const vm = this;

    vm.page = {};
    vm.page.labels = {};
    vm.page.name = "";
    vm.page.navigation = [];

    let webhookUri = $routeParams.id;

    onInit();

    function onInit() {

      loadNavigation();

      setPageName();
    }

    function loadNavigation() {

      const labelKeys = ["treeHeaders_webhooks", "webhooks_deliveries"];

      localizationService.localizeMany(labelKeys).then(data => {
        vm.page.labels.webhooks = data[0];
        vm.page.labels.logs = data[1];

        vm.page.navigation = [
          {
            "name": vm.page.labels.webhooks,
            "icon": "icon-webhook",
            "view": !webhookUri ? "views/webhooks/webhooks.html" : null,
            "active": !webhookUri,
            "alias": "umbWebhooks",
            "action": function () {
              $location.path("/settings/webhooks/overview");
            }
          },
          {
            "name": vm.page.labels.logs,
            "icon": "icon-box-alt",
            "view": webhookUri === 'logs' ? "views/webhooks/logs.html" : null,
            "active": webhookUri === 'logs',
            "alias": "umbWebhookLogs",
            "action": function () {
              $location.path("/settings/webhooks/overview/logs");
            }
          }
        ];
      });
    }

    function setPageName() {
      localizationService.localize("treeHeaders_webhooks").then(data => {
        vm.page.name = data;
      })
    }
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.OverviewController", OverviewController);

})();
