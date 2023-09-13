(function () {
  "use strict";
  function EditController($scope, editorService) {
    this.openContentTypePicker = () => {
      const isContent = $scope.model.webhook ? $scope.model.webhook.event.toLowerCase().includes("content") : null;
      editorService.treePicker({
        section: 'settings',
        treeAlias: isContent ? 'documentTypes' : 'mediaTypes',
        entityType: isContent ? 'DocumentType' : 'MediaType',
        multiPicker: true,
        submit(model) {
          $scope.model.contentTypes = model.selection;
          $scope.model.webhook.entityKeys =  model.selection.map((item) => item.key);
          editorService.close();
        },
        close() {
          editorService.close();
        }
      });
    };

    this.clearContentType = (contentTypeKey) => {
      $scope.model.webhook.entityKeys = $scope.model.webhook.entityKeys.filter(x => x !== contentTypeKey)
      $scope.model.contentTypes = $scope.model.contentTypes.filter(x => x.key !== contentTypeKey);

    };

    this.eventChanged = (newValue, oldValue) => {
      if (oldValue && newValue) {
        if (oldValue.split !== newValue) {
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
})();
