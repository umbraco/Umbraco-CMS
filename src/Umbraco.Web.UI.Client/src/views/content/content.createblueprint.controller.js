(function() {

  function CreateBlueprintController(
    $scope,
    contentResource,
    notificationsService,
    navigationService,
    localizationService
  ) {

    var successText = {}, errorText = {};
    localizationService.localizeMany([
      "content_createdBlueprintHeading",
      "content_createdBlueprintMessage",
      "content_failedBlueprintMessage",
      "content_failedBlueprintMessage"
    ]).then(function(localizedValues) {
      successText.heading = localizedValues[0];
      successText.message = localizedValues[1].replace("%0%", $scope.name);
      errorText.heading = localizedValues[2];
      errorText.message = localizedValues[3];
    });

    $scope.name = $scope.currentNode.name;

    $scope.cancel = function() {
      navigationService.hideMenu();
    };

    $scope.create = function () {
      contentResource.createBlueprintFromContent($scope.currentNode.id, $scope.name)
      .then(
        function () { notificationsService.showNotification({ type: 3, header: successText.heading, message: successText.message }) },
        function () { notificationsService.showNotification({ type: 2, header: errorText.heading, message: errorText.message }) }
      );
      navigationService.hideMenu();
    }
  }


  angular.module("umbraco").controller("Umbraco.Editors.Content.CreateBlueprintController", CreateBlueprintController);

}());