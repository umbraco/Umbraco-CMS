(function () {
    "use strict";

    function UsersOverviewController($scope, $location, $timeout, navigationService, localizationService) {

        var vm = this;
        var usersUri = $location.search().subview;
        if (!usersUri) {
            $location.search("subview", "users");
            //exit after this, we don't want to initialize anything further since this
            //is going to change the route
            return;
        }

        //note on the below, we dont assign a view unless it's the right route since if we did that it will load in that controller
        //for the view which is unecessary and will cause extra overhead/requests to occur
        vm.page = {};
        vm.page.name = localizationService.localize("user_userManagement");
        vm.page.navigation = [
            {
                "name": localizationService.localize("sections_users"),
                "icon": "icon-user",
                "action": function() {
                  $location.search("subview", "users")
                },
                "view": !usersUri || usersUri === "users" ? "views/users/views/users/users.html" : null,
                "active": !usersUri || usersUri === "users"
            },
            {
                "name": localizationService.localize("general_groups"),
                "icon": "icon-users",
                "action": function () {
                  $location.search("subview", "groups")
                },
                "view": usersUri === "groups" ? "views/users/views/groups/groups.html" : null,
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
