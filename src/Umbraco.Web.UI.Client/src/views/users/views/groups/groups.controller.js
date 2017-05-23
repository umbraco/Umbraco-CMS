(function () {
    "use strict";

    function UserGroupsController($scope, $timeout, $location, usersResource) {

        var vm = this;

        vm.userGroups = [];
        vm.selection = [];
        vm.viewState = 'overview';
        
        vm.pagination = {
            "pageNumber": 1,
            "totalPages": 5
        };

        vm.setViewState = setViewState;
        vm.goToUserGroup = goToUserGroup;
        vm.clearSelection = clearSelection;
        vm.selectUserGroup = selectUserGroup;
        vm.selectAll = selectAll;
        vm.areAllSelected = areAllSelected;

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

        function setViewState(state) {
            vm.viewState = state;
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
        function selectAll() {
            if(areAllSelected()) {
                vm.selection = [];
                angular.forEach(vm.userGroups, function(userGroup){
                    userGroup.selected = false;
                });
            } else {
                // clear selection so we don't add the same user twice
                vm.selection = [];
                // select all users
                angular.forEach(vm.userGroups, function(userGroup){
                    userGroup.selected = true;
                    vm.selection.push(userGroup.id);
                });
            }
        }

        function areAllSelected() {
            if(vm.selection.length === vm.userGroups.length) {
                return true;
            }
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.GroupsController", UserGroupsController);

})();
