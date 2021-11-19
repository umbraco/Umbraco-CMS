(function () {

    function CreateBlueprintController(
        $scope,
        contentResource,
        navigationService,
        localizationService,
        formHelper,
        contentEditingHelper) {

        $scope.message = {
            name: $scope.currentNode.name
        };

        localizationService.localize("blueprints_createBlueprintFrom", [$scope.message.name]).then(localizedVal => {
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
                    .then(function (data) {

                        formHelper.resetForm({ scope: $scope });

                        navigationService.hideMenu();
                    },
                    function (err) {
                        formHelper.resetForm({ scope: $scope, hasErrors: true });
                        contentEditingHelper.handleSaveError({
                            err: err
                        });

                    });
            }
        };
    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.CreateBlueprintController", CreateBlueprintController);

}());
