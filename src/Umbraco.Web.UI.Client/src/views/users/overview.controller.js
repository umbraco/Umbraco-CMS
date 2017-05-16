(function () {
    "use strict";

    function UsersOverviewController($scope, $location) {

        var vm = this;
        var usersUri =  $location.search().subview;

        vm.page = {};
        vm.page.name = "User Management";
        vm.page.navigation = [
            {
                "name": "Users",
                "icon": "icon-user",
                "view": "views/users/views/users/users.html",
                "active": !usersUri || usersUri === "users"
            },
            {
                "name": "Roles",
                "icon": "icon-users",
                "view": "views/users/views/roles/roles.html",
                "active": usersUri === "roles"
            }
        ];

        function init() {

        }
 
        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.OverviewController", UsersOverviewController);

})();
