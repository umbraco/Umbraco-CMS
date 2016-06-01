(function () {
    "use strict";

    function PackagesInstallLocalController($scope, $route, $location) {

        var vm = this;
        vm.state = "upload";

        vm.localPackage = {
            "icon":"https://our.umbraco.org/media/wiki/154472/635997115126742822_logopng.png?bgcolor=fff&height=154&width=281&format=png",
            "name": "SvgIconPicker Version: 0.1.0",
            "author": "Søren Kottal",
            "authorLink": "https://github.com/skttl/",
            "info": "https://github.com/skttl/Umbraco.SvgIconPicker",
            "licens": "GPLv3",
            "licensLink": "http://www.gnu.org/licenses/quick-guide-gplv3.en.html",
            "licensAccept": false,
            "readme": "Color Palettes is a simple property editor that let you define different color palettes (or get them from Adobe Kuler or COLOURlovers) and present them to the editor as a list of radio buttons.",
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
