(function () {
    "use strict";

    function PackagesInstalledController($scope, $route, $location, packageResource) {

        var vm = this;

        vm.uninstallPackage = uninstallPackage;

        function init() {
            packageResource.getInstalled()
                .then(function (packs) {
                    vm.installedPackages = packs;
                });
        }

        function uninstallPackage(installedPackage) {
            packageResource.uninstall(installedPackage.id)
                .then(function() {
                    init();
                });
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.InstalledController", PackagesInstalledController);

})();
