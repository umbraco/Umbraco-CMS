(function () {
    "use strict";

    function UserEditController($scope, $timeout, $location, $routeParams, usersResource, contentEditingHelper, localizationService, notificationsService) {

        var vm = this;
        var localizeSaving = localizationService.localize("general_saving");

        vm.page = {};
        vm.user = {};
        vm.breadcrumbs = [];

        vm.goToPage = goToPage;
        vm.openUserGroupPicker = openUserGroupPicker;
        vm.openContentPicker = openContentPicker;
        vm.openMediaPicker = openMediaPicker;
        vm.removeSelectedItem = removeSelectedItem;
        vm.disableUser = disableUser;
        vm.resetPassword = resetPassword;
        vm.getUserStateType = getUserStateType;
        vm.changeAvatar = changeAvatar;
        vm.save = save;

        function init() {

            vm.loading = true;

            // get user
            usersResource.getUser($routeParams.id).then(function (user) {
                vm.user = user;
                makeBreadcrumbs(vm.user);
                vm.loading = false;
            });
            
        }

        function save() {

            vm.page.saveButtonState = "busy";

            contentEditingHelper.contentEditorPerformSave({
                statusMessage: localizeSaving,
                saveMethod: usersResource.saveUser,
                scope: $scope,
                content: vm.user,
                // We do not redirect on failure for users - this is because it is not possible to actually save a user
                // when server side validation fails - as opposed to content where we are capable of saving the content
                // item if server side validation fails
                redirectOnFailure: false,
                rebindCallback: function (orignal, saved) {}
            }).then(function (saved) {

                vm.user = saved;
                vm.page.saveButtonState = "success";

            }, function (err) {

                vm.page.saveButtonState = "error";

            });

        }

        function goToPage(ancestor) {
            $location.path(ancestor.path).search("subview", ancestor.subView);
        }

        function openUserGroupPicker() {
            vm.userGroupPicker = {
                title: "Select user groups",
                view: "usergrouppicker",
                selection: vm.user.userGroups,
                closeButtonLabel: "Cancel",
                show: true,
                submit: function(model) {
                    // apply changes
                    if(model.selection) {
                        vm.user.userGroups = model.selection;
                    }
                    vm.userGroupPicker.show = false;
                    vm.userGroupPicker = null;
                },
                close: function(oldModel) {
                    // rollback on close
                    if(oldModel.selection) {
                        vm.user.userGroups = oldModel.selection;
                    }
                    vm.userGroupPicker.show = false;
                    vm.userGroupPicker = null;
                }
            };
        }

        function openContentPicker() {
            vm.contentPicker = {
                title: "Select content start node",
                view: "contentpicker",
                multiPicker: true,
                show: true,
                submit: function(model) {
                    if(model.selection) {
                        vm.user.startNodesContent = model.selection;
                    }
                    vm.contentPicker.show = false;
                    vm.contentPicker = null;
                },
                close: function(oldModel) {
                    vm.contentPicker.show = false;
                    vm.contentPicker = null;
                }
            };
        }

        function openMediaPicker() {
            vm.contentPicker = {
                title: "Select media start node",
                view: "treepicker",
                section: "media",
                treeAlias: "media",
                entityType: "media",
                multiPicker: true,
                show: true,
                submit: function(model) {
                    if(model.selection) {
                        vm.user.startNodesMedia = model.selection;
                    }
                    vm.contentPicker.show = false;
                    vm.contentPicker = null;
                },
                close: function(oldModel) {
                    vm.contentPicker.show = false;
                    vm.contentPicker = null;
                }
            };
        }

        function removeSelectedItem(index, selection) {
            selection.splice(index, 1);
        }

        function disableUser() {
            alert("disable user");
        }

        function resetPassword() {
            alert("reset password");
        }

        function getUserStateType(state) {
            switch (state) {
                case "disabled" || "umbracoDisabled":
                    return "danger";
                case "pending":
                    return "warning";
                default:
                    return "success";
            }
        }

        function changeAvatar() {
            alert("change avatar");
        }

        function makeBreadcrumbs() {
            vm.breadcrumbs = [
                {
                    "name": "Users",
                    "path": "/users/users/overview",
                    "subView": "users"
                },
                {
                    "name": vm.user.name
                }
            ];
        }
 
        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Users.UserController", UserEditController);

})();
