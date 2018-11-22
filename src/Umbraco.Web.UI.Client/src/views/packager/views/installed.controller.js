(function () {
    "use strict";

    function PackagesInstalledController($scope, $route, $location, packageResource, $timeout, $window, localStorageService, localizationService) {

        var vm = this;

        vm.confirmUninstall = confirmUninstall;
        vm.uninstallPackage = uninstallPackage;
        vm.state = "list";
        vm.installState = {
            status: ""
        };
        vm.package = {};

        function init() {
            packageResource.getInstalled()
                .then(function (packs) {
                    vm.installedPackages = packs;
                });
            vm.installState.status = "";
            vm.state = "list";
        }

        function confirmUninstall(pck) {
            vm.state = "packageDetails";
            vm.package = pck;
        }

        function uninstallPackage(installedPackage) {
            vm.installState.status = localizationService.localize("packager_installStateUninstalling");
            vm.installState.progress = "0";

            packageResource.uninstall(installedPackage.id)
                .then(function () {

                    if (installedPackage.files.length > 0) {
                        vm.installState.status = localizationService.localize("packager_installStateComplete");
                        vm.installState.progress = "100";

                        //set this flag so that on refresh it shows the installed packages list
                        localStorageService.set("packageInstallUri", "installed");
                        
                        //reload on next digest (after cookie)
                        $timeout(function () {
                            $window.location.reload(true);
                        });
                        
                    }
                    else {
                        init();
                    }
                });
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.InstalledController", PackagesInstalledController);

})();
