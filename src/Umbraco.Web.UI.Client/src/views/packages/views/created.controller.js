(function () {
    "use strict";

    function CreatedController($timeout, $location, packageResource, localizationService, overlayService) {

        const vm = this;

        vm.deleteCreatedPackage = deleteCreatedPackage;
        vm.goToPackage = goToPackage;
        vm.createPackage = createPackage;

        function onInit() {

            vm.createdPackages = [];

            packageResource.getAllCreated().then(createdPackages => {
                vm.createdPackages = createdPackages;
            }, angular.noop);

        }

        function deleteCreatedPackage(createdPackage) {

            const dialog = {
                submitButtonLabelKey: "contentTypeEditor_yesDelete",
                submit: function (model) {
                    performDelete(createdPackage);
                    overlayService.close();
                },
                close: function () {
                    overlayService.close();
                }
            };
    
            localizationService.localize("general_delete").then(value => {
                dialog.title = value;
                overlayService.open(dialog);
            });

        }

        function performDelete(createdPackage) {
            console.log("perform delete");
        }

        function goToPackage(createdPackage) {
            $location.path("packages/packages/edit/" + createdPackage.id);
        }

        function createPackage() {
            $location.search('create', null);
            $location.path("packages/packages/edit/-1").search("create", "true");
        }

        onInit();
        
    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.CreatedController", CreatedController);

})();
