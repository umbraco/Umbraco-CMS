(function () {
  "use strict";

  function WebhookController($q, $timeout, $location, $routeParams, webhooksResource, navigationService, notificationsService, overlayService, contentTypeResource, mediaTypeResource, memberTypeResource) {

    const vm = this;

    vm.addWebhook = addWebhook;
    vm.editWebhook = editWebhook;
    vm.deleteWebhook = deleteWebhook;
    vm.handleSubmissionError = handleSubmissionError;
    vm.resolveTypeNames = resolveTypeNames;
    vm.resolveEventNames = resolveEventNames;

    vm.page = {};
    vm.webhooks = [];
    vm.events = [];
    vm.webhooksContentTypes = {};
    vm.webhookEvents = {};

    function init() {
      vm.loading = true;

      let promises = [];

      promises.push(loadEvents());
      promises.push(loadWebhooks());

      $q.all(promises).then(function () {
        vm.loading = false;
      });

      // Activate tree node
      $timeout(function () {
        navigationService.syncTree({ tree: $routeParams.tree, path: [-1], activate: true });
      });
    }

    function loadEvents() {
      return webhooksResource.getAllEvents()
        .then(data => {
          vm.events = data;
        });
    }

    function resolveEventNames(webhook) {
      webhook.events.forEach(event => {
        if (!vm.webhookEvents[webhook.key]) {
          vm.webhookEvents[webhook.key] = event.eventName;
        } else {
          vm.webhookEvents[webhook.key] += ", " + event.eventName;
        }
      });
    }

    function determineResource(resourceType){
      let resource;
      switch (resourceType) {
        case "content":
          resource = contentTypeResource;
          break;
        case "media":
          resource = mediaTypeResource;
          break;
        case "member":
          resource = memberTypeResource;
          break;
        default:
          return;
      }

      return resource;
    }

    function resolveTypeNames(webhook) {
      let resource = determineResource(webhook.events[0].eventType.toLowerCase());

      if (vm.webhooksContentTypes[webhook.key]){
        delete vm.webhooksContentTypes[webhook.key];
      }

      webhook.contentTypeKeys.forEach(key => {
        resource.getById(key)
          .then(data => {
            if (!vm.webhooksContentTypes[webhook.key]) {
              vm.webhooksContentTypes[webhook.key] = data.name;
            } else {
              vm.webhooksContentTypes[webhook.key] += ", " + data.name;
            }
          });
      });
    }

    function handleSubmissionError(model, errorMessage) {
      notificationsService.error(errorMessage);
      model.disableSubmitButton = false;
      model.submitButtonState = 'error';
    }

    function addWebhook() {
      $location.search('create', null);
      $location.path("/settings/webhooks/edit/-1").search("create", "true");
    }

    function editWebhook(webhook) {
      $location.search('create', null);
      $location.path("/settings/webhooks/edit/" + webhook.key);
    }

    function loadWebhooks(){
      return webhooksResource
        .getAll()
        .then(result => {
          vm.webhooks = result;
          vm.webhookEvents = {};
          vm.webhooksContentTypes = {};

          vm.webhooks.forEach(webhook => {
            resolveTypeNames(webhook);
            resolveEventNames(webhook);
          })
        });
    }

    function deleteWebhook(webhook, event) {
      overlayService.open({
        title: 'Confirm delete webhook',
        content: 'Are you sure you want to delete the webhook?',
        submitButtonLabel: 'Yes, delete',
        submitButtonStyle: 'danger',
        closeButtonLabel: 'Cancel',
        submit: () => {
          webhooksResource.delete(webhook.key)
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
        }
      });

      event.preventDefault();
      event.stopPropagation();
    }

    init();
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.WebhookController", WebhookController);

})();
