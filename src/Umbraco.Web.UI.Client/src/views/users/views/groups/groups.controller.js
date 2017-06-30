(function () {
    "use strict";

    function UserGroupsController($scope, $timeout, $location, userGroupsResource) {

        var vm = this;

        vm.userGroups = [];
        vm.selection = [];

        vm.createUserGroup = createUserGroup;
        vm.clickUserGroup = clickUserGroup;
        vm.clearSelection = clearSelection;
        vm.selectUserGroup = selectUserGroup;

        function onInit() {

            vm.loading = true;

            // Get users
            userGroupsResource.getUserGroups().then(function (userGroups) {
                vm.userGroups = userGroups;
                vm.loading = false;
            });

        }

        function createUserGroup() {
            // clear all query params
            $location.search({});
            // go to create user group
            $location.path('users/users/group/-1').search("create", "true");;
        }

        function clickUserGroup(userGroup) {
            if (vm.selection.length > 0) {
                selectUserGroup(userGroup, vm.selection);
            } else {
                goToUserGroup(userGroup.id);
            }
        }

        function selectUserGroup(userGroup, selection, event) {

            if (userGroup.selected) {
                var index = selection.indexOf(userGroup.id);
                selection.splice(index, 1);
                userGroup.selected = false;
            } else {
                userGroup.selected = true;
                vm.selection.push(userGroup.id);
            }

            if(event){
                event.preventDefault();
                event.stopPropagation();
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
