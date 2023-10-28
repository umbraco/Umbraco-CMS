(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbLogin', {
            templateUrl: 'views/components/application/umb-login.html',
            controller: UmbLoginController,
            controllerAs: 'vm',
            bindings: {
                isTimedOut: "<",
                onLogin: "&"
            }
        });

    function UmbLoginController($scope, $location, currentUserResource, formHelper,
        mediaHelper, umbRequestHelper, localizationService,
        userService, externalLoginInfo, externalLoginInfoService,
        resetPasswordCodeInfo, authResource, $q) {

        const vm = this;

        vm.invitedUser = null;

        vm.invitedUserPasswordModel = {
            password: "",
            confirmPassword: "",
            buttonState: "",
            passwordPolicies: null,
            passwordPolicyText: ""
        };

        vm.loginStates = {
            submitButton: "init"
        };

        vm.allowPasswordReset = Umbraco.Sys.ServerVariables.umbracoSettings.canSendRequiredEmail && Umbraco.Sys.ServerVariables.umbracoSettings.allowPasswordReset;
        vm.errorMsg = "";
        const tempUrl = new URL(Umbraco.Sys.ServerVariables.umbracoUrls.externalLoginsUrl, window.location.origin);
        tempUrl.searchParams.append("redirectUrl", decodeURIComponent($location.search().returnPath ?? ""))

        vm.externalLoginFormAction = tempUrl.pathname + tempUrl.search;
        vm.externalLoginProviders = externalLoginInfoService.getLoginProviders();
        vm.externalLoginProviders.forEach(x => {
            x.customView = externalLoginInfoService.getLoginProviderView(x);
            // if there are errors set for this specific provider than assign them directly to the model
            if (externalLoginInfo.errorProvider === x.authType) {
                x.errors = externalLoginInfo.errors;
            }
        });
        vm.denyLocalLogin = externalLoginInfoService.hasDenyLocalLogin();
        vm.externalLoginInfo = externalLoginInfo;
        vm.resetPasswordCodeInfo = resetPasswordCodeInfo;
        vm.logoImage = Umbraco.Sys.ServerVariables.umbracoSettings.loginLogoImage;
        vm.backgroundImage = Umbraco.Sys.ServerVariables.umbracoSettings.loginBackgroundImage;
        vm.usernameIsEmail = Umbraco.Sys.ServerVariables.umbracoSettings.usernameIsEmail;

        vm.$onInit = onInit;
        vm.togglePassword = togglePassword;
        vm.getStarted = getStarted;
        vm.inviteSavePassword = inviteSavePassword;
        vm.showLogin = showLogin;
        vm.showRequestPasswordReset = showRequestPasswordReset;
        vm.showSetPassword = showSetPassword;
        vm.loginSubmit = loginSubmit;
        vm.requestPasswordResetSubmit = requestPasswordResetSubmit;
        vm.setPasswordSubmit = setPasswordSubmit;
        vm.newPasswordKeyUp = newPasswordKeyUp;
        vm.labels = {};
        localizationService.localizeMany([
            vm.usernameIsEmail ? "general_email" : "general_username",
            vm.usernameIsEmail ? "placeholders_email" : "placeholders_usernameHint",
            vm.usernameIsEmail ? "placeholders_emptyEmail" : "placeholders_emptyUsername",
            "placeholders_emptyPassword"]
        ).then(function (data) {
            vm.labels.usernameLabel = data[0];
            vm.labels.usernamePlaceholder = data[1];
            vm.labels.usernameError = data[2];
            vm.labels.passwordError = data[3];
        });

        vm.twoFactor = {};

        vm.loginSuccess = loginSuccess;

        function onInit() {

            // Check if it is a new user
            const inviteVal = $location.search().invite;

            //1 = enter password, 2 = password set, 3 = invalid token
            if (inviteVal && (inviteVal === "1" || inviteVal === "2")) {

                $q.all([
                    //get the current invite user
                    authResource.getCurrentInvitedUser().then(function (data) {
                        vm.invitedUser = data;
                    },
                        function () {
                            //it failed so we should remove the search
                            $location.search('invite', null);
                        }),
                    //get the membership provider config for password policies
                    authResource.getPasswordConfig(0).then(function (data) {
                        vm.invitedUserPasswordModel.passwordPolicies = data;

                        //localize the text
                        localizationService.localize("errorHandling_errorInPasswordFormat", [
                            vm.invitedUserPasswordModel.passwordPolicies.minPasswordLength,
                            vm.invitedUserPasswordModel.passwordPolicies.minNonAlphaNumericChars
                        ]).then(function (data) {
                            vm.invitedUserPasswordModel.passwordPolicyText = data;
                        });
                    })
                ]).then(function () {
                    vm.inviteStep = Number(inviteVal);
                });

            } else if (inviteVal && inviteVal === "3") {
                vm.inviteStep = Number(inviteVal);
            }

            // set the welcome greeting
            setGreeting();

            // show the correct panel
            if (vm.resetPasswordCodeInfo.resetCodeModel) {
                vm.showSetPassword();
            }
            else if (vm.resetPasswordCodeInfo.errors.length > 0) {
                vm.view = "password-reset-code-expired";
            }
            else {
                vm.showLogin();
            }

            SetTitle();
        }

        function togglePassword() {
            var elem = $("form[name='vm.loginForm'] input[name='password']");
            elem.attr("type", (elem.attr("type") === "text" ? "password" : "text"));
            elem.focus();
            $(".password-text.show, .password-text.hide").toggle();
        }

        function getStarted() {
            $location.search('invite', null);
            if (vm.onLogin) {
                vm.onLogin();
            }
        }

        function inviteSavePassword() {

            if (formHelper.submitForm({ scope: $scope, formCtrl: vm.inviteUserPasswordForm })) {

                vm.invitedUserPasswordModel.buttonState = "busy";

                currentUserResource.performSetInvitedUserPassword(vm.invitedUserPasswordModel.password)
                    .then(function (data) {

                        //success
                        formHelper.resetForm({ scope: $scope, formCtrl: vm.inviteUserPasswordForm });
                        vm.invitedUserPasswordModel.buttonState = "success";

                        //set the user
                        vm.invitedUser = data;

                        // hide the password form
                        vm.inviteStep = 2;

                        // set the user as logged in, which will initialise the app flow in init.js
                        // and eventually redirect the user to the content section when it's ready
                        userService.setAuthenticationSuccessful(data);

                    }, function (err) {
                        formHelper.resetForm({ scope: $scope, hasErrors: true, formCtrl: vm.inviteUserPasswordForm });
                        formHelper.handleError(err);
                        vm.invitedUserPasswordModel.buttonState = "error";
                    });
            }
        }

        function showLogin() {
            vm.errorMsg = "";
            resetInputValidation();
            vm.view = "login";
            SetTitle();
        }

        function showRequestPasswordReset() {
            vm.errorMsg = "";
            resetInputValidation();
            vm.view = "request-password-reset";
            vm.showEmailResetConfirmation = false;
            SetTitle();
        }

        function showSetPassword() {
            vm.errorMsg = "";
            resetInputValidation();
            vm.view = "set-password";
            SetTitle();
        }

        function loginSuccess() {
            vm.loginStates.submitButton = "success";
            userService._retryRequestQueue(true);
            if (vm.onLogin) {
                vm.onLogin();
            }
        }

        function loginSubmit() {

            if (formHelper.submitForm({ scope: $scope, formCtrl: vm.loginForm })) {
                //if the login and password are not empty we need to automatically
                // validate them - this is because if there are validation errors on the server
                // then the user has to change both username & password to resubmit which isn't ideal,
                // so if they're not empty, we'll just make sure to set them to valid.
                if (vm.login && vm.password && vm.login.length > 0 && vm.password.length > 0) {
                    vm.loginForm.username.$setValidity('auth', true);
                    vm.loginForm.password.$setValidity('auth', true);
                }

                if (vm.loginForm.$invalid) {
                    SetTitle();
                    return;
                }

                // make sure that we are returning to the login view.
                vm.view = "login";

                vm.loginStates.submitButton = "busy";

                userService.authenticate(vm.login, vm.password)
                    .then(function (data) {
                        loginSuccess();
                    },
                        function (reason) {

                            //is Two Factor required?
                            if (reason.status === 402) {
                                vm.errorMsg = "Additional authentication required";
                                show2FALoginDialog(reason.data.twoFactorView);
                            } else {
                                vm.loginStates.submitButton = "error";
                                vm.errorMsg = reason.errorMsg;

                                //set the form inputs to invalid
                                vm.loginForm.username.$setValidity("auth", false);
                                vm.loginForm.password.$setValidity("auth", false);
                            }

                            userService._retryRequestQueue();

                        });

                //setup a watch for both of the model values changing, if they change
                // while the form is invalid, then revalidate them so that the form can
                // be submitted again.
                vm.loginForm.username.$viewChangeListeners.push(function () {
                    if (vm.loginForm.$invalid) {
                        vm.loginForm.username.$setValidity('auth', true);
                        vm.loginForm.password.$setValidity('auth', true);
                    }
                });
                vm.loginForm.password.$viewChangeListeners.push(function () {
                    if (vm.loginForm.$invalid) {
                        vm.loginForm.username.$setValidity('auth', true);
                        vm.loginForm.password.$setValidity('auth', true);
                    }
                });
            }
        }

        function requestPasswordResetSubmit(email) {

            // TODO: Do validation properly like in the invite password update

            if (email && email.length > 0) {
                vm.requestPasswordResetForm.email.$setValidity('auth', true);
            }

            vm.showEmailResetConfirmation = false;

            if (vm.requestPasswordResetForm.$invalid) {
                vm.errorMsg = 'Email address cannot be empty';
                return;
            }

            vm.errorMsg = "";

            authResource.performRequestPasswordReset(email)
                .then(function () {
                    //remove the email entered
                    vm.email = "";
                    vm.showEmailResetConfirmation = true;
                }, function (reason) {
                    vm.errorMsg = reason.errorMsg;
                    vm.requestPasswordResetForm.email.$setValidity("auth", false);
                });

            vm.requestPasswordResetForm.email.$viewChangeListeners.push(function () {
                if (vm.requestPasswordResetForm.email.$invalid) {
                    vm.requestPasswordResetForm.email.$setValidity('auth', true);
                }
            });
        }

        function setPasswordSubmit(password, confirmPassword) {

            vm.showSetPasswordConfirmation = false;

            if (password && confirmPassword && password.length > 0 && confirmPassword.length > 0) {
                vm.setPasswordForm.password.$setValidity('auth', true);
                vm.setPasswordForm.confirmPassword.$setValidity('auth', true);
            }

            if (vm.setPasswordForm.$invalid) {
                return;
            }

            // TODO: All of this logic can/should be shared! We should do validation the nice way instead of all of this manual stuff, see: inviteSavePassword
            authResource.performSetPassword(vm.resetPasswordCodeInfo.resetCodeModel.userId, password, confirmPassword, vm.resetPasswordCodeInfo.resetCodeModel.resetCode)
                .then(function () {
                    vm.showSetPasswordConfirmation = true;
                    vm.resetComplete = true;

                    //reset the values in the resetPasswordCodeInfo angular so if someone logs out the change password isn't shown again
                    resetPasswordCodeInfo.resetCodeModel = null;

                }, function (reason) {
                    if (reason.data && reason.data.Message) {
                        vm.errorMsg = reason.data.Message;
                    }
                    else {
                        vm.errorMsg = reason.errorMsg;
                    }
                    vm.setPasswordForm.password.$setValidity("auth", false);
                    vm.setPasswordForm.confirmPassword.$setValidity("auth", false);
                });

            vm.setPasswordForm.password.$viewChangeListeners.push(function () {
                if (vm.setPasswordForm.password.$invalid) {
                    vm.setPasswordForm.password.$setValidity('auth', true);
                }
            });

            vm.setPasswordForm.confirmPassword.$viewChangeListeners.push(function () {
                if (vm.setPasswordForm.confirmPassword.$invalid) {
                    vm.setPasswordForm.confirmPassword.$setValidity('auth', true);
                }
            });
        }

        function newPasswordKeyUp(event) {
            vm.passwordVal = event.target.value;
        }

        ////

        function setGreeting() {
            const date = new Date();
            localizationService.localize("login_greeting" + date.getDay()).then(function (label) {
                $scope.greeting = label;
            });
        }

        function show2FALoginDialog(viewPath) {
            vm.twoFactor.submitCallback = function submitCallback() {
                vm.onLogin();
            }
            vm.twoFactor.cancelCallback = function cancelCallback() {
              vm.showLogin();
            }
            vm.twoFactor.view = viewPath;
            vm.view = "2fa-login";
            SetTitle();
        }

        function resetInputValidation() {
            vm.loginStates.submitButton = "init";
            vm.confirmPassword = "";
            vm.password = "";
            vm.login = "";
            if (vm.loginForm) {
                vm.loginForm.username.$setValidity('auth', true);
                vm.loginForm.password.$setValidity('auth', true);
            }
            if (vm.requestPasswordResetForm) {
                vm.requestPasswordResetForm.email.$setValidity("auth", true);
            }
            if (vm.setPasswordForm) {
                vm.setPasswordForm.password.$setValidity('auth', true);
                vm.setPasswordForm.confirmPassword.$setValidity('auth', true);
            }
        }


        function SetTitle() {
            var title = null;
            switch (vm.view.toLowerCase()) {
                case "login":
                    title = "Login";
                    break;
                case "password-reset-code-expired":
                case "request-password-reset":
                    title = "Password Reset";
                    break;
                case "set-password":
                    title = "Change Password";
                    break;
                case "2fa-login":
                    title = "Two Factor Authentication";
                    break;
            }

            $scope.$emit("$changeTitle", title);
        }

    }

})();
