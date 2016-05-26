(function () {
    "use strict";

    function PackagesEditController($scope) {

        var vm = this;

        vm.page = {};
        vm.page.name = "Packages";
        

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.EditController", PackagesEditController);

})();
