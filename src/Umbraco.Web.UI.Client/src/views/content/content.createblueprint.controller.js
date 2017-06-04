(function() {

  function CreateBlueprintController(
    $scope,
    contentResource,
    notificationsService,
    navigationService
  ) {

    $scope.name = $scope.currentNode.name;

    $scope.cancel = function() {
      navigationService.hideMenu();
    };

    $scope.create = function () {
      contentResource.createBlueprintFromContent($scope.currentNode.id, $scope.name)
        .then(function() {
          notificationsService.showNotification({
            type: 3,
            header: "Created blueprint",
            message: "Blueprint was created based on " + $scope.currentNode.name
          });
          navigationService.hideMenu();
        });
    };
  }


  angular.module("umbraco").controller("Umbraco.Editors.Content.CreateBlueprintController", CreateBlueprintController);

}());