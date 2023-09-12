(function () {
  "use strict";

  function WebhookController($q, webhooksResource, notificationsService, editorService, overlayService, contentTypeResource) {
    var vm = this;

    vm.openWebhookOverlay = openWebhookOverlay;
    vm.deleteWebhook = deleteWebhook;
    vm.handleSubmissionError = handleSubmissionError;

    vm.page = {};
    vm.webhooks = [];
    vm.events = [];



    function loadEvents (){
      return webhooksResource.getAllEvents()
        .then((data) => {
          vm.events = data;
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
        contentType: webhook ? webhook.contentType : null,
        webhook: webhook ? {
          entityKey: webhook.contentType ? webhook.contentType.key : null,
          enabled: webhook.enabled,
          event: webhook.event,
          id: webhook.id,
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
          if (!model.webhook.event) {
            handleSubmissionError(model, 'Please provide the event for which the webhook should trigger');
            return;
          }
          if(isCreating){
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
