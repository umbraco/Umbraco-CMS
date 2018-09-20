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
        vm.page.labels = {};
        vm.page.name = "";
        vm.page.navigation = [];

        function onInit() {

            loadNavigation();

            setPageName();

            $timeout(function () {
                navigationService.syncTree({ tree: "users", path: "-1" });
            });

        }

        function loadNavigation() {

            var labels = ["sections_users", "general_groups"];

            localizationService.localizeMany(labels).then(function(data){
                vm.page.labels.users = data[0];
                vm.page.labels.groups = data[1];

                vm.page.navigation = [
                    {
                        "name": vm.page.labels.users,
                        "icon": "icon-user",
                        "action": function() {
                          $location.search("subview", "users")
                        },
                        "view": !usersUri || usersUri === "users" ? "views/users/views/users/users.html" : null,
                        "active": !usersUri || usersUri === "users",
                        "alias": "users"
                    },
                    {
                        "name": vm.page.labels.groups,
                        "icon": "icon-users",
                        "action": function () {
                          $location.search("subview", "groups")
                        },
                        "view": usersUri === "groups" ? "views/users/views/groups/groups.html" : null,
                        "active": usersUri === "groups",
                        "alias": "userGroups"
                    }
                ];
            });
        }

        function setPageName() {
            localizationService.localize("user_userManagement").then(function(data){
                vm.page.name = data;
            })
        }
 
        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.OverviewController", UsersOverviewController);

})();
