(function () {
    "use strict";

    function ListViewDeleteController($scope, localizationService) {

        var vm = this;
        vm.loading = true;

        function onInit() {
            $scope.model.hideSubmitButton = true;
        }

        vm.checkingReferencesComplete = () => {
            $scope.model.hideSubmitButton = false;
        };

        vm.onReferencesWarning = () => {
            $scope.model.submitButtonStyle = "danger";

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
