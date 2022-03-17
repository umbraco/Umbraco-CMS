angular.module("umbraco").controller("Umbraco.Login2faController",
    function($scope, userService, authResource) {
        $scope.code = "";
        $scope.provider = "";
        $scope.providers = [];
        $scope.step = "send";

        authResource.get2FAProviders()
            .then(function (data) {
                $scope.providers = data;
                console.log($scope.providers);
                if ($scope.providers.length === 1) {
                    $scope.step = "code";
                    $scope.provider = $scope.providers[0];
                }
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
                    $scope.vm.twoFactor.submitCallback();
                }, function() {
                     $scope.error2FA = "Invalid code entered.";
                });
        };
    });
