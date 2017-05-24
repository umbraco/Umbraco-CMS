(function () {
    "use strict";

    function UsersController($scope, $timeout, $location, usersResource, localizationService, contentEditingHelper) {

        var vm = this;
        var localizeSaving = localizationService.localize("general_saving");

        vm.users = [];
        vm.userGroups = [];
        vm.userStates = [];
        vm.selection = [];
        vm.newUser = {};
        vm.usersOptions = {};
        vm.newUser.userGroups = [];
        vm.usersViewState = 'overview';
        
        vm.allowDisableUser = true;
        vm.allowEnableUser = true;
        vm.allowSetUserGroup = true;
        
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

        //don't set this if no email is configured
        if (Umbraco.Sys.ServerVariables.umbracoSettings.emailServerConfigured) {
          vm.defaultButton = {
            labelKey: "users_inviteUser",
            handler: function () {
              vm.setUsersViewState('inviteUser');
            }
          };  
        }

        vm.subButtons = [
            {
                labelKey: "users_createUser",
                handler: function () {
                    vm.setUsersViewState('createUser');
                }
            }
        ];

        vm.setUsersViewState = setUsersViewState;
        vm.getUserStateType = getUserStateType;
        vm.selectLayout = selectLayout;
        vm.selectUser = selectUser;
        vm.clearSelection = clearSelection;
        vm.goToUser = goToUser;
        vm.disableUser = disableUser;
        vm.openUserGroupPicker = openUserGroupPicker;
        vm.removeSelectedUserGroup = removeSelectedUserGroup;
        vm.selectAll = selectAll;
        vm.areAllSelected = areAllSelected;
        vm.searchUsers = searchUsers;
        vm.setOrderByFilter = setOrderByFilter;
        vm.createUser = createUser;

        function init() {

            vm.usersOptions.orderBy = "Name";

            // Get users
            getUsers();

            // Get user groups
            usersResource.getUserGroups().then(function (userGroups) {
                vm.userGroups = userGroups;
            });

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

        function selectUser(user, selection) {
            if(user.selected) {
                var index = selection.indexOf(user.id);
                selection.splice(index, 1);
                user.selected = false;
            } else {
                user.selected = true;
                vm.selection.push(user.id);
            }

            setBulkActions(vm.users);

        }

        function clearSelection() {
            angular.forEach(vm.users, function(user){
                user.selected = false;
            });
            vm.selection = [];
        }

        function goToUser(user, event) {
            $location.path('users/users/user/' + user.id);
        }

        function disableUser() {
            alert("disable users");
        }

        function openUserGroupPicker(event) {
            vm.userGroupPicker = {
                title: "Select user groups",
                view: "usergrouppicker",
                selection: vm.newUser.userGroups,
                closeButtonLabel: "Cancel",
                show: true,
                submit: function(model) {
                    // apply changes
                    if(model.selection) {
                        vm.newUser.userGroups = model.selection;
                    }
                    vm.userGroupPicker.show = false;
                    vm.userGroupPicker = null;
                },
                close: function(oldModel) {
                    // rollback on close
                    if(oldModel.selection) {
                        vm.newUser.userGroups = oldModel.selection;
                    }
                    vm.userGroupPicker.show = false;
                    vm.userGroupPicker = null;
                }
            };
        }

        function removeSelectedUserGroup(index, selection) {
            selection.splice(index, 1);
        }

        function selectAll() {
            if(areAllSelected()) {
                vm.selection = [];
                angular.forEach(vm.users, function(user){
                    user.selected = false;
                });
            } else {
                // clear selection so we don't add the same user twice
                vm.selection = [];
                // select all users
                angular.forEach(vm.users, function(user){
                    user.selected = true;
                    vm.selection.push(user.id);
                });
            }
        }

        function areAllSelected() {
            if(vm.selection.length === vm.users.length) {
                return true;
            }
        }

        var search = _.debounce(function () {
            $scope.$apply(function () {
                getUsers();
            });
        }, 500);

        function searchUsers() {
            search();
        }

        function setOrderByFilter(value) {
            vm.usersOptions.orderBy = value;
            getUsers();
        }

        function createUser() {

            vm.newUser.id = -1;
            vm.newUser.parentId = -1;
            vm.page.createButtonState = "busy";

            contentEditingHelper.contentEditorPerformSave({
                statusMessage: localizeSaving,
                saveMethod: usersResource.createUser,
                scope: $scope,
                content: vm.newUser,
                // We do not redirect on failure for users - this is because it is not possible to actually save a user
                // when server side validation fails - as opposed to content where we are capable of saving the content
                // item if server side validation fails
                redirectOnFailure: false,
                rebindCallback: function (orignal, saved) {}
            }).then(function (saved) {

                vm.page.createButtonState = "success";

            }, function (err) {

                vm.page.createButtonState = "error";

            });

        }

        // helpers
        function getUsers() {

            vm.loading = true;

            // Get users
            usersResource.getPagedResults(vm.usersOptions).then(function (users) {
                
                vm.users = users.items;

                vm.usersOptions.pageNumber = users.pageNumber;
                vm.usersOptions.pageSize = users.pageSize;
                vm.usersOptions.totalItems = users.totalItems;
                vm.usersOptions.totalPages = users.totalPages;

                vm.userStates = getUserStates(vm.users);
                formatDates(vm.users);

                vm.loading = false;

            });
        }

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

        function formatDates(users) {
            angular.forEach(users, function(user){
                if(user.lastLogin) {
                    user.formattedLastLogin = moment(user.lastLogin).format("MMMM Do YYYY, HH:mm");
                }
            });
        }

        function setBulkActions(users) {

            // reset all states
            vm.allowDisableUser = true;
            vm.allowEnableUser = true;
            vm.allowSetUserGroup = true;

            angular.forEach(users, function(user){

                if(!user.selected) {
                    return;
                }

                if(user.state === "disabled") {
                    vm.allowDisableUser = false;
                } 
                
                if(user.state === "active") {
                    vm.allowEnableUser = false;
                }

                if(user.state === "pending") {
                    vm.allowEnableUser = false;
                }

            });
        }


        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.UsersController", UsersController);

})();
