(function () {
  "use strict";
  function EditController($scope, editorService, contentTypeResource, mediaTypeResource) {
    var vm = this;
    vm.clearContentType = clearContentType;
    vm.clearEvent = clearEvent;
    this.openEventPicker = () => {
      editorService.eventPicker({
        title: "Select event",
        submit(model) {

          $scope.model.webhook.events =  model.selection;
          editorService.close();
        },
        close() {
          editorService.close();
        }
      });
    }
    this.openContentTypePicker = () => {
      const isContent = $scope.model.webhook ? $scope.model.webhook.events[0].toLowerCase().includes("content") : null;
      editorService.treePicker({
        section: 'settings',
        treeAlias: isContent ? 'documentTypes' : 'mediaTypes',
        entityType: isContent ? 'DocumentType' : 'MediaType',
        multiPicker: true,
        submit(model) {
          getEntities(model.selection, isContent);
          $scope.model.webhook.entityKeys =  model.selection.map((item) => item.key);
          editorService.close();
        },
        close() {
          editorService.close();
        }
      });
    };

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

    function clearContentType (contentTypeKey)
    {
      if (Array.isArray($scope.model.webhook.entityKeys)) {
        $scope.model.webhook.entityKeys = $scope.model.webhook.entityKeys.filter(x => x !== contentTypeKey);
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

    this.close = () => {
      if ($scope.model.close) {
        $scope.model.close();
      }
    };

    this.submit = () => {
      if ($scope.model.submit) {
        $scope.model.submit($scope.model);
      }
    };
  }

  angular.module("umbraco").controller("Umbraco.Editors.Webhooks.EditController", EditController);
})();
