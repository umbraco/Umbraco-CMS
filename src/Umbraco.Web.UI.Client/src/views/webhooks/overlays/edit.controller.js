(function () {
  "use strict";
  function EditController($scope, editorService) {
    this.openContentTypePicker = () => {
      const isContent = $scope.model.webhook ? $scope.model.webhook.event.split('.')[0] === 'content' : null;
      editorService.treePicker({
        section: 'settings',
        treeAlias: isContent ? 'documentTypes' : 'mediaTypes',
        entityType: isContent ? 'DocumentType' : 'MediaType',
        multiPicker: false,
        submit(model) {
          // sets $scope.model.contentType to model.selection[0]
          [$scope.model.contentType] = model.selection;
          $scope.model.webhook.contentType = $scope.model.contentType.id;
          editorService.close();
        },
        close() {
          editorService.close();
        }
      });
    };

    this.clearContentType = () => {
      delete $scope.model.webhook.contentType;
      delete $scope.model.contentType;
    };

    this.eventChanged = (newValue, oldValue) => {
      if (oldValue && newValue) {
        if (oldValue.split('.')[0] !== newValue.split('.')[0]) {
          this.clearContentType();
        }
      }
      if (!newValue) {
        this.clearContentType();
      }
    };

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
});
