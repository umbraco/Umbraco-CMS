(function () {
    "use strict";

    function UserRolePickerController($scope, usersResource) {
        
        var vm = this;

        vm.userRoles = [];
        vm.loading = false;

        vm.selectUserRole = selectUserRole;

        //////////

        function onInit() {

            vm.loading = true;

            // make sure we can push to something
            if(!$scope.model.selection) {
                $scope.model.selection = [];
            }

            // get venues
            usersResource.getUserGroups().then(function(userRoles){
                vm.userRoles = userRoles;
                
                if($scope.model.selection && $scope.model.selection.length > 0) {
                    preSelect($scope.model.selection);
                }
                
                vm.loading = false;

            });
            
        }

        function preSelect(selection) {

            angular.forEach(selection, function(selected){
                
                angular.forEach(vm.userRoles, function(userRole){
                    if(selected.id === userRole.id) {
                        userRole.selected = true;
                    }
                });

            });
        }

        function selectUserRole(userRole) {

            if(!userRole.selected) {
                
                userRole.selected = true;
                $scope.model.selection.push(userRole);

            } else {

                angular.forEach($scope.model.selection, function(selectedUserRole, index){
                    if(selectedUserRole.id === userRole.id) {
                        userRole.selected = false;
                        $scope.model.selection.splice(index, 1);
                    }
                });

            }

        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.UserRolePickerController", UserRolePickerController);

})();
