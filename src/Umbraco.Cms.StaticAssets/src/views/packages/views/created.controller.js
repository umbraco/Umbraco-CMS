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
            }, Utilities.noop);

        }

        function deleteCreatedPackage(event, index, createdPackage) {

            event.stopPropagation();
            event.preventDefault();

            const dialog = {
                view: "views/packages/overlays/delete.html",
                package: createdPackage,
                submitButtonLabelKey: "contentTypeEditor_yesDelete",
                submitButtonStyle:"danger",
                submit: function (model) {
                    performDelete(index, createdPackage);
                    overlayService.close();
                },
                close: function () {
                    overlayService.close();
                }
            };

            const keys = [
                "general_delete",
                "defaultdialogs_confirmdelete"
            ];
    
            localizationService.localizeMany(keys).then(values => {
                dialog.title = values[0];
                dialog.content = values[1];
                overlayService.open(dialog);
            });

        }

        function performDelete(index, createdPackage) {        
            createdPackage.deleteButtonState = "busy";

            packageResource.deleteCreatedPackage(createdPackage.id).then(function () {
                vm.createdPackages.splice(index, 1);
            }, function (err) {
                createdPackage.deleteButtonState = "error";
            });
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
