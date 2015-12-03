angular.module("umbraco")
    .controller("Umbraco.Dialogs.UserController", function ($scope, $location, $timeout, userService, historyService, eventsService, externalLoginInfo, authResource, currentUserResource, formHelper) {

        $scope.history = historyService.getCurrent();
        $scope.version = Umbraco.Sys.ServerVariables.application.version + " assembly: " + Umbraco.Sys.ServerVariables.application.assemblyVersion;
        $scope.showPasswordFields = false;
        $scope.changePasswordButtonState = "init";

        $scope.externalLoginProviders = externalLoginInfo.providers;
        $scope.externalLinkLoginFormAction = Umbraco.Sys.ServerVariables.umbracoUrls.externalLinkLoginsUrl;
        var evts = [];
        evts.push(eventsService.on("historyService.add", function (e, args) {
            $scope.history = args.all;
        }));
        evts.push(eventsService.on("historyService.remove", function (e, args) {
            $scope.history = args.all;
        }));
        evts.push(eventsService.on("historyService.removeAll", function (e, args) {
            $scope.history = [];
        }));

        $scope.logout = function () {

            //Add event listener for when there are pending changes on an editor which means our route was not successful
            var pendingChangeEvent = eventsService.on("valFormManager.pendingChanges", function (e, args) {
                //one time listener, remove the event
                pendingChangeEvent();
                $scope.close();
            });


            //perform the path change, if it is successful then the promise will resolve otherwise it will fail
            $scope.close();
            $location.path("/logout");
        };

        $scope.gotoHistory = function (link) {
            $location.path(link);
            $scope.close();
        };

        //Manually update the remaining timeout seconds
        function updateTimeout() {
            $timeout(function () {
                if ($scope.remainingAuthSeconds > 0) {
                    $scope.remainingAuthSeconds--;
                    $scope.$digest();
                    //recurse
                    updateTimeout();
                }

            }, 1000, false); // 1 second, do NOT execute a global digest
        }

        function updateUserInfo() {
            //get the user
            userService.getCurrentUser().then(function (user) {
                $scope.user = user;
                if ($scope.user) {
                    $scope.remainingAuthSeconds = $scope.user.remainingAuthSeconds;
                    $scope.canEditProfile = _.indexOf($scope.user.allowedSections, "users") > -1;
                    //set the timer
                    updateTimeout();

                    authResource.getCurrentUserLinkedLogins().then(function(logins) {
                        //reset all to be un-linked
                        for (var provider in $scope.externalLoginProviders) {
                            $scope.externalLoginProviders[provider].linkedProviderKey = undefined;
                        }

                        //set the linked logins
                        for (var login in logins) {
                            var found = _.find($scope.externalLoginProviders, function (i) {
                                return i.authType == login;
                            });
                            if (found) {
                                found.linkedProviderKey = logins[login];
                            }
                        }
                    });
                }
            });
        }

        $scope.unlink = function (e, loginProvider, providerKey) {
            var result = confirm("Are you sure you want to unlink this account?");
            if (!result) {
                e.preventDefault();
                return;
            }

            authResource.unlinkLogin(loginProvider, providerKey).then(function (a, b, c) {
                updateUserInfo();
            });
        }

        updateUserInfo();

        //remove all event handlers
        $scope.$on('$destroy', function () {
            for (var e = 0; e < evts.length; e++) {
                evts[e]();
            }

        });

        /* ---------- UPDATE PASSWORD ---------- */

        //create the initial model for change password property editor
        $scope.changePasswordModel = {
           alias: "_umb_password",
           view: "changepassword",
           config: {},
           value: {}
        };

        //go get the config for the membership provider and add it to the model
        currentUserResource.getMembershipProviderConfig().then(function(data) {
           $scope.changePasswordModel.config = data;
           //ensure the hasPassword config option is set to true (the user of course has a password already assigned)
           //this will ensure the oldPassword is shown so they can change it
           // disable reset password functionality beacuse it does not make sense inside the backoffice
           $scope.changePasswordModel.config.hasPassword = true;
           $scope.changePasswordModel.config.disableToggle = true;
           $scope.changePasswordModel.config.enableReset = false;
        });

        $scope.changePassword = function() {

           if (formHelper.submitForm({ scope: $scope })) {

                $scope.changePasswordButtonState = "busy";

                currentUserResource.changePassword($scope.changePasswordModel.value).then(function(data) {

                    //if the password has been reset, then update our model
                    if (data.value) {
                        $scope.changePasswordModel.value.generatedPassword = data.value;
                    }

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    $scope.changePasswordButtonState = "success";

                }, function (err) {

                    formHelper.handleError(err);

                    $scope.changePasswordButtonState = "error";

                });

            }

        };

        $scope.togglePasswordFields = function() {
           clearPasswordFields();
           $scope.showPasswordFields = !$scope.showPasswordFields;
        }

        function clearPasswordFields() {
           $scope.changePasswordModel.value.newPassword = "";
           $scope.changePasswordModel.confirm = "";
        }

    });
