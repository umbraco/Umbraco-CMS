(function () {
    "use strict";

    function PackagesOverviewController($scope, $location, localStorageService) {

        //Hack!
        // if there is a cookie value for packageInstallUri then we need to redirect there,
        // the issue is that we still have webforms and we cannot go to a hash location and then window.reload
        // because it will double load it.
        // we will refresh and then navigate there.

        let installPackageUri = localStorageService.get("packageInstallUri");
        let packageUri = $location.search().subview;

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

            packageUri = installPackageUri ? installPackageUri : packageUri; //use the path stored in storage over the one in the current path

            vm.page = {};
            vm.page.name = "Packages";
            vm.page.navigation = [
                {
                    "name": "Packages",
                    "icon": "icon-cloud",
                    "view": "views/packages/views/repo.html",
                    "active": !packageUri || packageUri === "navigation",
                    "alias": "umbPackages",
                    "action": function() {
                        $location.search("subview", "navigation");
                    }
                },
                {
                    "name": "Installed",
                    "icon": "icon-box",
                    "view": "views/packages/views/installed.html",
                    "active": packageUri === "installed",
                    "alias": "umbInstalled",
                    "action": function() {
                        $location.search("subview", "installed");
                    }
                },
                {
                    "name": "Install local",
                    "icon": "icon-add",
                    "view": "views/packages/views/install-local.html",
                    "active": packageUri === "local",
                    "alias": "umbInstallLocal",
                    "action": function() {
                        $location.search("subview", "local");
                    }
                },
                {
                    "name": "Created",
                    "icon": "icon-add",
                    "view": "views/packages/views/created.html",
                    "active": packageUri === "created",
                    "alias": "umbCreatedPackages",
                    "action": function() {
                        $location.search("subview", "created");
                    }
                }
            ];

        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.OverviewController", PackagesOverviewController);

})();
