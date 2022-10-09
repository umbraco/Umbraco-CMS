(function () {
    "use strict";

    function ContentRightsController($scope, $timeout, contentResource, localizationService, angularHelper, navigationService, overlayService) {

        var vm = this;
        var currentForm;

        vm.availableUserGroups = [];
        vm.selectedUserGroups = [];
        vm.removedUserGroups = [];
        vm.viewState = "manageGroups";
        vm.labels = {};
        vm.initialState = {};
        vm.setViewSate = setViewSate;
        vm.editPermissions = editPermissions;
        vm.setPermissions = setPermissions;
        vm.save = save;
        vm.removePermissions = removePermissions;
        vm.cancelManagePermissions = cancelManagePermissions;
        vm.closeDialog = closeDialog;
        vm.discardChanges = discardChanges;

        function onInit() {
            vm.loading = true;
            contentResource.getDetailedPermissions($scope.currentNode.id).then(userGroups => {
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
          vm.availableUserGroups.forEach(function (group) {
            if (group.permissions) {
              //if there's explicit permissions assigned than it's selected
              assignGroupPermissions(group);
            }
          });
          vm.initialState = angular.copy(userGroups);
        }

        function resetData() {
            vm.selectedUserGroups = [];
            vm.availableUserGroups = angular.copy(vm.initialState);
            vm.availableUserGroups.forEach(function (group) {
                if (group.permissions) {
                    //if there's explicit permissions assigned than it's selected
                    group.selected = false;
                    assignGroupPermissions(group);
                }
            });
            currentForm = angularHelper.getCurrentForm($scope);

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
          localizationService.localize("defaultdialogs_permissionsSetForGroup", [$scope.currentNode.name, vm.selectedUserGroup.name]).then(value => {
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
          Object.values(group.permissions).forEach(function (permissionGroup) {
            permissionGroup.forEach(function (permission) {
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
            $scope.dialog.confirmDiscardChanges = true;
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
            resetData();
        }

        function formatSaveModel(permissionsSave, selectedUserGroups, removedUserGroups) {
          selectedUserGroups.forEach(function (g) {
            permissionsSave[g.id] = [];
            g.allowedPermissions.forEach(function (p) {
              permissionsSave[g.id].push(p.permissionCode);
            });
          });
          removedUserGroups.forEach(function (g) {
            permissionsSave[g.id] = null;
          });
        }

        function save() {

            vm.saveState = "busy";
            vm.saveError = false;
            vm.saveSuccces = false;

            //this is a dictionary that we need to populate
            var permissionsSave = {};
            //format the selectedUserGroups, then the removedUserGroups since we want to pass data from both collections up
            formatSaveModel(permissionsSave, vm.selectedUserGroups,  vm.removedUserGroups);

            var saveModel = {
                contentId: $scope.currentNode.id,
                permissions: permissionsSave
            };

            contentResource.savePermissions(saveModel).then(userGroups => {

                //re-assign model from server since it could have changed
                initData(userGroups);

                // clear dirty state on the form so we don't see the discard changes notification
                // we use a timeout here because in some cases the initData reformats the userGroups model and triggers a change after the form state was changed
                $timeout(function() {
                  if (currentForm) {
                     currentForm.$dirty = false;
                  }
                });
                
                $scope.dialog.confirmDiscardChanges = false;
                vm.saveState = "success";
                vm.saveSuccces = true;

            }, function(error){
                vm.saveState = "error";
                vm.saveError = error;
            });
        }

        function closeDialog() {
          // check if form has been changed. If it has show discard changes notification
          if (currentForm && currentForm.$dirty) {

            const labelKeys = [
              "prompt_unsavedChanges",
              "prompt_unsavedChangesWarning",
              "prompt_discardChanges",
              "prompt_stay"
            ];

            localizationService.localizeMany(labelKeys).then(values => {
                
                const overlay = {
                    view: "default",
                    title: values[0],
                    content: values[1],
                    disableBackdropClick: true,
                    disableEscKey: true,
                    submitButtonLabel: values[2],
                    closeButtonLabel: values[3],
                    submit: () => {
                        overlayService.close();
                        navigationService.hideDialog();
                    },
                    close: () => overlayService.close()
                };

              overlayService.open(overlay);
            });
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
