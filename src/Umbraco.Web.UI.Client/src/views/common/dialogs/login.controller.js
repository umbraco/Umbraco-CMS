angular.module("umbraco").controller("Umbraco.Dialogs.LoginController",
    function ($scope, $location, $timeout, localizationService, userService, externalLoginInfo) {

        var setFieldFocus = function(form, field) {
            $timeout(function() {
                $("form[name='" + form + "'] input[name='" + field + "']").focus();
            });
        }

        $scope.showLogin = function () {
            $scope.errorMsg = "";
            $scope.view = "login";
            setFieldFocus("loginForm", "username");
        }

        $scope.showRequestPasswordReset = function () {
            $scope.errorMsg = "";
            $scope.view = "request-password-reset";
            $scope.showEmailResetConfirmation = false;
            setFieldFocus("requestPasswordResetForm", "email");
        }

        $scope.showSetPassword = function () {
            $scope.view = "set-password";
            setFieldFocus("setPasswordForm", "password");
        }

        var d = new Date();
        localizationService.localize("login_greeting" + d.getDay()).then(function (label) {
            $scope.greeting = label;
        });

        $scope.externalLoginFormAction = Umbraco.Sys.ServerVariables.umbracoUrls.externalLoginsUrl;
        $scope.externalLoginProviders = externalLoginInfo.providers;
        $scope.externalLoginInfo = externalLoginInfo;

        // Set initial view - either set password if reset code provided in querystring
        // otherwise login form
        var userId = $location.search().userId;
        var resetCode = $location.search().resetCode;
        if (userId && resetCode) {
            userService.validatePasswordResetCode(userId, resetCode)
                .then(function () {
                    $scope.showSetPassword();
                }, function () {
                    $scope.view = "password-reset-code-expired";
                });
        } else {
            $scope.showLogin();
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

            userService.requestPasswordReset(email)
                .then(function () {
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

            userService.setPassword(userId, password, confirmPassword, resetCode)
                .then(function () {
                    $scope.showSetPasswordConfirmation = true;
                }, function (reason) {
                    $scope.errorMsg = reason.errorMsg;
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

    });
