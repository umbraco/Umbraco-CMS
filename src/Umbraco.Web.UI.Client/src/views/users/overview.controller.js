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
                "name": "Groups",
                "icon": "icon-users",
                "view": "views/users/views/groups/groups.html",
                "active": usersUri === "groups"
            }
        ];

        function init() {

        }
 
        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.OverviewController", UsersOverviewController);

})();
