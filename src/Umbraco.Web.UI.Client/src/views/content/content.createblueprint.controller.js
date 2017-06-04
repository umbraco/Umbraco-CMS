(function() {

  function CreateBlueprintController(
    $scope,
    contentResource,
    notificationsService,
    navigationService,
    localizationService,
    formHelper) {

    var successText = {};
    localizationService.localizeMany([
      "content_createBlueprintFrom",
      "content_createdBlueprintHeading",
      "content_createdBlueprintMessage"
    ]).then(function(localizedValues) {
      $scope.label = localizedValues[0] + " " + $scope.name;
      successText.heading = localizedValues[1];
      successText.message = localizedValues[2].replace("%0%", $scope.name);
    });

    $scope.name = $scope.currentNode.name;

    $scope.cancel = function() {
      navigationService.hideMenu();
    };

    $scope.create = function() {
      if (formHelper.submitForm({
        scope: $scope,
        formCtrl: this.blueprintForm,
        statusMessage: "Creating blueprint..."
      })) {

        contentResource.createBlueprintFromContent($scope.currentNode.id, $scope.name)
          .then(function() {
              notificationsService.showNotification({
                type: 3,
                header: successText.heading,
                message: successText.message
              });
              navigationService.hideMenu();
            },
            function(response) {
              for (var n = 0; n < response.data.notifications.length; n++) {
                notificationsService.showNotification({
                  type: 2,
                  header: response.data.notifications[n].header,
                  message: response.data.notifications[n].message
                });
              }
            }
          );
      }
    };
  }

  angular.module("umbraco").controller("Umbraco.Editors.Content.CreateBlueprintController", CreateBlueprintController);

}());