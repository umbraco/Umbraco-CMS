(function () {
    "use strict";

    function UsersOverviewController($scope, $location, $routeParams, localizationService) {

        var vm = this;
        let usersUri = $routeParams.method;
        
        //note on the below, we dont assign a view unless it's the right route since if we did that it will load in that controller
        //for the view which is unecessary and will cause extra overhead/requests to occur
        vm.page = {};
        vm.page.labels = {};
        vm.page.name = "";
        vm.page.navigation = [];

        function onInit() {
            loadNavigation();
        }

        function loadNavigation() {

            var labels = ["sections_users", "general_groups", "user_userManagement"];

            localizationService.localizeMany(labels).then(function (data) {
                vm.page.labels.users = data[0];
                vm.page.labels.groups = data[1];
                vm.page.name = data[2];

                vm.page.navigation = [
                    {
                        "name": vm.page.labels.users,
                        "icon": "icon-user",
                        "action": function () {
                            $location.path("/users/users/users").search("create", null);
                        },
                        "view": !usersUri || usersUri === "users" ? "views/users/views/users/users.html" : null,
                        "active": !usersUri || usersUri === "users",
                        "alias": "users"
                    },
                    {
                        "name": vm.page.labels.groups,
                        "icon": "icon-users",
                        "action": function () {
                            $location.path("/users/users/groups").search("create", null);
                        },
                        "view": usersUri === "groups" ? "views/users/views/groups/groups.html" : null,
                        "active": usersUri === "groups",
                        "alias": "userGroups"
                    }
                ];
            });
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.OverviewController", UsersOverviewController);

})();
