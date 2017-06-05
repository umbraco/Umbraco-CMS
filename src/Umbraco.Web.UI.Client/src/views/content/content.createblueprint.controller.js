(function () {

  function CreateBlueprintController(
    $scope,
    contentResource,
    notificationsService,
    navigationService,
    localizationService,
    formHelper,
    contentEditingHelper) {

    var successText = {};
    localizationService.localizeMany([
      "content_createBlueprintFrom"
    ]).then(function (localizedValues) {
      $scope.label = localizedValues[0] + " " + $scope.message.name;
    });

    $scope.message = {
      name : $scope.currentNode.name
    };

    $scope.cancel = function () {
      navigationService.hideMenu();
    };

    $scope.create = function () {
      if (formHelper.submitForm({
        scope: $scope,
        formCtrl: this.blueprintForm,
        statusMessage: "Creating blueprint..."
      })) {

        contentResource.createBlueprintFromContent($scope.currentNode.id, $scope.message.name)
          .then(function(data) {

              formHelper.resetForm({ scope: $scope, notifications: data.notifications });

              navigationService.hideMenu();
            },
            function(err) {

              contentEditingHelper.handleSaveError({
                redirectOnFailure: false,
                err: err
              });

            }
          );
      }
    };
  }

  angular.module("umbraco").controller("Umbraco.Editors.Content.CreateBlueprintController", CreateBlueprintController);

}());