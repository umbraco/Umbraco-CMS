angular.module("umbraco").controller("TwoFactorAuthentication.DashboardController",
    function($scope, twoFactorService, userService) {

        $scope.error2FA = "";
        $scope.code = "";
        $scope.enabledText = "disabled";
        $scope.enabled = false;
        $scope.qrCodeImageUrl = "";
        $scope.secret = "";
        $scope.email = "";
        $scope.applicationName = "";
        $scope.googleAuthEnabled = false;

        twoFactorService.getGoogleAuthenticatorSetupCode().then(function (response) {
            $scope.qrCodeImageUrl = response.data;
            $scope.secret = response.data.Secret;
            $scope.email = response.data.Email;
            $scope.applicationName = response.data.ApplicationName;
        });

        userService.getCurrentUser().then(function (userData) {
            twoFactorService.getEnabled(userData.id).then(function (response) {
                
                if (response.data.length !== 0) {
                    $scope.enabledText = "enabled";
                    $scope.enabled = true;
                    for (var i = 0; i < response.data.length; i++) {
                        var currentProvider = response.data[i];
                        if (currentProvider.ApplicationName === "GoogleAuthenticator") {
                            $scope.googleAuthEnabled = true;
                            console.log($scope.googleAuthEnabled);
                        }
                    }
                }
            });
        });

        $scope.validateAndSaveGoogleAuth = function (code) {
            $scope.error2FA = "";
            twoFactorService.validateAndSaveGoogleAuth(code)
                .then(function (response) {
                    if (response.data === "true") {
                        $scope.enabledText = "enabled";
                        $scope.enabled = true;
                        $scope.googleAuthEnabled = true;
                    } else {
                        $scope.error2FA = "Invalid code entered.";
                    }
                }, function () {
                    $scope.error2FA = "Error validating code.";
                });
        };

        $scope.disable = function () {
            twoFactorService.disable()
                .then(function (response) {
                    if (response.data === "true") {
                        $scope.enabledText = "disabled";
                        $scope.enabled = false;
                        $scope.googleAuthEnabled = false;

                        twoFactorService.getGoogleAuthenticatorSetupCode().then(function (response) {
                            $scope.qrCodeImageUrl = response.data;
                            $scope.secret = response.data.Secret;
                            $scope.email = response.data.Email;
                            $scope.applicationName = response.data.ApplicationName;
                        });
                    }
                });
        };
    });