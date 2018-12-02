(function () {
    "use strict";

    function ContentRightsController($scope, $timeout, contentResource, localizationService, angularHelper, navigationService) {

        var vm = this;
        var currentForm;

        vm.availableUserGroups = [];
        vm.selectedUserGroups = [];
        vm.removedUserGroups = [];
        vm.viewState = "manageGroups";
        vm.labels = {};
        vm.showNotification = false;
        
        vm.setViewSate = setViewSate;
        vm.editPermissions = editPermissions;
        vm.setPermissions = setPermissions;
        vm.save = save;
        vm.removePermissions = removePermissions;
        vm.cancelManagePermissions = cancelManagePermissions;
        vm.closeDialog = closeDialog;
        vm.stay = stay;
        vm.discardChanges = discardChanges;

        function onInit() {
            vm.loading = true;
            contentResource.getDetailedPermissions($scope.currentNode.id).then(function (userGroups) {
                initData(userGroups);                
                vm.loading = false;
                currentForm = angularHelper.getCurrentForm($scope);
            });

        }

        /**
        * This will initialize the data and set the correct selectedUserGroups based on the default permissions and explicit permissions assigned
        * @param {any} userGroups
        */
        function initData(userGroups) {
          //reset this
          vm.selectedUserGroups = [];
          vm.availableUserGroups = userGroups;
          angular.forEach(vm.availableUserGroups, function (group) {
            if (group.permissions) {
              //if there's explicit permissions assigned than it's selected
              assignGroupPermissions(group);
            }
          });
        }

        function setViewSate(state) {
            vm.viewState = state;
        }

        function editPermissions(group) {
          vm.selectedUserGroup = group;
          if (!vm.selectedUserGroup.permissions) {
            //if no permissions are explicitly set this means we need to show the defaults
            vm.selectedUserGroup.permissions = vm.selectedUserGroup.defaultPermissions;
          }
          localizationService.localize("defaultdialogs_permissionsSetForGroup", [$scope.currentNode.name, vm.selectedUserGroup.name]).then(function (value) {
            vm.labels.permissionsSetForGroup = value;
          });
          setViewSate("managePermissions");
          // hide dropdown
          vm.groupsDropdownOpen = false;
        }

        function assignGroupPermissions(group) {
          // clear allowed permissions before we make the list so we don't have duplicates
          group.allowedPermissions = [];

          // get list of checked permissions
          angular.forEach(group.permissions, function (permissionGroup) {
            angular.forEach(permissionGroup, function (permission) {
              if (permission.checked) {
                //the `allowedPermissions` is what will get sent up to the server for saving
                group.allowedPermissions.push(permission);
              }
            });
          });

          if (!group.selected) {
            // set to selected so we can remove from the dropdown easily
            group.selected = true;
            vm.selectedUserGroups.push(group);
            //remove from the removed groups if it's been re-added
            vm.removedUserGroups = _.reject(vm.removedUserGroups, function (g) {
              return g.id == group.id;
            });
          }
        }

        function setPermissions(group) {
            assignGroupPermissions(group);  
            setViewSate("manageGroups");
        }

        /**
         * This essentially resets the permissions for a group for this content item, it will remove it from the selected list
         * @param {any} index
         */
        function removePermissions(index) {
            // remove as selected so we can select it from the dropdown again
            var group = vm.selectedUserGroups[index];
            group.selected = false;
            //reset assigned permissions - so it will default back to default permissions
            group.permissions = [];
            group.allowedPermissions = [];
            vm.selectedUserGroups.splice(index, 1);
            //track it in the removed so this gets pushed to the server
            vm.removedUserGroups.push(group);
        }

        function cancelManagePermissions() {
            setViewSate("manageGroups");
        }

        function formatSaveModel(permissionsSave, groupCollection) {
          angular.forEach(groupCollection, function (g) {
            permissionsSave[g.id] = [];
            angular.forEach(g.allowedPermissions, function (p) {
              permissionsSave[g.id].push(p.permissionCode);
            });
          });
        }

        function save() {

            vm.saveState = "busy";
            vm.saveError = false;
            vm.saveSuccces = false;

            //this is a dictionary that we need to populate
            var permissionsSave = {};
            //format the selectedUserGroups, then the removedUserGroups since we want to pass data from both collections up
            formatSaveModel(permissionsSave, vm.selectedUserGroups);
            formatSaveModel(permissionsSave, vm.removedUserGroups);

            var saveModel = {
                contentId: $scope.currentNode.id,
                permissions: permissionsSave
            };

            contentResource.savePermissions(saveModel).then(function (userGroups) {

                //re-assign model from server since it could have changed
                initData(userGroups);

                // clear dirty state on the form so we don't see the discard changes notification
                // we use a timeout here because in some cases the initData reformats the userGroups model and triggers a change after the form state was changed
                $timeout(function() {
                  if(currentForm) {
                    currentForm.$dirty = false;
                  }
                });

                vm.saveState = "success";
                vm.saveSuccces = true;
            }, function(error){
                vm.saveState = "error";
                vm.saveError = error;
            });
        }

        function stay() {
          vm.showNotification = false;
        }

        function closeDialog() {
          // check if form has been changed. If it has show discard changes notification
          if (currentForm && currentForm.$dirty) {
            vm.showNotification = true;
          } else {
            navigationService.hideDialog();
          }
        }

        function discardChanges() {
          navigationService.hideDialog();
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.RightsController", ContentRightsController);

})();