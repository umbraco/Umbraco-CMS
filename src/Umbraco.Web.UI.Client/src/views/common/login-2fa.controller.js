angular.module("umbraco").controller("Umbraco.Login2faController",
  function ($scope, userService, authResource) {
    let vm = this;
    vm.code = "";
    vm.provider = "";
    vm.providers = [];
    vm.step = "send";
    vm.error = "";
    vm.stateValidateButton = "";

    authResource.get2FAProviders()
      .then(function (data) {
        vm.providers = data;
        if (vm.providers.length === 1) {
          vm.step = "code";
          vm.provider = vm.providers[0];
        }
      });

    vm.send = function (provider) {
      vm.provider = provider;
      vm.step = "code";
    };

    vm.validate = function () {
      vm.error = "";
      vm.stateValidateButton = "busy";
      authResource.verify2FACode(vm.provider, vm.code)
        .then(function (data) {
          vm.stateValidateButton = "success";
          userService.setAuthenticationSuccessful(data);
          $scope.vm.twoFactor.submitCallback();
        }, function () {
          vm.stateValidateButton = "error";
          vm.error = "invalid";
        });
    };
  });
