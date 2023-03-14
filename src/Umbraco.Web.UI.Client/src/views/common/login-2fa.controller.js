angular.module("umbraco").controller("Umbraco.Login2faController",
  function ($scope, userService, authResource) {
    let vm = this;
    vm.code = "";
    vm.provider = "";
    vm.providers = [];
    vm.stateValidateButton = "init";
    vm.authForm = {};

    authResource.get2FAProviders()
      .then(function (data) {
        vm.providers = data;
        if (vm.providers.length > 0) {
          vm.provider = vm.providers[0];
        }
      });

    vm.validate = function () {
      vm.error = "";
      vm.stateValidateButton = "busy";
      vm.authForm.token.$setValidity('token', true);

      authResource.verify2FACode(vm.provider, vm.code)
        .then(function (data) {
          vm.stateValidateButton = "success";
          userService.setAuthenticationSuccessful(data);
          $scope.vm.twoFactor.submitCallback();
        })
        .catch(function () {
          vm.stateValidateButton = "error";
          vm.authForm.token.$setValidity('token', false);
        });
    };

    vm.goBack = function () {
      $scope.vm.twoFactor.cancelCallback();
    }
  });
