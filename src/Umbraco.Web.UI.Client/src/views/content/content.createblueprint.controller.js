(function () {

  function CreateBlueprintController(
    $scope,
    contentResource,
    notificationsService,
    navigationService,
    localizationService,
    formHelper,
    contentEditingHelper) {

    $scope.message = {
      name: $scope.currentNode.name
    };

    var successText = {};
    localizationService.localize("blueprints_createBlueprintFrom", ["<em>" + $scope.message.name + "</em>"]).then(function (localizedVal) {
      $scope.title = localizedVal;
    });

   

    $scope.cancel = function () {
      navigationService.hideMenu();
    };

    $scope.create = function () {
      if (formHelper.submitForm({
        scope: $scope,
        formCtrl: this.blueprintForm
      })) {

        contentResource.createBlueprintFromContent($scope.currentNode.id, $scope.message.name)
          .then(function(data) {

              formHelper.resetForm({ scope: $scope });

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
