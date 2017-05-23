(function () {
    "use strict";

    function UserEditController($scope, $timeout, $location, usersResource, $routeParams) {

        var vm = this;

        vm.loading = false;
        vm.page = {};
        vm.user = {};

        vm.goBack = goBack;

        function init() {

            vm.loading = true;

            // get user
            usersResource.getUser($routeParams.id).then(function (user) {
                vm.user = user;
            });

            // fake loading
            $timeout(function () {
                vm.loading = false;
            }, 500);
            
        }

        function goBack() {
            $location.path("/users/users/overview").search("subview", "users");
        }
 
        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.UserController", UserEditController);

})();
