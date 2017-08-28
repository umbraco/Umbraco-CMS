(function () {
    "use strict";

    function UserGroupsController($scope, $timeout, $location, userService, userGroupsResource, formHelper, localizationService) {

        var vm = this;

        vm.userGroups = [];
        vm.selection = [];

        vm.createUserGroup = createUserGroup;
        vm.clickUserGroup = clickUserGroup;
        vm.clearSelection = clearSelection;
        vm.selectUserGroup = selectUserGroup;
        vm.deleteUserGroups = deleteUserGroups;

        var currentUser = null;

        function onInit() {

            vm.loading = true;

            userService.getCurrentUser().then(function(user) {
                currentUser = user;
                // Get usergroups
                userGroupsResource.getUserGroups({ onlyCurrentUserGroups: false }).then(function (userGroups) {
                    vm.userGroups = _.map(userGroups, function (ug) {
                        return { group: ug, isMember: user.userGroups.indexOf(ug.alias) !== -1}  
                    });
                    vm.loading = false;
                });
            });

        }

        function createUserGroup() {
            // clear all query params
            $location.search({});
            // go to create user group
            $location.path('users/users/group/-1').search("create", "true");;
        }

        function clickUserGroup(userGroup) {

            if (currentUser.userGroups.indexOf(userGroup.group.alias) === -1) {
                return;
            }

            if (vm.selection.length > 0) {
                selectUserGroup(userGroup, vm.selection);
            } else {
                goToUserGroup(userGroup.group.id);
            }
        }

        function selectUserGroup(userGroup, selection, event) {

            if (currentUser.userGroups.indexOf(userGroup.group.alias) === -1) {
                return;
            }

            if (userGroup.selected) {
                var index = selection.indexOf(userGroup.group.id);
                selection.splice(index, 1);
                userGroup.selected = false;
            } else {
                userGroup.selected = true;
                vm.selection.push(userGroup.group.id);
            }

            if(event){
                event.preventDefault();
                event.stopPropagation();
            }
        }

        function deleteUserGroups() {

            if(vm.selection.length > 0) {

                localizationService.localize("defaultdialogs_confirmdelete")
                    .then(function(value) {

                        var confirmResponse = confirm(value);

                        if (confirmResponse === true) {
                            userGroupsResource.deleteUserGroups(vm.selection).then(function (data) {
                                clearSelection();
                                onInit();
                                formHelper.showNotifications(data);
                            }, function(error) {
                                formHelper.showNotifications(error.data);
                            });
                        }

                    });

            }
        }

        function clearSelection() {
            angular.forEach(vm.userGroups, function (userGroup) {
                userGroup.selected = false;
            });
            vm.selection = [];
        }

        function goToUserGroup(userGroupId) {
            $location.path('users/users/group/' + userGroupId).search("create", null);
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.GroupsController", UserGroupsController);

})();
