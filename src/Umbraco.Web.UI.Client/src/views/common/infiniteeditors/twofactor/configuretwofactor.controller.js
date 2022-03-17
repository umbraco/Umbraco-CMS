//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Editors.ConfigureTwoFactorController",
    function ($scope,
              localizationService,
              twoFactorLoginResource,
              editorService) {


        let vm = this;
        vm.close = close;
        vm.enable = enable;
        vm.disable = disable;
        vm.disableWithCode = disableWithCode;
        vm.code = "";

        function onInit() {

          localizationService.localize("user_configureTwoFactor").then(function (value) {
            vm.title = value;
          });

          twoFactorLoginResource.get2FAProvidersForUser($scope.model.user.id)
            .then(function(providers) {
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
            .then(function(viewPath) {
              var providerSettings = {
                user: $scope.model.user,
                providerName: providerName,
                size: "small",
                view: viewPath,
                close: function() {
                  editorService.close();
                }
              };

              editorService.open(providerSettings);
            });


        }

        function disable(providerName) {
          twoFactorLoginResource.disable(providerName, $scope.model.user.key)
            .then(function(response) {
              //TODO change the button to an enable one instead?
              close();
            });
        }

      function disableWithCode(providerName) {
        twoFactorLoginResource.disableWithCode(providerName, vm.code)
          .then(function(response) {
            //TODO change the button to an enable one instead?
            close();
          });
      }
        //initialize
        onInit();

    });
