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

        function deleteCreatedPackage(event, index, createdPackage) {

            event.stopPropagation();
            event.preventDefault();

            const dialog = {
                submitButtonLabelKey: "contentTypeEditor_yesDelete",
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
            packageResource.deleteCreatedPackage(createdPackage.id).then(()=> {
                vm.createdPackages.splice(index, 1);
            }, angular.noop);
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
