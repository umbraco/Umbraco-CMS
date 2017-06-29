(function () {
    "use strict";

    function ContentRightsController($scope, contentResource) {

        var vm = this;

        vm.availableUserGroups = [];
        vm.selectedUserGroups = [];
        vm.selectUserGroup = {};
        vm.viewState = "manageGroups";

        vm.setViewSate = setViewSate;
        vm.editPermissions = editPermissions;
        vm.setPermissions = setPermissions;
        vm.save = save;
        vm.removePermissions = removePermissions;
        vm.cancelManagePermissions = cancelManagePermissions;

        function onInit() {
            vm.loading = true;
            contentResource.getDetailedPermissions($scope.currentNode.id).then(function (userGroups) {
                vm.availableUserGroups = userGroups;
                vm.loading = false;              
            });
        }

        function setViewSate(state) {
            vm.viewState = state;
        }

        function editPermissions(group) {
            vm.selectedUserGroup = group;
            setViewSate("managePermissions");
        }

        function setPermissions(group) {
            // clear allowed permissions before we make the list 
            // so we don't have duplicates
            group.allowedPermissions = [];

            // get list of checked permissions
            angular.forEach(group.permissions, function(permissionGroup) {
                angular.forEach(permissionGroup, function(permission) {
                    if(permission.checked) {
                        group.allowedPermissions.push(permission);
                    }
                });
            });

            if(!group.selected) {
                // set to selected so we can remove from the dropdown easily
                group.selected = true;
                vm.selectedUserGroups.push(group);
            }

            setViewSate("manageGroups");
        }

        function removePermissions(index) {
            // remove as selected so we can select it from the dropdown again
            var group = vm.selectedUserGroups[index];
            group.selected = false;
            vm.selectedUserGroups.splice(index, 1);
        }

        function cancelManagePermissions() {
            setViewSate("manageGroups");
        }

        function save() {

          //this is a dictionary that we need to format
          var permissionsSave = {};
          angular.forEach(vm.selectedUserGroups, function(g) {
            permissionsSave[g.id] = [];
            angular.forEach(g.allowedPermissions, function(p) {
              permissionsSave[g.id].push(p.permissionCode);
            });
          });

          var saveModel = {
            contentId: $scope.currentNode.id,
            permissions: permissionsSave
          };

          contentResource.savePermissions(saveModel).then(function() {
            alert("hooray!");
          });
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.RightsController", ContentRightsController);

})();