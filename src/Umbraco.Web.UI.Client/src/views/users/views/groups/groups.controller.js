(function () {
    "use strict";

    function UserGroupsController($scope, $timeout, $location, usersResource) {

        var vm = this;

        vm.userGroups = [];
        vm.selection = [];

        vm.goToUserGroup = goToUserGroup;
        vm.clearSelection = clearSelection;
        vm.selectUserGroup = selectUserGroup;

        function init() {

            vm.loading = true;

            // Get users
            usersResource.getUserGroups().then(function (userGroups) {
                vm.userGroups = userGroups;
            });

            // fake loading
            $timeout(function () {
                vm.loading = false;
            }, 500);

        }

        function selectUserGroup(userGroup, selection) {
            if(userGroup.selected) {
                var index = selection.indexOf(userGroup.id);
                selection.splice(index, 1);
                userGroup.selected = false;
            } else {
                userGroup.selected = true;
                vm.selection.push(userGroup.id);
            }
        }

        function clearSelection() {
            angular.forEach(vm.userGroups, function(userGroup){
                userGroup.selected = false;
            });
            vm.selection = [];
        }

        function goToUserGroup(userGroup) {
            $location.path('users/users/group/' + userGroup.id);
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.GroupsController", UserGroupsController);

})();
