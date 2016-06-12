(function () {
    "use strict";

    function PackagesOverviewController($scope, $route, $location, navigationService, $timeout) {

        var vm = this;

        vm.page = {};
        vm.page.name = "Packages";
        vm.page.navigation = [
			{
			    "name": "Packages",
			    "icon": "icon-cloud",
			    "view": "views/packager/views/repo.html",
			    "active": true
			},
			{
			    "name": "Installed",
			    "icon": "icon-box",
			    "view": "views/packager/views/installed.html"
			},
			{
			    "name": "Install local",
			    "icon": "icon-add",
			    "view": "views/packager/views/install-local.html"
			}
        ];

        $timeout(function () {
            navigationService.syncTree({ tree: "packager", path: "-1" });
        });

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.OverviewController", PackagesOverviewController);

})();
