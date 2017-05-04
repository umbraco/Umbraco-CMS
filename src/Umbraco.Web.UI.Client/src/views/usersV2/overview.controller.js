(function () {
    "use strict";

    function UsersOverviewController($scope, $timeout, usersResource) {

        var vm = this;

        vm.page = {};
        vm.page.name = "Users";
        vm.users = [];
        vm.userGroups = [];
        vm.usersViewState = 'overview';
        vm.usersPagination = {
            "pageNumber": 1,
            "totalPages": 5
        }

        vm.save = save;
        vm.setUsersViewState = setUsersViewState;
        vm.getUserStateType = getUserStateType;

        function init() {

            vm.loading = true;

            // Get users
            usersResource.getUsers().then(function (users) {
                vm.users = users;
            });

            // Get user groups
            usersResource.getUserGroups().then(function (userGroups) {
                vm.userGroups = userGroups;
            });

            // fake loading
            $timeout(function () {
                vm.loading = false;
            }, 500);

        }

        function save() {
            alert("save");
        }

        function setUsersViewState(state) {
            vm.usersViewState = state;
        }

        function getUserStateType(state) {
            switch (state) {
                case "disabled" || "umbracoDisabled":
                    return "danger";
                case "pending":
                    return "warning";
                default:
                    return "success";
            }
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.OverviewController", UsersOverviewController);

})();
