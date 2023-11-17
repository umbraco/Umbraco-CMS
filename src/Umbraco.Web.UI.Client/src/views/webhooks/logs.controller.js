(function () {
  "use strict";

  function WebhookLogController($q, webhooksResource, overlayService) {

    const vm = this;

    vm.logs = [];
    vm.openLogOverlay = openLogOverlay;
    vm.isChecked = isChecked;

    function init() {
      vm.loading = true;

      let promises = [];

      promises.push(loadLogs());

      $q.all(promises).then(function () {
        vm.loading = false;
      });
    }

    function loadLogs() {
      return webhooksResource.getLogs()
        .then(data => {
          vm.logs = data.items;
        });
    }

    function openLogOverlay(log) {
      overlayService.open({
        view: "views/webhooks/overlays/details.html",
        title: 'Details',
        position: 'right',
        log,
        currentUser: this.currentUser,
        close: () => {
          overlayService.close();
        }
      });
    }

    function isChecked(log) {
      return log.statusCode === "OK (200)";
    }

    init();
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.WebhookLogController", WebhookLogController);

})();
