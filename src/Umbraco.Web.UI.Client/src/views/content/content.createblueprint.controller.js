(function() {

  function CreateBlueprintController($scope,
    contentResource,
    appState,
    navigationService,
    editorState,
    notificationService) {

    $scope.name = editorState.current.name;

    $scope.create = function() {
      contentResource.createBlueprintFromContent(editorState.current.id, $scope.name)
        .then(function() {
          notificationService.showNotification({
            type: 3,
            header: "Created blueprint",
            message: "Blueprint was created based on " + $scope.name
          });
          $scope.closeDialogs();
        });
    };
  }

  angular.module("umbraco").controller("Umbraco.Editors.Content.CreateBlueprintController",
    [
      "$scope",
      "contentResource",
      "navigationService",
      "editorState",
      "notificationsService",
      CreateBlueprintController
    ]);

}());