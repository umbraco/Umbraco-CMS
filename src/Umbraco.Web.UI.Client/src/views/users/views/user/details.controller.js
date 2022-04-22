(function () {
    "use strict";

    function DetailsController($scope, externalLoginInfoService) {

        var vm = this;

        vm.denyLocalLogin = externalLoginInfoService.hasDenyLocalLogin();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.DetailsController", DetailsController);

})();
