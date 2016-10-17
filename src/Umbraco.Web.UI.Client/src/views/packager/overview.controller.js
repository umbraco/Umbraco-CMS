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
                    "view": "views/packager/views/repo.html",
                    "active": !installPackageUri || installPackageUri === "navigation"
                },
                {
                    "name": "Installed",
                    "icon": "icon-box",
                    "view": "views/packager/views/installed.html",
                    "active": installPackageUri === "installed"
                },
                {
                    "name": "Install local",
                    "icon": "icon-add",
                    "view": "views/packager/views/install-local.html",
                    "active": installPackageUri === "local"
                }
            ];

            $timeout(function () {
                navigationService.syncTree({ tree: "packager", path: "-1" });
            });
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.OverviewController", PackagesOverviewController);

})();
