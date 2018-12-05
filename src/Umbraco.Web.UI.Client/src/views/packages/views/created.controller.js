(function () {
    "use strict";

    function CreatedController($timeout, $location, localizationService, overlayService) {

        const vm = this;

        vm.deleteCreatedPackage = deleteCreatedPackage;
        vm.goToPackage = goToPackage;
        vm.createPackage = createPackage;

        function onInit() {

            vm.createdPackages = [];

            //load created packages
            $timeout(function(){
                vm.createdPackages = [
                    {
                        "author": "Test",
                        "files": [],
                        "iconUrl": "",
                        "id": 1,
                        "license": "MIT License",
                        "licenseUrl": "http://opensource.org/licenses/MIT",
                        "name": "Test v8",
                        "url": "https://test.com",
                        "version": "0.0.0"
                    },
                    {
                        "author": "Test",
                        "files": [],
                        "iconUrl": "",
                        "id": 2,
                        "license": "MIT License",
                        "licenseUrl": "http://opensource.org/licenses/MIT",
                        "name": "Another Test v8",
                        "url": "https://test.com",
                        "version": "0.0.0"
                    }
                ];
            }, 1000);

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
            console.log("create package");
        }

        onInit();
        
    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.CreatedController", CreatedController);

})();
