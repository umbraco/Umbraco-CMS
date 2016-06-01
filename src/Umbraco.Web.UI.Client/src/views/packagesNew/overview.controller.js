(function () {
    "use strict";

    function PackagesOverviewController($scope, $route, $location) {

        var vm = this;

        vm.page = {};
        vm.page.name = "Packages";
        vm.page.navigation = [
			{
			    "name": "Packages",
			    "icon": "icon-cloud",
			    "view": "views/packagesNew/views/repo.html",
			    "active": true
			},
			{
			    "name": "Installed",
			    "icon": "icon-box",
			    "view": "views/packagesNew/views/installed.html"
			},
			{
			    "name": "Install local",
			    "icon": "icon-add",
			    "view": "views/packagesNew/views/install-local.html"
			}
        ];

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.OverviewController", PackagesOverviewController);

})();
