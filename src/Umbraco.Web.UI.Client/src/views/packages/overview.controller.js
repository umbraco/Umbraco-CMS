(function () {
    "use strict";

    function PackagesOverviewController($scope, $route, $location, navigationService, $timeout, localStorageService) {

        //Hack!
        // if there is a cookie value for packageInstallUri then we need to redirect there,
        // the issue is that we still have webforms and we cannot go to a hash location and then window.reload
        // because it will double load it.
        // we will refresh and then navigate there.

        var installPackageUri = localStorageService.get("packageInstallUri");
        if (installPackageUri) {            
            localStorageService.remove("packageInstallUri");                       
        }
        if (installPackageUri && installPackageUri !== "installed") {
            //navigate to the custom installer screen, if it is just "installed", then we'll 
            //show the installed view
            $location.path(installPackageUri).search("");
        }
        else {
            var vm = this;

            vm.page = {};
            vm.page.name = "Packages";
            vm.page.navigation = [
                {
                    "name": "Packages",
                    "icon": "icon-cloud",
                    "view": "views/packages/views/repo.html",
                    "active": !installPackageUri || installPackageUri === "navigation",
                    "alias": "umbPackages"
                },
                {
                    "name": "Installed",
                    "icon": "icon-box",
                    "view": "views/packages/views/installed.html",
                    "active": installPackageUri === "installed",
                    "alias": "umbInstalled"
                },
                {
                    "name": "Install local",
                    "icon": "icon-add",
                    "view": "views/packages/views/install-local.html",
                    "active": installPackageUri === "local",
                    "alias": "umbInstallLocal"
                },
                {
                    "name": "Created",
                    "icon": "icon-add",
                    "view": "views/packages/views/created.html",
                    "alias": "umbCreatedPackages"
                }
            ];

        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.OverviewController", PackagesOverviewController);

})();
