(function () {
    "use strict";

    function UserRoleEditController($scope, $timeout, usersResource) {

        var vm = this;

        vm.loading = false;
        vm.page = {};
        vm.userRole = {};

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
 
        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.RoleController", UserRoleEditController);

})();
