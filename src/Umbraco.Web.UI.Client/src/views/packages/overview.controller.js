(function () {
    "use strict";

    function PackagesOverviewController($scope, $location, $routeParams, localizationService, localStorageService) {

        //Hack!
        // if there is a local storage value for packageInstallData then we need to redirect there,
        // the issue is that we still have webforms and we cannot go to a hash location and then window.reload
        // because it will double load it.
        // we will refresh and then navigate there.

        let packageInstallData = localStorageService.get("packageInstallData");
        let packageUri = $routeParams.method;

        if (packageInstallData) {
            localStorageService.remove("packageInstallData");

            if (packageInstallData.postInstallationPath) {
                //navigate to the custom installer screen if set
                $location.path(packageInstallData.postInstallationPath).search("packageId", packageInstallData.id);
                return;
            }

            //if it is "installed" then set the uri/path to that
            if (packageInstallData === "installed") {
                packageUri = "installed";
            }
        }

        var vm = this;
        vm.page = {};
        vm.page.labels = {};
        vm.page.name = "";
        vm.page.navigation = [];

        onInit();

        function onInit() {

            loadNavigation();

            setPageName();
        }

        function loadNavigation() {

            var labels = ["sections_packages", "packager_installed", "packager_installLocal", "packager_created"];

            localizationService.localizeMany(labels).then(function (data) {
                vm.page.labels.packages = data[0];
                vm.page.labels.installed = data[1];
                vm.page.labels.install = data[2];
                vm.page.labels.created = data[3];

                vm.page.navigation = [
                    {
                        "name": vm.page.labels.packages,
                        "icon": "icon-cloud",
                        "view": "views/packages/views/repo.html",
                        "active": !packageUri || packageUri === "repo",
                        "alias": "umbPackages",
                        "action": function () {
                            $location.path("/packages/packages/repo");
                        }
                    },
                    {
                        "name": vm.page.labels.installed,
                        "icon": "icon-box",
                        "view": "views/packages/views/installed.html",
                        "active": packageUri === "installed",
                        "alias": "umbInstalled",
                        "action": function () {
                            $location.path("/packages/packages/installed");
                        }
                    },
                    {
                        "name": vm.page.labels.created,
                        "icon": "icon-files",
                        "view": "views/packages/views/created.html",
                        "active": packageUri === "created",
                        "alias": "umbCreatedPackages",
                        "action": function () {
                            $location.path("/packages/packages/created");
                        }
                    }
                ];
            });
        }

        function setPageName() {
            localizationService.localize("sections_packages").then(function (data) {
                vm.page.name = data;
            })
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.OverviewController", PackagesOverviewController);

})();
