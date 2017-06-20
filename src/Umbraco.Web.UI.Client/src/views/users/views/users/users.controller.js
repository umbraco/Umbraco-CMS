(function () {
    "use strict";

    function UsersController($scope, $timeout, $location, usersResource, localizationService, contentEditingHelper, usersHelper, formHelper, notificationsService) {

        var vm = this;
        var localizeSaving = localizationService.localize("general_saving");

        vm.users = [];
        vm.userGroups = [];
        vm.userStates = [];
        vm.selection = [];
        vm.newUser = {};
        vm.usersOptions = {};
        vm.userSortData = [
          { label: "Name (A-Z)", key: "Name", direction: "Ascending" },
          { label: "Name (Z-A)", key: "Name", direction: "Descending" },
          { label: "Newest", key: "CreateDate", direction: "Descending" },
          { label: "Oldest", key: "CreateDate", direction: "Ascending" },
          { label: "Last login", key: "LastLoginDate", direction: "Descending" }
        ];
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
        vm.selectLayout = selectLayout;
        vm.selectUser = selectUser;
        vm.clearSelection = clearSelection;
        vm.goToUser = goToUser;
        vm.disableUsers = disableUsers;
        vm.enableUsers = enableUsers;
        vm.openUserGroupPicker = openUserGroupPicker;
        vm.removeSelectedUserGroup = removeSelectedUserGroup;
        vm.selectAll = selectAll;
        vm.areAllSelected = areAllSelected;
        vm.searchUsers = searchUsers;
        vm.getGroupFilterName = getGroupFilterName;
        vm.setOrderByFilter = setOrderByFilter;
        vm.changePageNumber = changePageNumber;
        vm.createUser = createUser;
        vm.inviteUser = inviteUser;
        vm.getSortLabel = getSortLabel;

        function init() {

            vm.usersOptions.orderBy = "Name";
            vm.usersOptions.orderDirection = "Ascending";

            // Get users
            getUsers();

            // Get user groups
            usersResource.getUserGroups().then(function (userGroups) {
                vm.userGroups = userGroups;
            });

        }

        function getSortLabel(sortKey, sortDirection) {
          var found = _.find(vm.userSortData,
            function (i) {
              return i.key === sortKey && i.direction === sortDirection;
            });
          return found ? found.label : sortKey;
        }

        function setUsersViewState(state) {
            vm.usersViewState = state;
        }

        function selectLayout(selectedLayout) {
            angular.forEach(vm.layouts, function (layout) {
                layout.active = false;
            });
            selectedLayout.active = true;
            vm.activeLayout = selectedLayout;
        }

        function selectUser(user, selection) {
            if (user.selected) {
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
            angular.forEach(vm.users, function (user) {
                user.selected = false;
            });
            vm.selection = [];
        }

        function goToUser(user) {
            $location.path('users/users/user/' + user.id);
        }

        function disableUsers() {
            vm.disableUserButtonState = "busy";
            usersResource.disableUsers(vm.selection).then(function (data) {
                if (data === "true") {
                    // update userState
                    angular.forEach(vm.selection, function(userId){
                        var user = getUserFromArrayById(userId, vm.users);
                        if(user) {
                            user.userState = 1;
                        }
                    });
                    // show the correct badges
                    setUserDisplayState(vm.users);
                    // show notification
                    localizationService.localize("speechBubbles_disableUsersSuccess", [vm.selection.length]).then(function (value) {
                        notificationsService.success(value);
                    });
                    vm.disableUserButtonState = "init";
                    clearSelection();
                } else {
                    vm.disableUserButtonState = "error";
                    localizationService.localize("speechBubbles_disableUsersError").then(function (value) {
                        notificationsService.error(value);
                    });
                }
            }, function(error){
                vm.disableUserButtonState = "error";
                localizationService.localize("speechBubbles_disableUsersError").then(function (value) {
                    notificationsService.error(value);
                });
            });
        }

        function enableUsers() {
            vm.enableUserButtonState = "busy";
            usersResource.enableUsers(vm.selection).then(function (data) {
                if (data === "true") {
                    // update userState
                    angular.forEach(vm.selection, function(userId){
                        var user = getUserFromArrayById(userId, vm.users);
                        if(user) {
                            user.userState = 0;
                        }
                    });
                    // show the correct badges
                    setUserDisplayState(vm.users);
                    // show notification
                    localizationService.localize("speechBubbles_enableUsersSuccess", [vm.selection.length]).then(function (value) {
                        notificationsService.success(value);
                    });
                    vm.enableUserButtonState = "init";
                    clearSelection();
                } else {
                    vm.enableUserButtonState = "error";
                    localizationService.localize("speechBubbles_enableUsersError").then(function (value) {
                        notificationsService.error(value);
                    });
                }
            }, function (error) {
                vm.enableUserButtonState = "error";
                localizationService.localize("speechBubbles_enableUsersError").then(function (value) {
                    notificationsService.error(value);
                });
            });
        }

        function getUserFromArrayById(userId, users) {
            var userFound;
            angular.forEach(users, function(user){
                if(userId === user.id) {
                    userFound = user;
                }
            });
            return userFound;
        }

        function openUserGroupPicker(event) {
            vm.userGroupPicker = {
                title: "Select user groups",
                view: "usergrouppicker",
                selection: vm.newUser.userGroups,
                closeButtonLabel: "Cancel",
                show: true,
                submit: function (model) {
                    // apply changes
                    if (model.selection) {
                        vm.newUser.userGroups = model.selection;
                    }
                    vm.userGroupPicker.show = false;
                    vm.userGroupPicker = null;
                },
                close: function (oldModel) {
                    // rollback on close
                    if (oldModel.selection) {
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
            if (areAllSelected()) {
                vm.selection = [];
                angular.forEach(vm.users, function (user) {
                    user.selected = false;
                });
            } else {
                // clear selection so we don't add the same user twice
                vm.selection = [];
                // select all users
                angular.forEach(vm.users, function (user) {
                    user.selected = true;
                    vm.selection.push(user.id);
                });
            }
        }

        function areAllSelected() {
            if (vm.selection.length === vm.users.length) {
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

        function getGroupFilterName() {

            var name = "";
            var found = false;

            angular.forEach(vm.usersOptions.filter, function(value, index){
                angular.forEach(vm.userGroups, function(userGroup){
                    if(value === userGroup.alias) {
                        if(index === 0) {
                            name = userGroup.name;
                        } else {
                            name = name + ", " + userGroup.name;
                        }
                        found = true;
                    }
                });
            });

            if(!found) {
                name = "All";
            }

            return name;
        }

        function setOrderByFilter(value, direction) {
            vm.usersOptions.orderBy = value;
            vm.usersOptions.orderDirection = direction;
            getUsers();
        }

        function changePageNumber(pageNumber) {
            vm.usersOptions.pageNumber = pageNumber;
            getUsers();
        }

        function createUser(addUserForm) {

          if (formHelper.submitForm({ formCtrl: addUserForm,scope: $scope, statusMessage: "Saving..." })) {

            vm.newUser.id = -1;
            vm.newUser.parentId = -1;
            vm.page.createButtonState = "busy";

            usersResource.createUser(vm.newUser)
              .then(function (saved) {

                //success
                vm.page.createButtonState = "success";
                vm.newUser = saved;
                setUsersViewState('createUserSuccess');
                clearAddUserForm();

              }, function (err) {

                //error
                formHelper.handleError(err);
                vm.page.createButtonState = "error";
              });            
          }

        }

        function inviteUser(addUserForm) {

          if (formHelper.submitForm({ formCtrl: addUserForm, scope: $scope, statusMessage: "Saving..." })) {
            vm.newUser.id = -1;
            vm.newUser.parentId = -1;
            vm.page.createButtonState = "busy";

            usersResource.inviteUser(vm.newUser)
              .then(function (saved) {

                //success
                vm.page.createButtonState = "success";

              }, function (err) {

                //error
                formHelper.handleError(err);
                vm.page.createButtonState = "error";
              });                    
          }

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

                formatDates(vm.users);
                setUserDisplayState(vm.users);

                vm.loading = false;

            }, function(error){

                vm.loading = false;

            });
        }

        function setUserDisplayState(users) {
            angular.forEach(users, function(user){
                user.userDisplayState = usersHelper.getUserStateFromValue(user.userState);
            });
        }

        function formatDates(users) {
            angular.forEach(users, function (user) {
                if (user.lastLoginDate) {
                    user.formattedLastLogin = moment(user.lastLoginDate).format("MMMM Do YYYY, HH:mm");
                }
            });
        }

        function setBulkActions(users) {

            // reset all states
            vm.allowDisableUser = true;
            vm.allowEnableUser = true;
            vm.allowSetUserGroup = true;

            angular.forEach(users, function (user) {

                if (!user.selected) {
                    return;
                }

                if(user.userDisplayState.alias === "disabled") {
                    vm.allowDisableUser = false;
                }

                if(user.userDisplayState.alias === "active") {
                    vm.allowEnableUser = false;
                }

                if(user.userDisplayState.alias === "invited") {
                    vm.allowEnableUser = false;
                }

            });
        }

        function clearAddUserForm() {
            // clear form data
            vm.newUser.name = "";
            vm.newUser.email = "";
            vm.newUser.userGroups = [];
            vm.newUser.message = "";
            // clear button state
            vm.page.createButtonState = "init";
        }


        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.UsersController", UsersController);

})();
