(function () {
    "use strict";

    function UsersOverviewController($scope, $location, $timeout, navigationService) {

        var vm = this;
        var usersUri = $location.search().subview;
        if (!usersUri) {
          $location.search("subview", "users")  
        }

        vm.page = {};
        vm.page.name = "User Management";
        vm.page.navigation = [
            {
                "name": "Users",
                "icon": "icon-user",
                "action": function() {
                  $location.search("subview", "users")
                },
                "view": "views/users/views/users/users.html",
                "active": !usersUri || usersUri === "users"
            },
            {
                "name": "Groups",
                "icon": "icon-users",
                "action": function () {
                  $location.search("subview", "groups")
                },
                "view": "views/users/views/groups/groups.html",
                "active": usersUri === "groups"
            }
        ];

        function init() {

            $timeout(function () {
                navigationService.syncTree({ tree: "users", path: "-1" });
            });

        }
 
        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.OverviewController", UsersOverviewController);

})();
