(function () {
  "use strict";

  function OverviewController($q, webhooksResource, notificationsService, editorService, overlayService, localizationService, $scope) {
    var vm = this;
    vm.loading = true;

    vm.openWebhookOverlay = openWebhookOverlay;
    vm.deleteWebhook = deleteWebhook;

    vm.pagination = {
      pageNumber: 1,
      pageSize: 25,
    };

    vm.page = {};
    vm.webhooks = [];
    vm.events = [];
    var promises = [];

    // Localize labels
    promises.push(localizationService.localize("treeHeaders_webhooks").then(function (value) {
      vm.page.name = value;
      $scope.$emit("$changeTitle", value);
    }));

    console.log(this.webhooks);
    const loadEvents = () => webhooksResource.getAllEvents()
      .then((data) => {
        this.events = data;
      });

    // const handleSubmissionError = (model, errorMessage) => {
    //   notificationsService.error(errorMessage);
    //   model.disableSubmitButton = false;
    //   model.submitButtonState = 'error';
    // }

    function openWebhookOverlay (webhook) {
      editorService.open({
        title: webhook ? 'Edit webhook' : 'Add webhook',
        position: 'right',
        size: 'small',
        submitButtonLabel: webhook ? 'Save' : 'Create',
        view: EditOverlay,
        events: this.events,
        contentType: webhook ? webhook.contentType : null,
        webhook: webhook ? {
          contentType: webhook.contentType ? webhook.contentType.id : null,
          enabled: webhook.enabled,
          event: webhook.event.id,
          id: webhook.id,
          url: webhook.url,
        } : {enabled: true},
        submit: (model) => {
          model.disableSubmitButton = true;
          model.submitButtonState = 'busy';
          if (!model.webhook.url) {
            //Due to validation url will only be populated if it's valid, hence we can make do with checking url is there
            handleSubmissionError(model, 'Please provide a valid URL. Did you include https:// ?');
            return;
          }
          if (!model.webhook.event) {
            handleSubmissionError(model, 'Please provide the event for which the webhook should trigger');
            return;
          }
          headlessWebhooksResource.save(model.webhook)
            .then(() => {
              this.goToPage(1);

              notificationsService.success('Webhook saved.');
              editorService.close();
            }, x => {
              let errorMessage = undefined;
              if (x.data.ModelState) {
                errorMessage = `Message: ${Object.values(x.data.ModelState).flat().join(' ')}`;
              }
              handleSubmissionError(model, `Error saving webhook. ${errorMessage ?? ''}`);
            });
        },
        close: () => {
          editorService.close();
        },
      });
    }

    function loadWebhooks(){
      return webhooksResource
        .getAll(vm.pagination.pageNumber, vm.pagination.pageSize)
        .then((result) => {
          vm.webhooks = result;

          vm.pagination.pageNumber = result.pageNumber;
          vm.pagination.totalItems = result.totalItems;
          vm.pagination.totalPages = result.totalPages;
        });
    }

    function deleteWebhook (webhook) {
      overlayService.open({
        title: 'Confirm delete webhook',
        content: 'Are you sure you want to delete the webhook?',
        submitButtonLabel: 'Yes, delete',
        submitButtonStyle: 'danger',
        closeButtonLabel: 'Cancel',
        submit: () => {
          webhooksResource.deleteById(webhook.id)
            .then(() => {
              const index = this.webhooks.indexOf(webhook);
              this.webhooks.splice(index, 1);

              notificationsService.success('Webhook deleted.');
              overlayService.close();
            }, () => {
              notificationsService.error('Error deleting webhook.');
            });
        },
        close: () => {
          overlayService.close();
        },
      });
    };

    this.previousPage = () => this.goToPage(this.pagination.pageNumber - 1);
    this.nextPage = () => this.goToPage(this.pagination.pageNumber + 1);

    this.goToPage = (pageNumber) => {
      this.pagination.pageNumber = pageNumber;
      this.loading = true;
      loadWebhooks().then(() => {
        this.loading = false;
      });
    };

    $q.all([
      promises,
      loadWebhooks(),
      loadEvents(),
    ]).then(() => {
      vm.loading = false;
    });
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.OverviewController", OverviewController);

})();
