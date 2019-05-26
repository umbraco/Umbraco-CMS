/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentBlueprint.RestrictToUserGroupsController
 * @function
 * 
 * @description
 * The controller for assigning user groups to blueprints (conten templates)
 */
function RestrictToUserGroupsController($scope, navigationService, contentResource, editorService) {

    var vm = this;

    vm.removeSelectedItem = removeSelectedItem;
    vm.openUserGroupPicker = openUserGroupPicker;
    vm.saveChanges = saveChanges;

    function onInit() {

        vm.loading = true;
        contentResource.getGroupsAssignedToBlueprintById($scope.currentNode.id).then(function (userGroups) {
            vm.userGroups = userGroups;
            vm.loading = false;
        });

    }

    function removeSelectedItem(index, selection) {
        if (selection && selection.length > 0) {
            selection.splice(index, 1);
        }
    }

    function openUserGroupPicker() {
        var oldSelection = angular.copy(vm.userGroups);
        var userGroupPicker = {
            selection: vm.userGroups,
            submit: function (model) {
                // apply changes
                if (model.selection) {
                    vm.userGroups = model.selection;
                }
                editorService.close();
            },
            close: function () {
                // roll back the selection
                vm.userGroups = oldSelection;
                editorService.close();
            }
        };
        editorService.userGroupPicker(userGroupPicker);
    }

    function saveChanges() {

        vm.submitButtonState = "busy";

        var userGroupIds = vm.userGroups.map(a => a.id);
        contentResource.assignGroupsToBlueprintById($scope.currentNode.id, userGroupIds).then(function () {
            vm.submitButtonState = "success";
            navigationService.hideDialog();
        });
    }

    $scope.cancel = function () {
        navigationService.hideDialog();        
    };

    onInit();
}

angular.module("umbraco").controller("Umbraco.Editors.ContentBlueprint.RestrictToUserGroupsController", RestrictToUserGroupsController);
