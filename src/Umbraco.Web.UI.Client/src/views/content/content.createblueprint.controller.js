(function() {

    function CreateBlueprintController(
      $scope,
      contentResource,
      notificationService,
      navigationService
    ) {

    $scope.name = $scope.currentNode.name;

    $scope.create = function() {
      contentResource.createBlueprintFromContent($scope.currentNode.id, $scope.name)
        .then(function() {
          notificationService.showNotification({
            type: 3,
            header: "Created blueprint",
            message: "Blueprint was created based on " + $scope.name
          });
          navigationService.hideMenu();
        });
    };
  }

  angular.module("umbraco").controller("Umbraco.Editors.Content.CreateBlueprintController", CreateBlueprintController);

}());