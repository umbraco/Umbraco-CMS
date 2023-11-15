(function () {
  "use strict";

  function WebhookController($q, $timeout, $routeParams, webhooksResource, navigationService, notificationsService, editorService, overlayService, contentTypeResource, mediaTypeResource, memberTypeResource) {

    const vm = this;

    vm.openWebhookOverlay = openWebhookOverlay;
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

    function getEntities(webhook) {
      let resource = determineResource(webhook.events[0].eventType.toLowerCase());
      let entities = [];

      webhook.contentTypeKeys.forEach(key => {
        resource.getById(key)
          .then(data => {
            entities.push(data);
          });
      });

      return entities;
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

    function handleSubmissionError (model, errorMessage) {
      notificationsService.error(errorMessage);
      model.disableSubmitButton = false;
      model.submitButtonState = 'error';
    }

    function openWebhookOverlay (webhook) {
      let isCreating = !webhook;
      editorService.open({
        title: webhook ? 'Edit webhook' : 'Add webhook',
        position: 'right',
        size: 'small',
        submitButtonLabel: webhook ? 'Save' : 'Create',
        view: "views/webhooks/overlays/edit.html",
        events: vm.events,
        contentTypes : webhook ? getEntities(webhook) : null,
        webhook: webhook ? webhook : {enabled: true},
        submit: (model) => {
          model.disableSubmitButton = true;
          model.submitButtonState = 'busy';
          if (!model.webhook.url) {
            //Due to validation url will only be populated if it's valid, hence we can make do with checking url is there
            handleSubmissionError(model, 'Please provide a valid URL. Did you include https:// ?');
            return;
          }
          if (!model.webhook.events || model.webhook.events.length === 0) {
            handleSubmissionError(model, 'Please provide the event for which the webhook should trigger');
            return;
          }

          if (isCreating) {
            webhooksResource.create(model.webhook)
              .then(() => {
                loadWebhooks()
                notificationsService.success('Webhook saved.');
                editorService.close();
              }, x => {
                let errorMessage = undefined;
                if (x.data.ModelState) {
                  errorMessage = `Message: ${Object.values(x.data.ModelState).flat().join(' ')}`;
                }
                handleSubmissionError(model, `Error saving webhook. ${errorMessage ?? ''}`);
              });
          }
          else {
            webhooksResource.update(model.webhook)
              .then(() => {
                loadWebhooks()
                notificationsService.success('Webhook saved.');
                editorService.close();
              }, x => {
                let errorMessage = undefined;
                if (x.data.ModelState) {
                  errorMessage = `Message: ${Object.values(x.data.ModelState).flat().join(' ')}`;
                }
                handleSubmissionError(model, `Error saving webhook. ${errorMessage ?? ''}`);
              });
          }

        },
        close: () => {
          editorService.close();
        }
      });
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

    function deleteWebhook (webhook, event) {
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
