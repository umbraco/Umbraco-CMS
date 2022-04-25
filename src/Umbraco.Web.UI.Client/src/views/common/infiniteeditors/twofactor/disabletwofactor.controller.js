//used for the user editor overlay
angular.module("umbraco").controller("Umbraco.Editors.DisableTwoFactorController",
  function ($scope,
    localizationService,
    notificationsService,
    overlayService,
    twoFactorLoginResource) {

    let vm = this;
    vm.close = close;
    vm.disableWithCode = disableWithCode;
    vm.code = "";
    vm.buttonState = "init";
    vm.authForm = {};

    if (!$scope.model.provider) {
      notificationsService.error("No provider specified");
    }
    vm.provider = $scope.model.provider;
    vm.title = vm.provider.providerName;

    function close() {
      if ($scope.model.close) {
        $scope.model.close();
      }
    }

    function disableWithCode() {
      vm.authForm.token.$setValidity("token", true);
      vm.buttonState = "busy";
      twoFactorLoginResource.disableWithCode(vm.provider.providerName, vm.code)
        .then(onResponse)
        .catch(onError);
    }

    function onResponse(response) {
      if (response) {
        vm.buttonState = "success";
        localizationService.localize("user_2faProviderIsDisabledMsg").then(function (value) {
          notificationsService.info(value);
        });
        close();
      } else {
        vm.buttonState = "error";
        vm.authForm.token.$setValidity("token", false);
      }
    }

    function onError(error) {
      vm.buttonState = "error";
      overlayService.ysod(error);
    }
  });
