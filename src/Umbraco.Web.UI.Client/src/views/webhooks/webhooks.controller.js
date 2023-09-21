(function () {
  "use strict";

  function WebhookController($q,$scope, webhooksResource, notificationsService, editorService, overlayService, contentTypeResource, mediaTypeResource) {
    var vm = this;

    vm.openWebhookOverlay = openWebhookOverlay;
    vm.deleteWebhook = deleteWebhook;
    vm.handleSubmissionError = handleSubmissionError;
    vm.resolveTypeNames = resolveTypeNames;
    vm.resolveEventNames = resolveEventNames;

    vm.page = {};
    vm.webhooks = [];
    vm.events = [];
    vm.webHooksContentTypes = {};
    vm.webhookEvents = {};

    function loadEvents (){
      return webhooksResource.getAllEvents()
        .then((data) => {
          vm.events = data.map(item => item.eventName);
          console.log("logging vm.events")
          console.log(vm.events)
        });
    }

    function resolveEventNames(webhook) {
      webhook.events.forEach((event) => {
        if (!vm.webhookEvents[webhook.key]) {
          vm.webhookEvents[webhook.key] = event;
        } else {
          vm.webhookEvents[webhook.key] += ", " + event;
        }
      });
    }

    function getEntities(webhook) {
      const isContent = webhook.events[0].toLowerCase().includes("content");
      const resource = isContent ? contentTypeResource : mediaTypeResource;
      let entities = [];

      webhook.entityKeys.forEach((key) => {
        resource.getById(key)
          .then((data) => {
            entities.push(data);
          });
      });

      return entities;
    }

    function resolveTypeNames(webhook) {
      const isContent = webhook.events[0].toLowerCase().includes("content");
      const resource = isContent ? contentTypeResource : mediaTypeResource;

      if (vm.webHooksContentTypes[webhook.key]){
        delete vm.webHooksContentTypes[webhook.key];
      }

      webhook.entityKeys.forEach((key) => {
        resource.getById(key)
          .then((data) => {
            if (!vm.webHooksContentTypes[webhook.key]) {
              vm.webHooksContentTypes[webhook.key] = data.name;
            } else {
              vm.webHooksContentTypes[webhook.key] += ", " + data.name;
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
        webhook: webhook ? {
          entityKeys: webhook.entityKeys,
          enabled: webhook.enabled,
          events: webhook.events,
          key: webhook.key,
          url: webhook.url
        } : {enabled: true},
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
          if(isCreating){
            webhooksResource.create(model.webhook)
              .then(() => {
                console.log("Loading freaking webhooks")
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
          else{
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
      webhooksResource
        .getAll()
        .then((result) => {
          vm.webhooks = result;
          vm.webhookEvents = {};
          vm.webHooksContentTypes = {};

          vm.webhooks.forEach((webhook) => {
            resolveTypeNames(webhook);
            resolveEventNames(webhook);
          })
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
    }

    loadWebhooks()
    loadEvents()
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.WebhookController", WebhookController);

})();
