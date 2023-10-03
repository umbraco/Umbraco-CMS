(function () {
  "use strict";

  function WebhookController($q,$scope, webhooksResource, notificationsService, editorService, overlayService, contentTypeResource, mediaTypeResource) {
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
    }

    function isChecked (log) {
      return log.retryCount < 5;
    }

    loadLogs();
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.WebhookLogController", WebhookController);

})();
