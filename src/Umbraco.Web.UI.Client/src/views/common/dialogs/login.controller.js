angular.module("umbraco").controller("Umbraco.Dialogs.LoginController",
  function ($scope, $cookies, $location, currentUserResource, formHelper, mediaHelper, umbRequestHelper, Upload, localizationService, userService, externalLoginInfo, resetPasswordCodeInfo, $timeout, authResource, dialogService, $q) {

    $scope.invitedUser = null;
    $scope.invitedUserPasswordModel = {
      password: "",
      confirmPassword: "",
      buttonState: "",
      passwordPolicies: null,
      passwordPolicyText: ""
    }
    $scope.avatarFile = {
      filesHolder: null,
      uploadStatus: null,
      uploadProgress: 0,
      maxFileSize: Umbraco.Sys.ServerVariables.umbracoSettings.maxFileSize + "KB",
      acceptedFileTypes: mediaHelper.formatFileTypes(Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes),
      uploaded: false
    }
    $scope.togglePassword = function () {
        var elem = $("form[name='loginForm'] input[name='password']");
        elem.attr("type", (elem.attr("type") === "text" ? "password" : "text"));
    }

    function init() {
      // Check if it is a new user
      var inviteVal = $location.search().invite;
      if (inviteVal && (inviteVal === "1" || inviteVal === "2")) {

        $q.all([
          //get the current invite user
          authResource.getCurrentInvitedUser().then(function (data) {
              $scope.invitedUser = data;
            },
            function() {
              //it failed so we should remove the search
              $location.search('invite', null);
            }),
          //get the membership provider config for password policies
          authResource.getMembershipProviderConfig().then(function (data) {
            $scope.invitedUserPasswordModel.passwordPolicies = data;

            //localize the text
            localizationService.localize("errorHandling_errorInPasswordFormat",
              [
                $scope.invitedUserPasswordModel.passwordPolicies.minPasswordLength,
                $scope.invitedUserPasswordModel.passwordPolicies.minNonAlphaNumericChars
              ]).then(function(data) {
                $scope.invitedUserPasswordModel.passwordPolicyText = data;
            });
          })
        ]).then(function () {

          $scope.inviteStep = Number(inviteVal);

        });
      }
    }

    $scope.changeAvatar = function (files, event) {
      if (files && files.length > 0) {
        upload(files[0]);
      }
    };

    $scope.getStarted = function() {
      $location.search('invite', null);
      $scope.submit(true);
    }

    function upload(file) {

      $scope.avatarFile.uploadProgress = 0;

      Upload.upload({
        url: umbRequestHelper.getApiUrl("currentUserApiBaseUrl", "PostSetAvatar"),
        fields: {},
        file: file
      }).progress(function (evt) {

        if ($scope.avatarFile.uploadStatus !== "done" && $scope.avatarFile.uploadStatus !== "error") {
          // set uploading status on file
          $scope.avatarFile.uploadStatus = "uploading";

          // calculate progress in percentage
          var progressPercentage = parseInt(100.0 * evt.loaded / evt.total, 10);

          // set percentage property on file
          $scope.avatarFile.uploadProgress = progressPercentage;
        }

      }).success(function (data, status, headers, config) {

        $scope.avatarFile.uploadProgress = 100;

        // set done status on file
        $scope.avatarFile.uploadStatus = "done";

        $scope.invitedUser.avatars = data;

        $scope.avatarFile.uploaded = true;

      }).error(function (evt, status, headers, config) {

        // set status done
        $scope.avatarFile.uploadStatus = "error";

        // If file not found, server will return a 404 and display this message
        if (status === 404) {
          $scope.avatarFile.serverErrorMessage = "File not found";
        }
        else if (status == 400) {
          //it's a validation error
          $scope.avatarFile.serverErrorMessage = evt.message;
        }
        else {
          //it's an unhandled error
          //if the service returns a detailed error
          if (evt.InnerException) {
            $scope.avatarFile.serverErrorMessage = evt.InnerException.ExceptionMessage;

            //Check if its the common "too large file" exception
            if (evt.InnerException.StackTrace && evt.InnerException.StackTrace.indexOf("ValidateRequestEntityLength") > 0) {
              $scope.avatarFile.serverErrorMessage = "File too large to upload";
            }

          } else if (evt.Message) {
            $scope.avatarFile.serverErrorMessage = evt.Message;
          }
        }
      });
    }

    $scope.inviteSavePassword = function () {

      if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

        $scope.invitedUserPasswordModel.buttonState = "busy";

        currentUserResource.performSetInvitedUserPassword($scope.invitedUserPasswordModel.password)
          .then(function (data) {

            //success
            formHelper.resetForm({ scope: $scope, notifications: data.notifications });
            $scope.invitedUserPasswordModel.buttonState = "success";
            //set the user and set them as logged in
            $scope.invitedUser = data;
            userService.setAuthenticationSuccessful(data);

            $scope.inviteStep = 2;

          }, function(err) {

            //error
            formHelper.handleError(err);

            $scope.invitedUserPasswordModel.buttonState = "error";

          });
      }
    };

    var setFieldFocus = function (form, field) {
      $timeout(function () {
        $("form[name='" + form + "'] input[name='" + field + "']").focus();
      });
    }

    var twoFactorloginDialog = null;
    function show2FALoginDialog(view, callback) {
      if (!twoFactorloginDialog) {
        twoFactorloginDialog = dialogService.open({

          //very special flag which means that global events cannot close this dialog
          manualClose: true,
          template: view,
          modalClass: "login-overlay",
          animation: "slide",
          show: true,
          callback: callback,

        });
      }
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
    $scope.backgroundImage = Umbraco.Sys.ServerVariables.umbracoSettings.loginBackgroundImage;

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

      //TODO: Do validation properly like in the invite password update

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
        },
        function (reason) {

          //is Two Factor required?
          if (reason.status === 402) {
            $scope.errorMsg = "Additional authentication required";
            show2FALoginDialog(reason.data.twoFactorView, $scope.submit);
          }
          else {
            $scope.errorMsg = reason.errorMsg;

            //set the form inputs to invalid
            $scope.loginForm.username.$setValidity("auth", false);
            $scope.loginForm.password.$setValidity("auth", false);
          }
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

      //TODO: Do validation properly like in the invite password update

      if (email && email.length > 0) {
        $scope.requestPasswordResetForm.email.$setValidity('auth', true);
      }

      $scope.showEmailResetConfirmation = false;

      if ($scope.requestPasswordResetForm.$invalid) {
        return;
      }

      $scope.errorMsg = "";

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

      //TODO: All of this logic can/should be shared! We should do validation the nice way instead of all of this manual stuff, see: inviteSavePassword
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

    init();

  });
