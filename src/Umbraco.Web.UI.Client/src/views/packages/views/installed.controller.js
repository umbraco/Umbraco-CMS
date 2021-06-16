(function () {
    "use strict";

    function PackagesInstalledController($location, packageResource, localizationService) {

        var vm = this;

        vm.packageOptions = packageOptions;
        vm.state = "list";
        vm.installState = {
            status: ""
        };
        vm.package = {};

        var labels = {};

        function init() {
            packageResource.getInstalled()
                .then(function (packs) {
                    vm.installedPackages = packs;
                });
            vm.installState.status = "";
            vm.state = "list";

            var labelKeys = [
                "packager_installStateUninstalling",
                "packager_installStateComplete"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                labels.installStateUninstalling = values[0];
                labels.installStateComplete = values[1];
            });
        }

        function packageOptions(pck) {
            $location.path("packages/packages/options/" + pck.id)
                .search("packageId", null); //ensure the installId flag is gone, it's only available on first install
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.InstalledController", PackagesInstalledController);

})();
