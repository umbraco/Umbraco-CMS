(function () {
    "use strict";

    function PackagesInstalledController($scope, $route, $location, packageResource) {

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
            vm.installState.status = "Uninstalling package...";
            packageResource.uninstall(installedPackage.id)
                .then(function () {
                    if (installedPackage.files.length > 0) {
                        vm.installState.status = "All done, your browser will now refresh";

                        var url = window.location.href + "?uninstalled=" + vm.package.packageGuid;
                        window.location.reload(true);
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
