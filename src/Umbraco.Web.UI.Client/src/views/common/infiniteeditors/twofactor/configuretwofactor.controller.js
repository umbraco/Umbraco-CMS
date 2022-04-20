//used for the user editor overlay
angular.module("umbraco").controller("Umbraco.Editors.ConfigureTwoFactorController",
  function ($scope,
    localizationService,
    notificationsService,
    overlayService,
    twoFactorLoginResource,
    editorService) {


    let vm = this;
    vm.close = close;
    vm.enable = enable;
    vm.disable = disable;
    vm.code = "";
    vm.buttonState = "init";

    localizationService.localize("user_configureTwoFactor").then(function (value) {
      vm.title = value;
    });

    function onInit() {
      vm.code = "";

      twoFactorLoginResource.get2FAProvidersForUser($scope.model.user.id)
        .then(function (providers) {
          vm.providers = providers;
        });
    }

    function close() {
      if ($scope.model.close) {
        $scope.model.close();
      }
    }

    function enable(providerName) {
      twoFactorLoginResource.viewPathForProviderName(providerName)
        .then(function (viewPath) {
          var providerSettings = {
            user: $scope.model.user,
            providerName: providerName,
            size: "small",
            view: viewPath,
            close: function () {
              notificationsService.removeAll();
              editorService.close();
              onInit();
            }
          };

          editorService.open(providerSettings);
        }).catch(onError);
    }

    function disable(provider) {

      const disableTwoFactorSettings = {
        provider,
        user: vm.user,
        size: "small",
        view: "views/common/infiniteeditors/twofactor/disabletwofactor.html",
        close: function () {
          editorService.close();
          onInit();
        }
      };

      editorService.open(disableTwoFactorSettings);
    }

    function onError(error) {
      vm.buttonState = "error";
      overlayService.ysod(error);
    }

    //initialize
    onInit();
  });
