(function () {
    "use strict";

    function UserEditController($scope, eventsService, $q, $location, $routeParams, formHelper, usersResource, twoFactorLoginResource,
        userService, contentEditingHelper, localizationService, mediaHelper, Upload, umbRequestHelper,
        usersHelper, authResource, dateHelper, editorService, overlayService, externalLoginInfoService) {

        var currentLoggedInUser = null;

        var vm = this;

        vm.page = {};
        vm.page.rootIcon = "icon-folder";
        vm.user = {
            changePassword: null
        };
        vm.breadcrumbs = [];
        vm.showBackButton = true;
        vm.hasTwoFactorProviders = false;
        vm.avatarFile = {};
        vm.labels = {};
        vm.maxFileSize = Umbraco.Sys.ServerVariables.umbracoSettings.maxFileSize + "KB";
        vm.acceptedFileTypes = mediaHelper.formatFileTypes(Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes);
        vm.usernameIsEmail = Umbraco.Sys.ServerVariables.umbracoSettings.usernameIsEmail;

        //create the initial model for change password
        vm.changePasswordModel = {
            config: {},
            isChanging: false,
            value: {}
        };

        vm.goToPage = goToPage;
        vm.openUserGroupPicker = openUserGroupPicker;
        vm.openContentPicker = openContentPicker;
        vm.openMediaPicker = openMediaPicker;
        vm.editSelectedItem = editSelectedItem;
        vm.removeSelectedItem = removeSelectedItem;
        vm.disableUser = disableUser;
        vm.enableUser = enableUser;
        vm.unlockUser = unlockUser;
        vm.toggleConfigureTwoFactor = toggleConfigureTwoFactor;
        vm.resendInvite = resendInvite;
        vm.deleteNonLoggedInUser = deleteNonLoggedInUser;
        vm.changeAvatar = changeAvatar;
        vm.clearAvatar = clearAvatar;
        vm.save = save;
        vm.allowGroupEdit = allowGroupEdit;

        vm.changePassword = changePassword;
        vm.toggleChangePassword = toggleChangePassword;

        vm.denyLocalLogin = externalLoginInfoService.hasDenyLocalLogin();

        function init() {

            vm.loading = true;

            var labelKeys = [
                "general_saving",
                "general_cancel",
                "defaultdialogs_selectContentStartNode",
                "defaultdialogs_selectMediaStartNode",
                "sections_users",
                "content_contentRoot",
                "media_mediaRoot",
                "user_noStartNodes",
                "user_defaultInvitationMessage",
                "user_deleteUserConfirmation"
            ];

            localizationService.localizeMany(labelKeys).then(function (values) {
                vm.labels.saving = values[0];
                vm.labels.cancel = values[1];
                vm.labels.selectContentStartNode = values[2];
                vm.labels.selectMediaStartNode = values[3];
                vm.labels.users = values[4];
                vm.labels.contentRoot = values[5];
                vm.labels.mediaRoot = values[6];
                vm.labels.noStartNodes = values[7];
                vm.labels.defaultInvitationMessage = values[8];
                vm.labels.deleteUserConfirmation = values[9];
            });

            // get user
            usersResource.getUser($routeParams.id).then(function (user) {
                vm.user = user;
                makeBreadcrumbs(vm.user);
                setUserDisplayState();
                formatDatesToLocal(vm.user);

                vm.usernameIsEmail = Umbraco.Sys.ServerVariables.umbracoSettings.usernameIsEmail && user.email === user.username;

                //go get the config for the membership provider and add it to the model
                authResource.getPasswordConfig(user.id).then(function (data) {
                  vm.changePasswordModel.config = data;

                    //the user has a password if they are not states: Invited, NoCredentials
                    vm.changePasswordModel.config.hasPassword = vm.user.userState !== "Invited" && vm.user.userState !== "Inactive";

                    vm.changePasswordModel.config.disableToggle = true;

                    $scope.$emit("$setAccessibleHeader", false, "general_user", false, vm.user.name, "", true);
                    vm.loading = false;
                });

              twoFactorLoginResource.get2FAProvidersForUser(vm.user.id).then(function (providers) {
                vm.hasTwoFactorProviders = providers.length > 0;
              });
            });
        }

        function getLocalDate(date, culture, format) {
            if (date) {
                var dateVal;
                var serverOffset = Umbraco.Sys.ServerVariables.application.serverTimeOffset;
                var localOffset = new Date().getTimezoneOffset();
                var serverTimeNeedsOffsetting = (-serverOffset !== localOffset);

                if (serverTimeNeedsOffsetting) {
                    dateVal = dateHelper.convertToLocalMomentTime(date, serverOffset);
                } else {
                    dateVal = moment(date, "YYYY-MM-DD HH:mm:ss");
                }

                return dateVal.locale(culture).format(format);
            }
        }

        function toggleChangePassword() {
            //reset it
            vm.user.changePassword = null;

            localizationService.localizeMany(["general_cancel", "general_confirm", "general_changePassword"])
                .then(function (data) {
                    const overlay = {
                        view: "changepassword",
                        title: data[2],
                        changePassword: vm.user.changePassword,
                        config: vm.changePasswordModel.config,
                        closeButtonLabel: data[0],
                        submitButtonLabel: data[1],
                        submitButtonStyle: 'success',
                        close: () => overlayService.close(),
                        submit: model => {
                            overlayService.close();
                            vm.changePasswordModel.value = model.changePassword;
                            changePassword();
                        }
                    };
                    overlayService.open(overlay);
                });
        }

        function save() {

            if (formHelper.submitForm({ scope: $scope })) {

                vm.page.saveButtonState = "busy";

                //save current nav to be restored later so that the tabs dont change
                var currentNav = vm.user.navigation;

                usersResource.saveUser(vm.user)
                    .then(function (saved) {

                        //if the user saved, then try to execute all extended save options
                        extendedSave(saved).then(function (result) {
                            //if all is good, then reset the form
                            formHelper.resetForm({ scope: $scope });
                        }, function () {
                            formHelper.resetForm({ scope: $scope, hasErrors: true });
                        });

                        vm.user = _.omit(saved, "navigation");
                        //restore
                        vm.user.navigation = currentNav;
                        setUserDisplayState();
                        formatDatesToLocal(vm.user);

                        vm.page.saveButtonState = "success";

                    }, function (err) {
                        formHelper.resetForm({ scope: $scope, hasErrors: true });
                        contentEditingHelper.handleSaveError({
                            err: err,
                            showNotifications: true
                        });

                        vm.page.saveButtonState = "error";
                    });
            }
        }

        /**
         *
         */
        function changePassword() {
            //anytime a user is changing another user's password, we are in effect resetting it so we need to set that flag here
            if (vm.changePasswordModel.value) {
                //NOTE: the check for allowManuallyChangingPassword is due to this legacy user membership provider setting, if that is true, then the current user
                //can change their own password without entering their current one (this is a legacy setting since that is a security issue but we need to maintain compat).
                //if allowManuallyChangingPassword=false, then we are using default settings and the user will need to enter their old password to change their own password.
                vm.changePasswordModel.value.reset = (!vm.changePasswordModel.value.oldPassword && !vm.user.isCurrentUser) || vm.changePasswordModel.config.allowManuallyChangingPassword;
            }

            // since we don't send the entire user model, the id is required
            vm.changePasswordModel.value.id = vm.user.id;

            usersResource.changePassword(vm.changePasswordModel.value)
                .then(() => {
                    vm.changePasswordModel.isChanging = false;
                    vm.changePasswordModel.value = {};

                    //the user has a password if they are not states: Invited, NoCredentials
                    vm.changePasswordModel.config.hasPassword = vm.user.userState !== "Invited" && vm.user.userState !== "Inactive";
                }, err => {
                    contentEditingHelper.handleSaveError({
                        err: err,
                        showNotifications: true
                    });
                });
        }

        /**
         * Used to emit the save event and await any async operations being performed by editor extensions
         * @param {any} savedUser
         */
        function extendedSave(savedUser) {

            //used to track any promises added by the event handlers to be awaited
            var promises = [];

            var args = {
                //getPromise: getPromise,
                user: savedUser,
                //a promise can be added by the event handler if the handler needs an async operation to be awaited
                addPromise: function (p) {
                    promises.push(p);
                }
            };

            //emit the event
            eventsService.emit("editors.user.editController.save", args);

            //await all promises to complete
            var resultPromise = $q.all(promises);

            return resultPromise;
        }

        function goToPage(ancestor) {
            $location.path(ancestor.path);
        }

        function openUserGroupPicker() {
            var currentSelection = [];
            Utilities.copy(vm.user.userGroups, currentSelection);
            var userGroupPicker = {
                selection: currentSelection,
                submit: function (model) {
                    // apply changes
                    if (model.selection) {
                        vm.user.userGroups = model.selection;
                    }
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.userGroupPicker(userGroupPicker);
        }

        function openContentPicker() {
            var contentPicker = {
                title: vm.labels.selectContentStartNode,
                section: "content",
                treeAlias: "content",
                multiPicker: true,
                selection: vm.user.startContentIds,
                hideHeader: false,
                submit: function (model) {
                    // select items
                    if (model.selection) {
                        model.selection.forEach(function (item) {
                            if (item.id === "-1") {
                                item.name = vm.labels.contentRoot;
                                item.icon = "icon-folder";
                            }
                            multiSelectItem(item, vm.user.startContentIds);
                        });
                    }
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.treePicker(contentPicker);
        }

        function openMediaPicker() {
            var mediaPicker = {
                title: vm.labels.selectMediaStartNode,
                section: "media",
                treeAlias: "media",
                entityType: "media",
                multiPicker: true,
                hideHeader: false,
                show: true,
                submit: function (model) {
                    // select items
                    if (model.selection) {
                        model.selection.forEach(function (item) {
                            if (item.id === "-1") {
                                item.name = vm.labels.mediaRoot;
                                item.icon = "icon-folder";
                            }
                            multiSelectItem(item, vm.user.startMediaIds);
                        });
                    }
                    // close overlay
                    editorService.close();
                },
                close: function () {
                    // close overlay
                    editorService.close();
                }
            };
            editorService.treePicker(mediaPicker);
        }

        function multiSelectItem(item, selection) {
            var found = false;
            // check if item is already in the selected list
            if (selection.length > 0) {
                selection.forEach(function (selectedItem) {
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

        function editSelectedItem(index, selection) {
            var group = selection[index];
            const editor = {
                id: group.id,
                submit: function (model) {
                    selection[index] = model;
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.userGroupEditor(editor);
        }

        function removeSelectedItem(index, selection) {
            selection.splice(index, 1);
        }

        function disableUser() {
            vm.disableUserButtonState = "busy";
            usersResource.disableUsers([vm.user.id]).then(function (data) {
                vm.user.userState = "Disabled";
                setUserDisplayState();
                vm.disableUserButtonState = "success";

            }, function (error) {
                vm.disableUserButtonState = "error";

            });
        }

        function enableUser() {
            vm.enableUserButtonState = "busy";
            usersResource.enableUsers([vm.user.id]).then(function (data) {
                vm.user.userState = "Active";
                setUserDisplayState();
                vm.enableUserButtonState = "success";
            }, function (error) {
                vm.enableUserButtonState = "error";
            });
        }

        function unlockUser() {
            vm.unlockUserButtonState = "busy";
            usersResource.unlockUsers([vm.user.id]).then(function (data) {
                vm.user.userState = "Active";
                vm.user.failedPasswordAttempts = 0;
                setUserDisplayState();
                vm.unlockUserButtonState = "success";

            }, function (error) {
                vm.unlockUserButtonState = "error";
            });
        }

        function toggleConfigureTwoFactor() {

          var configureTwoFactorSettings = {
            create: true,
            user: vm.user,
            isCurrentUser:  vm.user.isCurrentUser,
            size: "small",
            view: "views/common/infiniteeditors/twofactor/configuretwofactor.html",
            close: function() {
              editorService.close();
            }
          };

          editorService.open(configureTwoFactorSettings);
        }


        function resendInvite() {
            vm.resendInviteButtonState = "busy";

            if (vm.resendInviteMessage) {
                vm.user.message = vm.resendInviteMessage;
            }
            else {
                vm.user.message = vm.labels.defaultInvitationMessage;
            }

            usersResource.inviteUser(vm.user).then(function (data) {
                vm.resendInviteButtonState = "success";
                vm.resendInviteMessage = "";
                formHelper.showNotifications(data);
            }, function (error) {
                vm.resendInviteButtonState = "error";
                formHelper.showNotifications(error.data);
            });
        }

        function deleteNonLoggedInUser() {
            vm.deleteNotLoggedInUserButtonState = "busy";

            var confirmationMessage = vm.labels.deleteUserConfirmation;

            localizationService.localizeMany(["general_delete", "general_cancel", "contentTypeEditor_yesDelete"])
                .then(function (data) {

                    const overlay = {
                        view: "confirm",
                        title: data[0],
                        content: confirmationMessage,
                        closeButtonLabel: data[1],
                        submitButtonLabel: data[2],
                        submitButtonStyle: "danger",
                        close: function () {
                            vm.deleteNotLoggedInUserButtonState = "danger";
                            overlayService.close();
                        },
                        submit: function () {
                            performDelete();
                            overlayService.close();
                        }
                    };
                    overlayService.open(overlay);

                });
        }

        function performDelete() {
            usersResource.deleteNonLoggedInUser(vm.user.id).then(function (data) {
               goToPage(vm.breadcrumbs[0]);
            }, function (error) {
                vm.deleteNotLoggedInUserButtonState = "error";
                formHelper.showNotifications(error.data);
            });
        }

        function clearAvatar() {
            // get user
            usersResource.clearAvatar(vm.user.id).then(function (data) {
                vm.user.avatars = data;
            });
        }

        function changeAvatar(files, event) {
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
                    "path": "/users/users/users"
                },
                {
                    "name": vm.user.name
                }
            ];
        }

        function setUserDisplayState() {
            vm.user.userDisplayState = usersHelper.getUserStateByKey(vm.user.userState);
        }

        function formatDatesToLocal(user) {
            // get current backoffice user and format dates
            userService.getCurrentUser().then(function (currentUser) {
                currentLoggedInUser = currentUser;

                user.formattedLastLogin = getLocalDate(user.lastLoginDate, currentUser.locale, "LLL");
                user.formattedLastLockoutDate = getLocalDate(user.lastLockoutDate, currentUser.locale, "LLL");
                user.formattedCreateDate = getLocalDate(user.createDate, currentUser.locale, "LLL");
                user.formattedUpdateDate = getLocalDate(user.updateDate, currentUser.locale, "LLL");
                user.formattedLastPasswordChangeDate = getLocalDate(user.lastPasswordChangeDate, currentUser.locale, "LLL");
            });
        }

        function allowGroupEdit(group) {
            if (!currentLoggedInUser) {
                return false;
            }
            if (currentLoggedInUser.userGroups.indexOf(group.alias) === -1 && currentLoggedInUser.userGroups.indexOf("admin") === -1) {
                return false;
            }
            return true;
        }

        init();
    }
    angular.module("umbraco").controller("Umbraco.Editors.Users.UserController", UserEditController);
})();
