(function () {
    "use strict";

    function ListViewDeleteController($scope, localizationService) {

        var vm = this;
        vm.loading = true;
        vm.disableDelete = false;

        function onInit() {
            $scope.model.hideSubmitButton = true;
        }

        vm.checkingReferencesComplete = () => {
            $scope.model.hideSubmitButton = false;
        };

        vm.onReferencesWarning = () => {
            $scope.model.submitButtonStyle = "danger";

            // check if the deletion of items that have references has been disabled
            if (Umbraco.Sys.ServerVariables.umbracoSettings.disableDeleteWhenReferenced) {
                // this will only be set to true if we have a warning, indicating that this item or its descendants have reference
                vm.disableDelete = true;
                $scope.model.disableSubmitButton = true;
            }

            localizationService.localize("general_delete").then(function (action) {
              localizationService.localize("references_listViewDialogWarning", [action.toLowerCase()]).then((value) => {
                    vm.warningText = value;
                });
            });
        };

        onInit();
    }

    angular.module("umbraco").controller("Umbraco.Overlays.ListViewDeleteController", ListViewDeleteController);

})();
