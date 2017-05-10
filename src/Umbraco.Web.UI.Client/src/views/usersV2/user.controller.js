(function () {
    "use strict";

    function UserEditController($scope, $timeout, usersResource) {

        var vm = this;

        vm.loading = false;
        vm.page = {};
        vm.user = {};

        function init() {

            vm.loading = true;

            // get user
            usersResource.getUser().then(function (user) {
                vm.user = user;
            });

            // fake loading
            $timeout(function () {
                vm.loading = false;
            }, 500);
            
        }
 
        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.UserController", UserEditController);

})();
