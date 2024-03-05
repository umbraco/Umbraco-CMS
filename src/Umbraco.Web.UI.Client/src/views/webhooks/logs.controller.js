(function () {
  "use strict";

  function WebhookLogController($q, webhooksResource, editorService, userService, dateHelper) {

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
          vm.logs.forEach(log => {
            formatDatesToLocal(log);
          });
        });
    }

    function formatDatesToLocal(log) {
      userService.getCurrentUser().then(currentUser => {
        log.formattedLogDate = dateHelper.getLocalDate(log.date, currentUser.locale, "LLL");
      });
    }

    function openLogOverlay(log) {

      const dialog = {
        view: "views/webhooks/overlays/details.html",
        title: 'Details',
        position: 'right',
        size: 'medium',
        log,
        currentUser: this.currentUser,
        close: () => {
          editorService.close();
        }
      };

      editorService.open(dialog);
    }

    function isChecked(log) {
      return log.statusCode === "OK (200)";
    }

    init();
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.WebhookLogController", WebhookLogController);

})();
