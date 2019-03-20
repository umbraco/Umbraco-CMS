(function () {
    "use strict";

    function UsersController($scope, $timeout, $location, $routeParams, usersResource, 
        userGroupsResource, userService, localizationService, contentEditingHelper, 
        usersHelper, formHelper, notificationsService, dateHelper, editorService, 
        listViewHelper) {

        var vm = this;
        var localizeSaving = localizationService.localize("general_saving");

        vm.page = {};
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

        localizationService.localizeMany(_.map(vm.userSortData, function (userSort) {
            return "user_sort" + userSort.key + userSort.direction;
        })).then(function (data) {
            var reg = /^\[[\S\s]*]$/g;
            _.each(data, function (value, index) {
                if (!reg.test(value)) {
                    // Only translate if key exists
                    vm.userSortData[index].label = value;
                }
            });
        });

        vm.labels = {};
        localizationService.localizeMany(["user_stateAll"]).then(function (data) {
            vm.labels.all = data[0];
        });

        vm.userStatesFilter = [];
        vm.newUser.userGroups = [];
        vm.usersViewState = 'overview';

        vm.selectedBulkUserGroups = [];

        vm.usernameIsEmail = Umbraco.Sys.ServerVariables.umbracoSettings.usernameIsEmail;

        vm.allowDisableUser = true;
        vm.allowEnableUser = true;
        vm.allowUnlockUser = true;
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

        // Get last selected layout for "users" (defaults to first layout = card layout)
        vm.activeLayout = listViewHelper.getLayout("users", vm.layouts); 

        // Don't show the invite button if no email is configured
        if (Umbraco.Sys.ServerVariables.umbracoSettings.showUserInvite) {
            vm.defaultButton = {
                labelKey: "user_inviteUser",
                handler: function () {
                    vm.setUsersViewState('inviteUser');
                }
            };
            vm.subButtons = [
                {
                    labelKey: "user_createUser",
                    handler: function () {
                        vm.setUsersViewState('createUser');
                    }
                }
            ];
        }
        else {
            vm.defaultButton = {
                labelKey: "user_createUser",
                handler: function () {
                    vm.setUsersViewState('createUser');
                }
            };
        }

        vm.toggleFilter = toggleFilter;
        vm.setUsersViewState = setUsersViewState;
        vm.selectLayout = selectLayout;
        vm.isSelectable = isSelectable;
        vm.selectUser = selectUser;
        vm.clearSelection = clearSelection;
        vm.clickUser = clickUser;
        vm.getEditPath = getEditPath;
        vm.disableUsers = disableUsers;
        vm.enableUsers = enableUsers;
        vm.unlockUsers = unlockUsers;
        vm.openBulkUserGroupPicker = openBulkUserGroupPicker;
        vm.openUserGroupPicker = openUserGroupPicker;
        vm.removeSelectedUserGroup = removeSelectedUserGroup;
        vm.selectAll = selectAll;
        vm.areAllSelected = areAllSelected;
        vm.searchUsers = searchUsers;
        vm.getFilterName = getFilterName;
        vm.setUserStatesFilter = setUserStatesFilter;
        vm.setUserGroupFilter = setUserGroupFilter;
        vm.setOrderByFilter = setOrderByFilter;
        vm.changePageNumber = changePageNumber;
        vm.createUser = createUser;
        vm.inviteUser = inviteUser;
        vm.getSortLabel = getSortLabel;
        vm.toggleNewUserPassword = toggleNewUserPassword;
        vm.copySuccess = copySuccess;
        vm.copyError = copyError;
        vm.goToUser = goToUser;

        function init() {

            vm.usersOptions.orderBy = "Name";
            vm.usersOptions.orderDirection = "Ascending";

            if ($routeParams.create) {
                setUsersViewState("createUser");
            }
            else if ($routeParams.invite) {
                setUsersViewState("inviteUser");
            }

            // Get users
            getUsers();

            // Get user groups
            userGroupsResource.getUserGroups({ onlyCurrentUserGroups: false }).then(function (userGroups) {
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

        function toggleFilter(type) {
            // hack: on-outside-click prevents us from closing the dropdown when clicking on another link
            // so I had to do this manually
            switch (type) {
                case "state":
                    vm.page.showStatusFilter = !vm.page.showStatusFilter;
                    vm.page.showGroupFilter = false;
                    vm.page.showOrderByFilter = false;
                    break;
                case "group":
                    vm.page.showGroupFilter = !vm.page.showGroupFilter;
                    vm.page.showStatusFilter = false;
                    vm.page.showOrderByFilter = false;
                    break;
                case "orderBy":
                    vm.page.showOrderByFilter = !vm.page.showOrderByFilter;
                    vm.page.showStatusFilter = false;
                    vm.page.showGroupFilter = false;
                    break;
            }
        }

        function setUsersViewState(state) {

            if (state === "createUser") {
                clearAddUserForm();

                $location.search("create", "true");
                $location.search("invite", null);
            }
            else if (state === "inviteUser") {
                $location.search("create", null);
                $location.search("invite", "true");
            }
            else if (state === "overview") {
                $location.search("create", null);
                $location.search("invite", null);
            }

            vm.usersViewState = state;
        }

        function selectLayout(selectedLayout) {
            // save the selected layout for "users" so it's applied next time the user visits this section
            vm.activeLayout = listViewHelper.setLayout("users", selectedLayout, vm.layouts); 
        }
        
        function isSelectable(user) {
            return !user.isCurrentUser;
        }
        
        function selectUser(user) {
            
            if (!isSelectable(user)) {
                return;
            }
            
            if (user.selected) {
                var index = vm.selection.indexOf(user.id);
                vm.selection.splice(index, 1);
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
        
        function clickUser(user, $event) {
            
            $event.stopPropagation();
            
            if ($event) {
                // targeting a new tab/window?
                if ($event.ctrlKey || 
                    $event.shiftKey ||
                    $event.metaKey || // apple
                    ($event.button && $event.button === 1) // middle click, >IE9 + everyone else
                ) {
                    // yes, let the link open itself
                    return;
                }
            }
            
            goToUser(user);
            $event.preventDefault();

        }

        function disableUsers() {
            vm.disableUserButtonState = "busy";
            usersResource.disableUsers(vm.selection).then(function (data) {
                // update userState
                angular.forEach(vm.selection, function (userId) {
                    var user = getUserFromArrayById(userId, vm.users);
                    if (user) {
                        user.userState = 1;
                    }
                });
                // show the correct badges
                setUserDisplayState(vm.users);

                vm.disableUserButtonState = "init";
                clearSelection();

            }, function (error) {
                vm.disableUserButtonState = "error";
            });
        }

        function enableUsers() {
            vm.enableUserButtonState = "busy";
            usersResource.enableUsers(vm.selection).then(function (data) {
                // update userState
                angular.forEach(vm.selection, function (userId) {
                    var user = getUserFromArrayById(userId, vm.users);
                    if (user) {
                        user.userState = 0;
                    }
                });
                // show the correct badges
                setUserDisplayState(vm.users);
                vm.enableUserButtonState = "init";
                clearSelection();
            }, function (error) {
                vm.enableUserButtonState = "error";
            });
        }

        function unlockUsers() {
            vm.unlockUserButtonState = "busy";
            usersResource.unlockUsers(vm.selection).then(function (data) {
                // update userState
                angular.forEach(vm.selection, function (userId) {
                    var user = getUserFromArrayById(userId, vm.users);
                    if (user) {
                        user.userState = 0;
                    }
                });
                // show the correct badges
                setUserDisplayState(vm.users);
                vm.unlockUserButtonState = "init";
                clearSelection();
            }, function (error) {
                vm.unlockUserButtonState = "error";
            });
        }

        function getUserFromArrayById(userId, users) {
            return _.find(users, function (u) { return u.id === userId });
        }

        function openBulkUserGroupPicker() {
            var firstSelectedUser = getUserFromArrayById(vm.selection[0], vm.users);

            vm.selectedBulkUserGroups = _.clone(firstSelectedUser.userGroups);

            var userGroupPicker = {
                selection: vm.selectedBulkUserGroups,
                submit: function (model) {
                    usersResource.setUserGroupsOnUsers(model.selection, vm.selection).then(function (data) {
                        // sorting to ensure they show up in right order when updating the UI
                        vm.selectedBulkUserGroups.sort(function (a, b) {
                            return a.alias > b.alias ? 1 : a.alias < b.alias ? -1 : 0;
                        });
                        // apply changes to UI
                        _.each(vm.selection,
                            function (userId) {
                                var user = getUserFromArrayById(userId, vm.users);
                                user.userGroups = vm.selectedBulkUserGroups;
                            });
                        vm.selectedBulkUserGroups = [];
                        editorService.close();
                        clearSelection();
                    }, angular.noop);
                },
                close: function () {
                    vm.selectedBulkUserGroups = [];
                    editorService.close();
                }
            };
            editorService.userGroupPicker(userGroupPicker);
        }

        function openUserGroupPicker() {
            var oldSelection = angular.copy(vm.newUser.userGroups);
            var userGroupPicker = {
                selection: vm.newUser.userGroups,
                submit: function (model) {
                    // apply changes
                    if (model.selection) {
                        vm.newUser.userGroups = model.selection;
                    }
                    editorService.close();
                },
                close: function () {
                    // rollback on close
                    vm.newUser.userGroups = oldSelection;
                    editorService.close();
                }
            };
            editorService.userGroupPicker(userGroupPicker);
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
                    // prevent the current user to be selected
                    if (!user.isCurrentUser) {
                        user.selected = true;
                        vm.selection.push(user.id);
                    }
                });
            }
        }

        function areAllSelected() {
            // we need to check if the current user is part of the selection and 
            // subtract the user from the total selection to find out if all users are selected
            var includesCurrentUser = vm.users.some(function (user) { return user.isCurrentUser === true; });

            if (includesCurrentUser) {
                if (vm.selection.length === vm.users.length - 1) { return true; }
            } else {
                if (vm.selection.length === vm.users.length) { return true; }
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

        function getFilterName(array) {
            var name = vm.labels.all;
            var found = false;
            angular.forEach(array, function (item) {
                if (item.selected) {
                    if (!found) {
                        name = item.name
                        found = true;
                    } else {
                        name = name + ", " + item.name;
                    }
                }
            });
            return name;
        }

        function setUserStatesFilter(userState) {

            if (!vm.usersOptions.userStates) {
                vm.usersOptions.userStates = [];
            }

            //If the selection is "ALL" then we need to unselect everything else since this is an 'odd' filter
            if (userState.key === "All") {
                angular.forEach(vm.userStatesFilter, function (i) {
                    i.selected = false;
                });
                //we can't unselect All
                userState.selected = true;
                //reset the selection passed to the server
                vm.usersOptions.userStates = [];
            }
            else {
                angular.forEach(vm.userStatesFilter, function (i) {
                    if (i.key === "All") {
                        i.selected = false;
                    }
                });
                var indexOfAll = vm.usersOptions.userStates.indexOf("All");
                if (indexOfAll >= 0) {
                    vm.usersOptions.userStates.splice(indexOfAll, 1);
                }
            }

            if (userState.selected) {
                vm.usersOptions.userStates.push(userState.key);
            }
            else {
                var index = vm.usersOptions.userStates.indexOf(userState.key);
                vm.usersOptions.userStates.splice(index, 1);
            }

            getUsers();
        }

        function setUserGroupFilter(userGroup) {

            if (!vm.usersOptions.userGroups) {
                vm.usersOptions.userGroups = [];
            }

            if (userGroup.selected) {
                vm.usersOptions.userGroups.push(userGroup.alias);
            } else {
                var index = vm.usersOptions.userGroups.indexOf(userGroup.alias);
                vm.usersOptions.userGroups.splice(index, 1);
            }

            getUsers();
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

            if (formHelper.submitForm({ formCtrl: addUserForm, scope: $scope })) {

                vm.newUser.id = -1;
                vm.newUser.parentId = -1;
                vm.page.createButtonState = "busy";

                usersResource.createUser(vm.newUser)
                    .then(function (saved) {
                        vm.page.createButtonState = "success";
                        vm.newUser = saved;
                        setUsersViewState('createUserSuccess');
                        getUsers();
                    }, function (err) {
                        formHelper.handleError(err);
                        vm.page.createButtonState = "error";
                    });
            }

        }

        function inviteUser(addUserForm) {

            if (formHelper.submitForm({ formCtrl: addUserForm, scope: $scope })) {
                vm.newUser.id = -1;
                vm.newUser.parentId = -1;
                vm.page.createButtonState = "busy";

                usersResource.inviteUser(vm.newUser)
                    .then(function (saved) {
                        //success
                        vm.page.createButtonState = "success";
                        vm.newUser = saved;
                        setUsersViewState('inviteUserSuccess');
                        getUsers();
                    }, function (err) {
                        //error
                        formHelper.handleError(err);
                        vm.page.createButtonState = "error";
                    });
            }

        }

        function toggleNewUserPassword() {
            vm.newUser.showPassword = !vm.newUser.showPassword;
        }

        // copy to clip board success
        function copySuccess() {
            if (vm.page.copyPasswordButtonState !== "success") {
                $timeout(function(){
                    vm.page.copyPasswordButtonState = "success";
                });
                $timeout(function () {
                    resetClipboardButtonState();
                }, 1000);
            }
        }

        // copy to clip board error
        function copyError() {
            if (vm.page.copyPasswordButtonState !== "error") {
                $timeout(function() {
                    vm.page.copyPasswordButtonState = "error";
                });
                $timeout(function () {
                    resetClipboardButtonState();
                }, 1000);
            }
        }

        function resetClipboardButtonState() {
            vm.page.copyPasswordButtonState = "init";
        }

        function goToUser(user) {
            $location.path(pathToUser(user)).search("create", null).search("invite", null);
        }
        
        function getEditPath(user) {
            return pathToUser(user) + "?mculture=" + $location.search().mculture;
        }
        
        function pathToUser(user) {
            return "/users/users/user/" + user.id;
        }

        // helpers
        function getUsers() {

            vm.loading = true;

            // Get users
            usersResource.getPagedResults(vm.usersOptions).then(function (data) {

                vm.users = data.items;

                vm.usersOptions.pageNumber = data.pageNumber;
                vm.usersOptions.pageSize = data.pageSize;
                vm.usersOptions.totalItems = data.totalItems;
                vm.usersOptions.totalPages = data.totalPages;
                
                formatDates(vm.users);
                setUserDisplayState(vm.users);
                vm.userStatesFilter = usersHelper.getUserStatesFilter(data.userStates);

                vm.loading = false;

            }, function (error) {

                vm.loading = false;

            });
        }

        function setUserDisplayState(users) {
            angular.forEach(users, function (user) {
                user.userDisplayState = usersHelper.getUserStateFromValue(user.userState);
            });
        }

        function formatDates(users) {
            angular.forEach(users, function (user) {
                if (user.lastLoginDate) {
                    var dateVal;
                    var serverOffset = Umbraco.Sys.ServerVariables.application.serverTimeOffset;
                    var localOffset = new Date().getTimezoneOffset();
                    var serverTimeNeedsOffsetting = (-serverOffset !== localOffset);

                    if (serverTimeNeedsOffsetting) {
                        dateVal = dateHelper.convertToLocalMomentTime(user.lastLoginDate, serverOffset);
                    } else {
                        dateVal = moment(user.lastLoginDate, "YYYY-MM-DD HH:mm:ss");
                    }

                    // get current backoffice user and format date
                    userService.getCurrentUser().then(function (currentUser) {
                        user.formattedLastLogin = dateVal.locale(currentUser.locale).format("LLL");
                    });
                }
            });
        }

        function setBulkActions(users) {

            // reset all states
            vm.allowDisableUser = true;
            vm.allowEnableUser = true;
            vm.allowUnlockUser = true;
            vm.allowSetUserGroup = true;

            var firstSelectedUserGroups;

            angular.forEach(users, function (user) {
                
                if (!user.selected) {
                    return;
                }
                
                
                // if the current user is selected prevent any bulk actions with the user included
                if (user.isCurrentUser) {
                    vm.allowDisableUser = false;
                    vm.allowEnableUser = false;
                    vm.allowUnlockUser = false;
                    vm.allowSetUserGroup = false;
                    
                    return false;
                }

                if (user.userDisplayState && user.userDisplayState.key === "Disabled") {
                    vm.allowDisableUser = false;
                }

                if (user.userDisplayState && user.userDisplayState.key === "Active") {
                    vm.allowEnableUser = false;
                }

                if (user.userDisplayState && user.userDisplayState.key === "Invited") {
                    vm.allowEnableUser = false;
                }

                if (user.userDisplayState && user.userDisplayState.key === "LockedOut") {
                    vm.allowEnableUser = false;
                }

                if (user.userDisplayState && user.userDisplayState.key !== "LockedOut") {
                    vm.allowUnlockUser = false;
                }

                // store the user group aliases of the first selected user
                if (vm.allowSetUserGroup === true) {
                    if (!firstSelectedUserGroups) {
                        firstSelectedUserGroups = user.userGroups.map(function (ug) { return ug.alias; });
                    } else {
                        // for 2nd+ selected user, compare the user group aliases to determine if we should allow bulk editing.
                        // we don't allow bulk editing of users not currently having the same assigned user groups, as we can't
                        // really support that in the user group picker.
                        var userGroups = user.userGroups.map(function (ug) { return ug.alias; });
                        if (_.difference(firstSelectedUserGroups, userGroups).length > 0) {
                            vm.allowSetUserGroup = false;
                        }
                    }
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
