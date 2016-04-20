angular.module("umbraco").controller("Umbraco.Dialogs.LoginController",
    function ($scope, $cookies, localizationService, userService, externalLoginInfo, resetPasswordCodeInfo, $timeout, authResource) {

        var setFieldFocus = function(form, field) {
            $timeout(function() {
                $("form[name='" + form + "'] input[name='" + field + "']").focus();
            });
        }

        function resetInputValidation() {
            $scope.confirmPassword = "";
            $scope.password = "";
            $scope.login = "";
            if ($scope.loginForm) {
                $scope.loginForm.username.$setValidity('auth', true);
                $scope.loginForm.password.$setValidity('auth', true);
            }
            if ($scope.requestPasswordResetForm) {
                $scope.requestPasswordResetForm.email.$setValidity("auth", true);
            }
            if ($scope.setPasswordForm) {
                $scope.setPasswordForm.password.$setValidity('auth', true);
                $scope.setPasswordForm.confirmPassword.$setValidity('auth', true);
            }
        }

        $scope.allowPasswordReset = Umbraco.Sys.ServerVariables.umbracoSettings.allowPasswordReset;

        $scope.showLogin = function () {
            $scope.errorMsg = "";
            resetInputValidation();
            $scope.view = "login";
            setFieldFocus("loginForm", "username");
        }

        $scope.showRequestPasswordReset = function () {
            $scope.errorMsg = "";
            resetInputValidation();
            $scope.view = "request-password-reset";
            $scope.showEmailResetConfirmation = false;
            setFieldFocus("requestPasswordResetForm", "email");
        }

        $scope.showSetPassword = function () {
            $scope.errorMsg = "";
            resetInputValidation();
            $scope.view = "set-password";
            setFieldFocus("setPasswordForm", "password");
        }

        var d = new Date();
        var konamiGreetings = new Array("Suze Sunday", "Malibu Monday", "Tequila Tuesday", "Whiskey Wednesday", "Negroni Day", "Fernet Friday", "Sancerre Saturday");
        var konamiMode = $cookies.konamiLogin;
        if (konamiMode == "1") {
            $scope.greeting = "Happy " + konamiGreetings[d.getDay()];
        } else {
            localizationService.localize("login_greeting" + d.getDay()).then(function (label) {
                $scope.greeting = label;
            }); // weekday[d.getDay()];
        }
        $scope.errorMsg = "";

        $scope.externalLoginFormAction = Umbraco.Sys.ServerVariables.umbracoUrls.externalLoginsUrl;
        $scope.externalLoginProviders = externalLoginInfo.providers;
        $scope.externalLoginInfo = externalLoginInfo;
        $scope.resetPasswordCodeInfo = resetPasswordCodeInfo;

        $scope.activateKonamiMode = function () {
            if ($cookies.konamiLogin == "1") {
                // somehow I can't update the cookie value using $cookies, so going native
                document.cookie = "konamiLogin=; expires=Thu, 01 Jan 1970 00:00:01 GMT;";
                document.location.reload();
            } else {
                document.cookie = "konamiLogin=1; expires=Tue, 01 Jan 2030 00:00:01 GMT;";
                $scope.$apply(function () {
                    $scope.greeting = "Happy " + konamiGreetings[d.getDay()];
                });
            }
        }

        $scope.loginSubmit = function (login, password) {

            //if the login and password are not empty we need to automatically 
            // validate them - this is because if there are validation errors on the server
            // then the user has to change both username & password to resubmit which isn't ideal,
            // so if they're not empty, we'll just make sure to set them to valid.
            if (login && password && login.length > 0 && password.length > 0) {
                $scope.loginForm.username.$setValidity('auth', true);
                $scope.loginForm.password.$setValidity('auth', true);
            }

            if ($scope.loginForm.$invalid) {
                return;
            }

            userService.authenticate(login, password)
                .then(function (data) {
                    $scope.submit(true);
                }, function (reason) {
                    $scope.errorMsg = reason.errorMsg;

                    //set the form inputs to invalid
                    $scope.loginForm.username.$setValidity("auth", false);
                    $scope.loginForm.password.$setValidity("auth", false);
                });

            //setup a watch for both of the model values changing, if they change
            // while the form is invalid, then revalidate them so that the form can 
            // be submitted again.
            $scope.loginForm.username.$viewChangeListeners.push(function () {
                if ($scope.loginForm.username.$invalid) {
                    $scope.loginForm.username.$setValidity('auth', true);
                }
            });
            $scope.loginForm.password.$viewChangeListeners.push(function () {
                if ($scope.loginForm.password.$invalid) {
                    $scope.loginForm.password.$setValidity('auth', true);
                }
            });
        };

        $scope.requestPasswordResetSubmit = function (email) {

            $scope.errorMsg = "";
            $scope.showEmailResetConfirmation = false;

            if ($scope.requestPasswordResetForm.$invalid) {
                return;
            }

            authResource.performRequestPasswordReset(email)
                .then(function () {
                    //remove the email entered
                    $scope.email = "";
                    $scope.showEmailResetConfirmation = true;
                }, function (reason) {
                    $scope.errorMsg = reason.errorMsg;
                    $scope.requestPasswordResetForm.email.$setValidity("auth", false);
                });

            $scope.requestPasswordResetForm.email.$viewChangeListeners.push(function () {
                if ($scope.requestPasswordResetForm.email.$invalid) {
                    $scope.requestPasswordResetForm.email.$setValidity('auth', true);
                }
            });
        };

        $scope.setPasswordSubmit = function (password, confirmPassword) {

            $scope.showSetPasswordConfirmation = false;

            if (password && confirmPassword && password.length > 0 && confirmPassword.length > 0) {
                $scope.setPasswordForm.password.$setValidity('auth', true);
                $scope.setPasswordForm.confirmPassword.$setValidity('auth', true);
            }

            if ($scope.setPasswordForm.$invalid) {
                return;
            }

            authResource.performSetPassword($scope.resetPasswordCodeInfo.resetCodeModel.userId, password, confirmPassword, $scope.resetPasswordCodeInfo.resetCodeModel.resetCode)
                .then(function () {
                    $scope.showSetPasswordConfirmation = true;
                    $scope.resetComplete = true;

                    //reset the values in the resetPasswordCodeInfo angular so if someone logs out the change password isn't shown again
                    resetPasswordCodeInfo.resetCodeModel = null;

                }, function (reason) {
                    if (reason.data && reason.data.Message) {
                        $scope.errorMsg = reason.data.Message;
                    }
                    else {
                        $scope.errorMsg = reason.errorMsg;
                    }
                    $scope.setPasswordForm.password.$setValidity("auth", false);
                    $scope.setPasswordForm.confirmPassword.$setValidity("auth", false);
                });

            $scope.setPasswordForm.password.$viewChangeListeners.push(function () {
                if ($scope.setPasswordForm.password.$invalid) {
                    $scope.setPasswordForm.password.$setValidity('auth', true);
                }
            });
            $scope.setPasswordForm.confirmPassword.$viewChangeListeners.push(function () {
                if ($scope.setPasswordForm.confirmPassword.$invalid) {
                    $scope.setPasswordForm.confirmPassword.$setValidity('auth', true);
                }
            });
        }


        //Now, show the correct panel:

        if ($scope.resetPasswordCodeInfo.resetCodeModel) {
            $scope.showSetPassword();
        }
        else if ($scope.resetPasswordCodeInfo.errors.length > 0) {
            $scope.view = "password-reset-code-expired";
        }
        else {
            $scope.showLogin();
        }

    });
