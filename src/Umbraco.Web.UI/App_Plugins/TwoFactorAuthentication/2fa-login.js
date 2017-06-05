angular.module("umbraco").controller("TwoFactorAuthentication.LoginController",
    function ($scope, $cookies, localizationService, userService, externalLoginInfo, resetPasswordCodeInfo, $timeout, authResource) {

        $scope.code = "";
        $scope.provider = "";
        $scope.providers = [];
        $scope.step = "send";

        authResource.get2FAProviders()
            .then(function (data) {
                console.log(data);
                $scope.providers = data;
            });

        $scope.send = function (provider) {
            $scope.provider = provider;
            $scope.step = "code";
        };

        $scope.validate = function (provider, code) {
            $scope.error2FA = "";
            $scope.code = code;
            authResource.verify2FACode(provider, code)
                .then(function (data) {
                    userService.setAuthenticationSuccessful(data);
                    $scope.submit(true);
                }, function () { $scope.error2FA = "Invalid code entered." });
        };
    });