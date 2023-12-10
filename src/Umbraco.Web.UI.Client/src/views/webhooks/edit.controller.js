(function () {
    "use strict";

    function WebhooksEditController($scope, $q, $timeout, $location, $routeParams, editorService, eventsService, navigationService, notificationsService, localizationService, contentTypeResource, mediaTypeResource, memberTypeResource, webhooksResource, formHelper) {

        const vm = this;

        vm.isNew = false;
        vm.showIdentifier = true;

        vm.contentTypes = [];
        vm.webhook = {};
        vm.breadcrumbs = [];
        vm.labels = {};
        vm.save = save;
        vm.back = back;
        vm.goToPage = goToPage;

        vm.clearContentType = clearContentType;
        vm.clearEvent = clearEvent;
        vm.openContentTypePicker = openContentTypePicker;
        vm.openEventPicker = openEventPicker;
        vm.openCreateHeader = openCreateHeader;
        vm.removeHeader = removeHeader;

        function init() {
            vm.loading = true;

            let promises = [];

            // Localize labels
            promises.push(localizationService.localizeMany([
                "treeHeaders_webhooks",
                "webhooks_addWebhook",
                "defaultdialogs_confirmSure",
                "defaultdialogs_editWebhook"
            ]).then(function (values) {
                vm.labels.webhooks = values[0];
                vm.labels.addWebhook = values[1];
                vm.labels.areYouSure = values[2];
                vm.labels.editWebhook = values[3];

                if ($routeParams.create) {
                  vm.isNew = true;
                  vm.showIdentifier = false;
                  vm.webhook.name = vm.labels.addWebhook;
                  vm.webhook.enabled = true;
                }
            }));

            // Load events
            promises.push(loadEvents());

            if (!$routeParams.create) {

                promises.push(webhooksResource.getByKey($routeParams.id).then(webhook => {

                    vm.webhook = webhook;
                    vm.webhook.name = vm.labels.editWebhook;

                    const eventType = vm.webhook ? vm.webhook.events[0].eventType.toLowerCase() : null;
                    const contentTypes = webhook.contentTypeKeys.map(x => ({ key: x }));

                    getEntities(contentTypes, eventType);

                    makeBreadcrumbs();
                }));
            }

            $q.all(promises).then(() => {
                if ($routeParams.create) {
                    $scope.$emit("$changeTitle", vm.labels.addWebhook);
                } else {
                  $scope.$emit("$changeTitle", vm.labels.editWebhook + ": " + vm.webhook.key);
                }

                vm.loading = false;
            });

            // Activate tree node
            $timeout(function () {
                navigationService.syncTree({ tree: $routeParams.tree, path: [-1], activate: true });
            });
        }

        function openEventPicker() {

          const dialog = {
            selectedEvents: vm.webhook.events,
            submit(model) {
              vm.webhook.events = model.selection;
              editorService.close();
            },
            close() {
              editorService.close();
            }
          };

          localizationService.localize("defaultdialogs_selectEvent").then(value => {
            dialog.title = value;
            editorService.eventPicker(dialog);
          });
        }

        function openContentTypePicker() {
          const eventType = vm.webhook ? vm.webhook.events[0].eventType.toLowerCase() : null;

          const editor = {
            multiPicker: true,
            filterCssClass: "not-allowed not-published",
            filter: function (item) {
              // filter out folders (containers), element types (for content) and already selected items
              return item.nodeType === "container"; // || item.metaData.isElement || !!_.findWhere(vm.itemTypes, { udi: item.udi });
            },
            submit(model) {
              getEntities(model.selection, eventType);
              vm.webhook.contentTypeKeys = model.selection.map(item => item.key);
              editorService.close();
            },
            close() {
              editorService.close();
            }
          };

          switch (eventType.toLowerCase()) {
            case "content":
              editorService.contentTypePicker(editor);
              break;
            case "media":
              editorService.mediaTypePicker(editor);
              break;
            case "member":
              editorService.memberTypePicker(editor);
              break;
          }
        }

        function getEntities(selection, eventType) {
          let resource;
          switch (eventType.toCamelCase()) {
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
          vm.contentTypes = [];
          selection.forEach(entity => {
            resource.getById(entity.key)
              .then(data => {
                if (!vm.contentTypes.some(x => x.key === data.key)) {
                  vm.contentTypes.push(data);
                }
              }).catch(err => {
                let name;
                switch (eventType.toCamelCase()) {
                  case "content":
                    name = "Unknown content type";
                    break;
                  case "media":
                    name = "Unknown media type";
                    break;
                  case "member":
                    name = "Unknown member type";
                    break;
                  default:
                    name = "Unknown type";
                }

                let data = {
                  icon: "icon-alert",
                  name: name,
                  description: "An error occurred while loading the content type.",
                  key: entity.key
                }
                vm.contentTypes.push(data);
            });
          });
        }

        function loadEvents() {
          return webhooksResource.getAllEvents()
            .then(data => {
              vm.events = data;
            });
        }

        function clearContentType(contentTypeKey) {
          if (Utilities.isArray(vm.webhook.contentTypeKeys)) {
            vm.webhook.contentTypeKeys = vm.webhook.contentTypeKeys.filter(x => x !== contentTypeKey);
          }
          if (Utilities.isArray(vm.contentTypes)) {
            vm.contentTypes = vm.contentTypes.filter(x => x.key !== contentTypeKey);
          }
        }

        function clearEvent(event) {
          if (Utilities.isArray(vm.webhook.events)) {
            vm.webhook.events = vm.webhook.events.filter(x => x !== event);
          }

          if (Utilities.isArray(vm.contentTypes)) {
            vm.events = vm.events.filter(x => x.key !== event);
          }
        }

        function openCreateHeader() {

          const dialog = {
            view: "views/webhooks/overlays/header.html",
            size: 'small',
            position: 'right',
            submit(model) {
              if (!vm.webhook.headers) {
                vm.webhook.headers = {};
              }
              vm.webhook.headers[model.key] = model.value;
              editorService.close();
            },
            close() {
              editorService.close();
            }
          };

          localizationService.localize("webhooks_createHeader").then(value => {
            dialog.title = value;
            editorService.open(dialog);
          });
        }

        function removeHeader(key) {
          delete vm.webhook.headers[key];
        }

        function save() {

            if (!formHelper.submitForm({ scope: $scope })) {
              return;
            }

            saveWebhook();
        }

        function saveWebhook() {

          if (vm.isNew) {
            webhooksResource.create(vm.webhook)
              .then(webhook => {

                formHelper.resetForm({ scope: $scope });

                vm.webhook = webhook;
                vm.webhook.name = vm.labels.editWebhook;

                vm.saveButtonState = "success";

                $scope.$emit("$changeTitle", vm.labels.editWebhook + ": " + vm.webhook.key);

                localizationService.localize("speechBubbles_webhookSaved").then(value => {
                  notificationsService.success(value);
                });

                // Emit event when language is created or updated/saved
                eventsService.emit("editors.webhooks.webhookSaved", {
                  webhook: webhook,
                  isNew: vm.isNew
                });

                vm.isNew = false;
                vm.showIdentifier = true;

              }, x => {
                let errorMessage = undefined;
                if (x.data.ModelState) {
                  errorMessage = `Message: ${Object.values(x.data.ModelState).flat().join(' ')}`;
                }
                handleSubmissionError(x, `Error saving webhook. ${errorMessage ?? ''}`);
              });
          }
          else {
            webhooksResource.update(vm.webhook)
              .then(webhook => {

                formHelper.resetForm({ scope: $scope });

                vm.webhook = webhook;
                vm.webhook.name = vm.labels.editWebhook;

                vm.saveButtonState = "success";

                $scope.$emit("$changeTitle", vm.labels.editWebhook + ": " + vm.webhook.key);

                localizationService.localize("speechBubbles_webhookSaved").then(value => {
                  notificationsService.success(value);
                });

                // Emit event when language is created or updated/saved
                eventsService.emit("editors.webhooks.webhookSaved", {
                  webhook: webhook,
                  isNew: vm.isNew
                });

                vm.isNew = false;
                vm.showIdentifier = true;

              }, x => {
                let errorMessage = undefined;
                if (x.data.ModelState) {
                  errorMessage = `Message: ${Object.values(x.data.ModelState).flat().join(' ')}`;
                }
                handleSubmissionError(x, `Error saving webhook. ${errorMessage ?? ''}`);
              });
            }
        }

        function handleSubmissionError(err, errorMessage) {
          notificationsService.error(errorMessage);
          vm.saveButtonState = 'error';
          formHelper.resetForm({ scope: $scope, hasErrors: true });
          formHelper.handleError(err);
        }

        function back() {
            $location.path("settings/webhooks/overview");
        }

        function goToPage(ancestor) {
            $location.path(ancestor.path);
        }

        function makeBreadcrumbs() {
            vm.breadcrumbs = [
                {
                    "name": vm.labels.webhooks,
                    "path": "/settings/webhooks/overview"
                },
                {
                    "name": vm.webhook.name
                }
            ];
        }

        init();
    }

    angular.module("umbraco").controller("Umbraco.Editors.Webhooks.EditController", WebhooksEditController);
})();
