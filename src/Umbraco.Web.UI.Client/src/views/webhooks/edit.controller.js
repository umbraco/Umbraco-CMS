(function () {
    "use strict";

    function WebhooksEditController($scope, $q, $timeout, $location, $routeParams, editorService, eventsService, navigationService, notificationsService, localizationService, contentTypeResource, mediaTypeResource, memberTypeResource, webhooksResource, formHelper) {

        const vm = this;

        vm.isNew = false;
        vm.showIdentifier = true;

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
                }
            }));

            // Load events
            promises.push(loadEvents());

            if (!$routeParams.create) {
                promises.push(webhooksResource.getByKey($routeParams.id).then(webhook => {

                    const obj = {};

                    // Convert webhook object to camelCase properties.
                    for (const [key, value] of Object.entries(webhook)) {
                      obj[key.toCamelCase()] = value;
                    }

                    vm.webhook = obj;

                    resolveEventNames(vm.webhook);

                    makeBreadcrumbs();
                }));
            }

            $q.all(promises).then(() => {
                if ($routeParams.create) {
                    $scope.$emit("$changeTitle", vm.labels.addWebhook);
                } else {
                  $scope.$emit("$changeTitle", vm.labels.editWebhook + ": " + vm.webhook.name);
                }

                vm.loading = false;
            });

            // Activate tree node
            $timeout(function () {
                navigationService.syncTree({ tree: $routeParams.tree, path: [-1], activate: true });
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

        function openEventPicker() {
          editorService.eventPicker({
            title: "Select event",
            selectedEvents: vm.webhook.events,
            submit(model) {
              vm.webhook.events = model.selection;
              editorService.close();
            },
            close() {
              editorService.close();
            }
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
          editorService.open({
            title: "Create header",
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
          });
        }

        function removeHeader(key) {
          delete vm.webhook.headers[key];
        }

        function save() {
            if (formHelper.submitForm({ scope: $scope })) {
                saveWebhook();
            }
        }

        function saveWebhook() {
            webhooksResource.save(vm.webhook).then(webhook => {
                formHelper.resetForm({ scope: $scope });

                vm.webhook = webhook;
                vm.saveButtonState = "success";

                $scope.$emit("$changeTitle", vm.labels.editWebhook + ": " + vm.webhook.name);

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
            }, function (err) {
                vm.saveButtonState = "error";
                formHelper.resetForm({ scope: $scope, hasErrors: true });
                formHelper.handleError(err);
            });
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
