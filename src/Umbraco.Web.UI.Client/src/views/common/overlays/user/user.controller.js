angular.module("umbraco")
    .controller("Umbraco.Overlays.UserController", function ($scope, $location, $timeout,
        dashboardResource, userService, historyService, eventsService,
        externalLoginInfo, externalLoginInfoService, authResource,
        currentUserResource, formHelper, localizationService) {

        $scope.history = historyService.getCurrent();
        //$scope.version = Umbraco.Sys.ServerVariables.application.version + " assembly: " + Umbraco.Sys.ServerVariables.application.assemblyVersion;
        $scope.showPasswordFields = false;
        $scope.changePasswordButtonState = "init";
        $scope.model.title = "user.name";
        //$scope.model.subtitle = "Umbraco version" + " " + $scope.version;
        /*
        if(!$scope.model.title) {
            localizationService.localize("general_user").then(function(value){
                $scope.model.title = value;
            });
        }
        */

        // Set flag if any have deny local login, in which case we must disable all password functionality
        $scope.denyLocalLogin = externalLoginInfoService.hasDenyLocalLogin();
        // Only include login providers that have editable options
        $scope.externalLoginProviders = externalLoginInfoService.getLoginProvidersWithOptions();

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
                $scope.model.close();
            });


            //perform the path change, if it is successful then the promise will resolve otherwise it will fail
            $scope.model.close();
            $location.path("/logout").search('');
        };

        $scope.gotoHistory = function (link) {
            $location.path(link);
            $scope.model.close();
        };
        /*
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
        */
        function updateUserInfo() {
            //get the user
            userService.getCurrentUser().then(function (user) {
                $scope.user = user;
                if ($scope.user) {
                    $scope.model.title = user.name;
                    $scope.remainingAuthSeconds = $scope.user.remainingAuthSeconds;
                    $scope.canEditProfile = _.indexOf($scope.user.allowedSections, "users") > -1;
                    //set the timer
                    //updateTimeout();

                    authResource.getCurrentUserLinkedLogins().then(function(logins) {

                        //reset all to be un-linked
                        $scope.externalLoginProviders.forEach(provider => provider.linkedProviderKey = undefined);

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

        $scope.linkProvider = function (e) {
            e.target.submit();
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

        //create the initial model for change password
        $scope.changePasswordModel = {
           config: {},
           value: {}
        };

        //go get the config for the membership provider and add it to the model
        authResource.getMembershipProviderConfig().then(function(data) {
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

                    //reset old data 
                    clearPasswordFields();

                    //if the password has been reset, then update our model
                    if (data.value) {
                        $scope.changePasswordModel.value.generatedPassword = data.value;
                    }

                    formHelper.resetForm({ scope: $scope });

                    $scope.changePasswordButtonState = "success";
                    $timeout(function() {
                        $scope.togglePasswordFields();
                    }, 2000);

                }, function (err) {
                    formHelper.resetForm({ scope: $scope, hasErrors: true });
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
           $scope.changePasswordModel.value.oldPassword = "";
           $scope.changePasswordModel.value.newPassword = "";
           $scope.changePasswordModel.value.confirm = "";
        }

        dashboardResource.getDashboard("user-dialog").then(function (dashboard) {
            $scope.dashboard = dashboard;
        });
    });
