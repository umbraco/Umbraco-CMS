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

                    // only allow editing and selection if user is member of the group or admin
                    vm.userGroups = _.map(userGroups, function (ug) {
                        return { group: ug, hasAccess: user.userGroups.indexOf(ug.alias) !== -1 || user.userGroups.indexOf("admin") !== -1}
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

            // only allow editing if user is member of the group or admin
            if (currentUser.userGroups.indexOf(userGroup.group.alias) === -1 && currentUser.userGroups.indexOf("admin") === -1) {
                return;
            }

            if (vm.selection.length > 0) {
                selectUserGroup(userGroup, vm.selection);
            } else {
                goToUserGroup(userGroup.group.id);
            }
        }

        function selectUserGroup(userGroup, selection, event) {

            // Only allow selection if user is member of the group or admin
            if (currentUser.userGroups.indexOf(userGroup.group.alias) === -1 && currentUser.userGroups.indexOf("admin") === -1) {
                return;
            }
            // Disallow selection of the admin/translators group, the checkbox is not visible in the UI, but clicking(and thus selecting) is still possible.
            // Currently selection can only be used for deleting, and the Controller will also disallow deleting the admin group.
            if (userGroup.group.alias === "admin" || userGroup.group.alias === "translator")
                return;

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
