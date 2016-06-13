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
        }

        function confirmUninstall(pck) {
            vm.state = "packageDetails";
            vm.package = pck;
        }

        function uninstallPackage(installedPackage) {
            vm.installState.status = "Uninstalling package...";
            packageResource.uninstall(installedPackage.id)
                .then(function() {
                    var url = window.location.href + "?uninstalled=" + vm.package.packageGuid;
                    window.location.reload(true);
                });
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.InstalledController", PackagesInstalledController);

})();
