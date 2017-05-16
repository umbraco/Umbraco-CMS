(function () {
    "use strict";

    function UserRolesController($scope, $timeout, $location, usersResource) {

        var vm = this;

        vm.userRoles = [];
        vm.selection = [];
        vm.viewState = 'overview';
        
        vm.pagination = {
            "pageNumber": 1,
            "totalPages": 5
        };

        vm.setViewState = setViewState;
        vm.goToUserRole = goToUserRole;
        vm.clearSelection = clearSelection;
        vm.selectUserRole = selectUserRole;
        vm.selectAll = selectAll;
        vm.areAllSelected = areAllSelected;

        function init() {

            vm.loading = true;

            // Get users
            usersResource.getUserGroups().then(function (userRoles) {
                vm.userRoles = userRoles;
            });

            // fake loading
            $timeout(function () {
                vm.loading = false;
            }, 500);

        }

        function setViewState(state) {
            vm.viewState = state;
        }

        function selectUserRole(userRole, selection) {
            if(userRole.selected) {
                var index = selection.indexOf(userRole.id);
                selection.splice(index, 1);
                userRole.selected = false;
            } else {
                userRole.selected = true;
                vm.selection.push(userRole.id);
            }
        }

        function clearSelection() {
            angular.forEach(vm.userRoles, function(userRole){
                userRole.selected = false;
            });
            vm.selection = [];
        }

        function goToUserRole(userRole) {
            $location.path('users/users/role/' + userRole.id);
        }
        function selectAll() {
            if(areAllSelected()) {
                vm.selection = [];
                angular.forEach(vm.userRoles, function(userRole){
                    userRole.selected = false;
                });
            } else {
                // clear selection so we don't add the same user twice
                vm.selection = [];
                // select all users
                angular.forEach(vm.userRoles, function(userRole){
                    userRole.selected = true;
                    vm.selection.push(userRole.id);
                });
            }
        }

        function areAllSelected() {
            if(vm.selection.length === vm.userRoles.length) {
                return true;
            }
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.RolesController", UserRolesController);

})();
