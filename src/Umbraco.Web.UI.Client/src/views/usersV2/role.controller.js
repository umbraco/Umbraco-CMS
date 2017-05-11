(function () {
    "use strict";

    function UserRoleEditController($scope, $timeout, $location, usersResource) {

        var vm = this;

        vm.loading = false;
        vm.page = {};
        vm.userRole = {};

        vm.goBack = goBack;

        function init() {

            vm.loading = true;

            // get user
            usersResource.getUserRole().then(function (userRole) {
                vm.userRole = userRole;
            });

            // fake loading
            $timeout(function () {
                vm.loading = false;
            }, 500);
            
        }

        function goBack() {
            $location.path("users/usersV2/overview");
        }
 
        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.RoleController", UserRoleEditController);

})();
