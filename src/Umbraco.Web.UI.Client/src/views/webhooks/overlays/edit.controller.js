(function () {
  "use strict";

  function EditController($scope, editorService, contentTypeResource, mediaTypeResource) {
    var vm = this;
    vm.clearContentType = clearContentType;
    vm.clearEvent = clearEvent;
    vm.removeHeader = removeHeader;
    vm.openCreateHeader = openCreateHeader;
    vm.openEventPicker = openEventPicker;
    vm.openContentTypePicker = openContentTypePicker;
    vm.close = close;
    vm.submit = submit;


    function openEventPicker()
    {
      editorService.eventPicker({
        title: "Select event",
        selectedEvents: $scope.model.webhook.events,
        submit(model) {
          $scope.model.webhook.events = model.selection;
          editorService.close();
        },
        close() {
          editorService.close();
        }
      });
    }

    function openContentTypePicker()
    {
      const isContent = $scope.model.webhook ? $scope.model.webhook.events[0].toLowerCase().includes("content") : null;
      editorService.treePicker({
        section: 'settings',
        treeAlias: isContent ? 'documentTypes' : 'mediaTypes',
        entityType: isContent ? 'DocumentType' : 'MediaType',
        multiPicker: true,
        submit(model) {
          getEntities(model.selection, isContent);
          $scope.model.webhook.contentTypeKeys = model.selection.map((item) => item.key);
          editorService.close();
        },
        close() {
          editorService.close();
        }
      });
    }

    function openCreateHeader() {
      editorService.open({
        title: "Create header",
        view: "views/webhooks/overlays/header.html",
        size: 'small',
        position: 'right',
        submit(model) {
          if (!$scope.model.webhook.headers) {
            $scope.model.webhook.headers = {};
          }
          $scope.model.webhook.headers[model.key] = model.value;
          editorService.close();
        },
        close() {
          editorService.close();
        }
      });
    }

    function getEntities(selection, isContent) {
      const resource = isContent ? contentTypeResource : mediaTypeResource;
      $scope.model.contentTypes = [];

      selection.forEach((entity) => {
        resource.getById(entity.key)
          .then((data) => {
            $scope.model.contentTypes.push(data);
          });
      });
    }

    function clearContentType(contentTypeKey) {
      if (Array.isArray($scope.model.webhook.contentTypeKeys)) {
        $scope.model.webhook.contentTypeKeys = $scope.model.webhook.contentTypeKeys.filter(x => x !== contentTypeKey);
      }
      if (Array.isArray($scope.model.contentTypes)) {
        $scope.model.contentTypes = $scope.model.contentTypes.filter(x => x.key !== contentTypeKey);
      }
    }

    function clearEvent(event) {
      if (Array.isArray($scope.model.webhook.events)) {
        $scope.model.webhook.events = $scope.model.webhook.events.filter(x => x !== event);
      }

      if (Array.isArray($scope.model.contentTypes)) {
        $scope.model.events = $scope.model.events.filter(x => x.key !== event);
      }
    }

    function removeHeader(key) {
      delete $scope.model.webhook.headers[key];
    }


    function close()
    {
      if ($scope.model.close) {
        $scope.model.close();
      }
    }

    function submit()
    {
      if ($scope.model.submit) {
        $scope.model.submit($scope.model);
      }
    }
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.EditController", EditController);
})();
