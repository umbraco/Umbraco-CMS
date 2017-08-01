(function () {
    "use strict";

    function UserEditController($scope, $timeout, $location, $routeParams, formHelper, usersResource, contentEditingHelper, localizationService, notificationsService, mediaHelper, Upload, umbRequestHelper, usersHelper, authResource) {

        var vm = this;

        vm.page = {};
        vm.page.rootIcon = "icon-folder";
        vm.user = {
          changePassword: null
        };
        vm.breadcrumbs = [];
        vm.avatarFile = {};
        vm.labels = {};

        vm.goToPage = goToPage;
        vm.openUserGroupPicker = openUserGroupPicker;
        vm.openContentPicker = openContentPicker;
        vm.openMediaPicker = openMediaPicker;
        vm.removeSelectedItem = removeSelectedItem;
        vm.disableUser = disableUser;
        vm.enableUser = enableUser;
        vm.clearAvatar = clearAvatar;
        vm.save = save;

        function init() {

            vm.loading = true;

            localizationService.localize("user_noStartNode").then(function (name) {
                vm.page.noStartNodeLabel = name;
            });

            var labelKeys = [
                "general_saving",
                "general_cancel",
                "defaultdialogs_selectContentStartNode",
                "defaultdialogs_selectMediaStartNode",
                "sections_users",
                "content_contentRoot",
                "media_mediaRoot"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                vm.labels.saving = values[0];
                vm.labels.cancel = values[1];
                vm.labels.selectContentStartNode = values[2];
                vm.labels.selectMediaStartNode = values[3];
                vm.labels.users = values[4];
                vm.labels.contentRoot = values[5];
                vm.labels.mediaRoot = values[6];
            });

            // get user
            usersResource.getUser($routeParams.id).then(function (user) {
                vm.user = user;
                makeBreadcrumbs(vm.user);
                setUserDisplayState();

                vm.emailIsUsername = user.email === user.username;

                //go get the config for the membership provider and add it to the model
                authResource.getMembershipProviderConfig().then(function (data) {
                  vm.changePasswordModel.config = data;

                  //the user has a password if they are not states: Invited, NoCredentials
                  vm.changePasswordModel.config.hasPassword = vm.user.userState !== 3 && vm.user.userState !== 4;

                  vm.changePasswordModel.config.disableToggle = true;
                  
                  vm.loading = false;
                });
            });
        }

        function toggleChangePassword() {
          vm.changePasswordModel.isChanging = !vm.changePasswordModel.isChanging;
          //reset it
          vm.user.changePassword = null;
        }

        function save() {

            vm.page.saveButtonState = "busy";
            vm.user.resetPasswordValue = null;

            contentEditingHelper.contentEditorPerformSave({
                statusMessage: vm.labels.saving,
                saveMethod: usersResource.saveUser,
                scope: $scope,
                content: vm.user,
                // We do not redirect on failure for users - this is because it is not possible to actually save a user
                // when server side validation fails - as opposed to content where we are capable of saving the content
                // item if server side validation fails
                redirectOnFailure: false,
                rebindCallback: function (orignal, saved) { }
            }).then(function (saved) {

                vm.user = saved;
                setUserDisplayState();

                vm.changePasswordModel.isChanging = false;
                vm.page.saveButtonState = "success";

                //the user has a password if they are not states: Invited, NoCredentials
                vm.changePasswordModel.config.hasPassword = vm.user.userState !== 3 && vm.user.userState !== 4;
            }, function (err) {
                vm.page.saveButtonState = "error";
            });
        }

        function goToPage(ancestor) {
            $location.path(ancestor.path).search("subview", ancestor.subView);
        }

        function openUserGroupPicker() {
            vm.userGroupPicker = {
                view: "usergrouppicker",
                selection: vm.user.userGroups,
                closeButtonLabel: vm.labels.cancel,
                show: true,
                submit: function (model) {
                    // apply changes
                    if (model.selection) {
                        vm.user.userGroups = model.selection;
                    }
                    vm.userGroupPicker.show = false;
                    vm.userGroupPicker = null;
                },
                close: function (oldModel) {
                    // rollback on close
                    if (oldModel.selection) {
                        vm.user.userGroups = oldModel.selection;
                    }
                    vm.userGroupPicker.show = false;
                    vm.userGroupPicker = null;
                }
            };
        }

        function openContentPicker() {
            vm.contentPicker = {
                title: vm.labels.selectContentStartNode,
                view: "contentpicker",
                multiPicker: true,
                selection: vm.user.startContentIds,
                hideHeader: false,
                show: true,
                submit: function (model) {
                    // select items
                    if (model.selection) {
                        angular.forEach(model.selection, function (item) {
                            if (item.id === "-1") {
                                item.name = vm.labels.contentRoot;
                                item.icon = "icon-folder";
                            }
                            multiSelectItem(item, vm.user.startContentIds);
                        });
                    }
                    // close overlay
                    vm.contentPicker.show = false;
                    vm.contentPicker = null;
                },
                close: function (oldModel) {
                    // close overlay
                    vm.contentPicker.show = false;
                    vm.contentPicker = null;
                }
            };
        }

        function openMediaPicker() {
            vm.mediaPicker = {
                title: vm.labels.selectMediaStartNode,
                view: "treepicker",
                section: "media",
                treeAlias: "media",
                entityType: "media",
                multiPicker: true,
                hideHeader: false,
                show: true,
                submit: function (model) {
                    // select items
                    if (model.selection) {
                        angular.forEach(model.selection, function (item) {
                            if (item.id === "-1") {
                                item.name = vm.labels.mediaRoot;
                                item.icon = "icon-folder";
                            }
                            multiSelectItem(item, vm.user.startMediaIds);
                        });
                    }
                    // close overlay
                    vm.mediaPicker.show = false;
                    vm.mediaPicker = null;
                },
                close: function (oldModel) {
                    // close overlay
                    vm.mediaPicker.show = false;
                    vm.mediaPicker = null;
                }
            };
        }

        function multiSelectItem(item, selection) {
            var found = false;
            // check if item is already in the selected list
            if (selection.length > 0) {
                angular.forEach(selection, function (selectedItem) {
                    if (selectedItem.udi === item.udi) {
                        found = true;
                    }
                });
            }
            // only add the selected item if it is not already selected
            if (!found) {
                selection.push(item);
            }
        }

        function removeSelectedItem(index, selection) {
            selection.splice(index, 1);
        }

        function disableUser() {
            vm.disableUserButtonState = "busy";
            usersResource.disableUsers([vm.user.id]).then(function (data) {
              vm.user.userState = 1;
              setUserDisplayState();
              vm.disableUserButtonState = "success";
              formHelper.showNotifications(data);
            }, function(error){
                vm.disableUserButtonState = "error";
                formHelper.showNotifications(error.data);
            });
        }

        function enableUser() {
            vm.enableUserButtonState = "busy";
            usersResource.enableUsers([vm.user.id]).then(function (data) {
              vm.user.userState = 0;
              setUserDisplayState();
              vm.enableUserButtonState = "success";
              formHelper.showNotifications(data);
            }, function(error){
                vm.disableUserButtonState = "error";
                formHelper.showNotifications(error.data);
            });
        }
      

        function clearAvatar() {
            // get user
            usersResource.clearAvatar(vm.user.id).then(function (data) {
              vm.user.avatars = data;
            });
        }

        $scope.changeAvatar = function (files, event) {
            if (files && files.length > 0) {
                upload(files[0]);
            }
        };

        function upload(file) {

            vm.avatarFile.uploadProgress = 0;

            Upload.upload({
                url: umbRequestHelper.getApiUrl("userApiBaseUrl", "PostSetAvatar", { id: vm.user.id }),
                fields: {},
                file: file
            }).progress(function (evt) {

                if (vm.avatarFile.uploadStatus !== "done" && vm.avatarFile.uploadStatus !== "error") {
                  // set uploading status on file
                  vm.avatarFile.uploadStatus = "uploading";

                  // calculate progress in percentage
                  var progressPercentage = parseInt(100.0 * evt.loaded / evt.total, 10);

                  // set percentage property on file
                  vm.avatarFile.uploadProgress = progressPercentage;
                }               

            }).success(function (data, status, headers, config) {

                // set done status on file
                vm.avatarFile.uploadStatus = "done";
                vm.avatarFile.uploadProgress = 100;
                vm.user.avatars = data;

            }).error(function (evt, status, headers, config) {

                // set status done
                vm.avatarFile.uploadStatus = "error";

                // If file not found, server will return a 404 and display this message
                if (status === 404) {
                    vm.avatarFile.serverErrorMessage = "File not found";
                }
                else if (status == 400) {
                    //it's a validation error
                    vm.avatarFile.serverErrorMessage = evt.message;
                }
                else {
                    //it's an unhandled error
                    //if the service returns a detailed error
                    if (evt.InnerException) {
                        vm.avatarFile.serverErrorMessage = evt.InnerException.ExceptionMessage;

                        //Check if its the common "too large file" exception
                        if (evt.InnerException.StackTrace && evt.InnerException.StackTrace.indexOf("ValidateRequestEntityLength") > 0) {
                            vm.avatarFile.serverErrorMessage = "File too large to upload";
                        }

                    } else if (evt.Message) {
                        vm.avatarFile.serverErrorMessage = evt.Message;
                    }
                }
            });
        }

        function makeBreadcrumbs() {
            vm.breadcrumbs = [
                {
                    "name": vm.labels.users,
                    "path": "/users/users/overview",
                    "subView": "users"
                },
                {
                    "name": vm.user.name
                }
            ];
        }

        function setUserDisplayState() {
            vm.user.userDisplayState = usersHelper.getUserStateFromValue(vm.user.userState);
        }

        init();
    }
    angular.module("umbraco").controller("Umbraco.Editors.Users.UserController", UserEditController);
})();
