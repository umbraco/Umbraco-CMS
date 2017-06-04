(function () {

  function CreateBlueprintController(
    $scope,
    contentResource,
    notificationsService,
    navigationService,
    localizationService,
    formHelper) {

    $scope.name = $scope.currentNode.name;

    localizationService.localize("content_createBlueprintFrom").then(function (value) {
      $scope.label = value + " " + $scope.name;
    });


    $scope.create = function () {

      if (formHelper.submitForm({ scope: $scope, formCtrl: this.blueprintForm, statusMessage: "Creating blueprint..."})) {

        contentResource.createBlueprintFromContent($scope.currentNode.id, $scope.name)
          .then(function () {
            notificationsService.showNotification({
              type: 3,
              header: "Created blueprint",
              message: "Blueprint was created based on " + $scope.currentNode.name
            });
            navigationService.hideMenu();
          });

      }

    };
  }


  angular.module("umbraco").controller("Umbraco.Editors.Content.CreateBlueprintController", CreateBlueprintController);

}());