(function () {
  "use strict";

  function WebhookLogController($q,$scope, webhooksResource, notificationsService, overlayService) {
    var vm = this;
    vm.logs = [];
    vm.openLogOverlay = openLogOverlay;
    vm.isChecked = isChecked;

    function loadLogs (){
      return webhooksResource.getLogs()
        .then((data) => {
          vm.logs = data.items;
        });
    }

    function openLogOverlay (log) {
      overlayService.open({
        view: "views/webhooks/overlays/details.html",
        title: 'Details',
        position: 'right',
        log,
        currentUser: this.currentUser,
        close: () => {
          overlayService.close();
        },
      });
    }

    function isChecked (log) {
      return log.retryCount < 5;
    }

    loadLogs();
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.WebhookLogController", WebhookLogController);

})();
