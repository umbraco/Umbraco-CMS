(function () {
    "use strict";

    function PackagesInstallLocalController($scope, $route, $location) {

        var vm = this;

        vm.state = "upload";
        vm.localPackage = {
            "packageName": "SvgIconPicker Version: 0.1.0",
            "packageAuthor": "Søren Kottal",
            "packageAuthorLink": "https://github.com/skttl/",
            "packageInfo": "https://github.com/skttl/Umbraco.SvgIconPicker",
            "packageLicens": "GPLv3",
            "packageLicensLink": "http://www.gnu.org/licenses/quick-guide-gplv3.en.html",
            "packageLicensAccept": false,
            "packageReadme": false,
            "filePath": "",
            "riskAccept": false
        };

        vm.loadPackage = loadPackage;

        function loadPackage(){
            vm.state = "packageDetails";
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.InstallLocalController", PackagesInstallLocalController);

})();
