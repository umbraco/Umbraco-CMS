(function () {
    "use strict";

    function UsersOverviewController($scope, $timeout, usersResource) {

        var vm = this;

        vm.page = {};
        vm.page.name = "Users";
        vm.users = [];
        vm.userGroups = [];
        vm.userStates = [];
        vm.usersViewState = 'overview';
        vm.usersPagination = {
            "pageNumber": 1,
            "totalPages": 5
        }
        
        vm.layouts = [
            {
                "icon": "icon-thumbnails-small",
                "path": "1",
                "selected": true
            },
            {
                "icon": "icon-list",
                "path": "2",
                "selected": true
            }
        ];

        vm.activeLayout = {
            "icon": "icon-thumbnails-small",
            "path": "1",
            "selected": true
        };

        vm.defaultButton = {
            labelKey:"users_inviteUser",
            handler: function() {
                vm.setUsersViewState('inviteUser');
            }
        };

        vm.subButtons = [
            {
                labelKey: "users_createUser",
                handler: function () {
                    vm.setUsersViewState('createUser');
                }
            }
        ];

        vm.save = save;
        vm.setUsersViewState = setUsersViewState;
        vm.getUserStateType = getUserStateType;
        vm.selectLayout = selectLayout;

        function init() {

            vm.loading = true;

            // Get users
            usersResource.getUsers().then(function (users) {
                vm.users = users;
                vm.userStates = getUserStates(users);
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

        function selectLayout(selectedLayout) {

            angular.forEach(vm.layouts, function(layout){
                layout.active = false;
            });

            selectedLayout.active = true;
            vm.activeLayout = selectedLayout;
        }


        // helpers
        function getUserStates(users) {
            var userStates = [];
            
            angular.forEach(users, function(user) {

                var newUserState = {"name": user.state, "count": 1};
                var userStateExists = false;

                angular.forEach(userStates, function(userState){
                    if(newUserState.name === userState.name) {
                        userState.count = userState.count + 1;
                        userStateExists = true;
                    }
                });

                if(userStateExists === false) {
                    userStates.push(newUserState);
                }

            });

            return userStates;
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.OverviewController", UsersOverviewController);

})();
