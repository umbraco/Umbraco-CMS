(function () {
  "use strict";

  function WebhookLogController($q, webhooksResource, editorService, userService, dateHelper) {

    const vm = this;

    vm.pagination = {
      pageNumber: 1,
      pageSize: 25
    };

    vm.logs = [];
    vm.openLogOverlay = openLogOverlay;

    function init() {
      vm.loading = true;

      let promises = [];

      promises.push(loadLogs());

      $q.all(promises).then(function () {
        vm.loading = false;
      });
    }

    function loadLogs() {
      const take = vm.pagination.pageSize;
      const skip = (vm.pagination.pageNumber - 1) * take;

      return webhooksResource.getLogs(skip, take)
        .then(data => {
          vm.logs = data.items;
          vm.pagination.totalPages = Math.ceil(data.totalItems/vm.pagination.pageSize);

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

    vm.previousPage = () => vm.goToPage(vm.pagination.pageNumber - 1);
    vm.nextPage = () => vm.goToPage(vm.pagination.pageNumber + 1);

    vm.goToPage = (pageNumber) => {
      vm.pagination.pageNumber = pageNumber;
      vm.loading = true;
      loadLogs().then(() => {
        vm.loading = false;
      });
    };

    init();
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.WebhookLogController", WebhookLogController);

})();
