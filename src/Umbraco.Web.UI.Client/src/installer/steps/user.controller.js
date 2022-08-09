angular.module("umbraco.install").controller("Umbraco.Install.UserController", function ($scope, $sce, installerService) {

  $scope.majorVersion = Umbraco.Sys.ServerVariables.application.version;
  $scope.passwordPattern = /.*/;
  $scope.installer.current.model.subscribeToNewsLetter = $scope.installer.current.model.subscribeToNewsLetter || false;
  $scope.installer.current.model.telemetryLevel = $scope.installer.current.model.telemetryLevel || $scope.installer.current.model.consentLevels[1].level;

  if ($scope.installer.current.model.minNonAlphaNumericLength > 0) {
    var exp = "";
    for (var i = 0; i < $scope.installer.current.model.minNonAlphaNumericLength; i++) {
      exp += ".*[\\W].*";
    }
    //replace duplicates
    exp = exp.replace(".*.*", ".*");
    $scope.passwordPattern = new RegExp(exp);
  }

  $scope.consentTooltip = {
    show: false,
    event: null
  };

  if ('noUiSlider' in window) {
    let consentSliderStartLevel = 2;
    const initialConsentLevel = $scope.installer.current.model.consentLevels.findIndex(x => x.level === $scope.installer.current.model.telemetryLevel);
    if (initialConsentLevel !== -1) {
      consentSliderStartLevel = initialConsentLevel + 1;
    }

    const sliderOptions =
    {
      "start": consentSliderStartLevel,
      "step": 1,
      "tooltips": [false],
      "range": {
        "min": 1,
        "max": 3
      },
      pips: {
        mode: 'values',
        density: 50,
        values: [1, 2, 3],
        "format": {
          to: function (value) {
            return $scope.installer.current.model.consentLevels[value - 1].level;
          },
          from: function (value) {
            return Number(value);
          }
        }
      }
    };

    const consentSlider = document.getElementById("consentSlider");
    if (consentSlider) {
      window.noUiSlider.create(consentSlider, sliderOptions);
      consentSlider.noUiSlider.on('change', onChangeConsent);

      const pips = consentSlider.querySelectorAll('.noUi-value');

      $(consentSlider).on('$destroy', function () {
        consentSlider.noUiSlider.off();
      });

      pips.forEach(function (pip) {
        pip.addEventListener('mouseenter', function (e) {
          $scope.$apply(function () {
            const value = pip.getAttribute('data-value');
            $scope.consentTooltip.show = true;
            $scope.consentTooltip.event = e;
            $scope.consentTooltip.description = $sce.trustAsHtml($scope.installer.current.model.consentLevels[value - 1].description);
          });
        });

        pip.addEventListener('mouseleave', function () {
          $scope.$apply(function () {
            $scope.consentTooltip.show = false;
            $scope.consentTooltip.event = null;
            $scope.consentTooltip.description = '';
          });
        });

        pip.addEventListener('click', function () {
          const value = pip.getAttribute('data-value');
          consentSlider.noUiSlider.set(value);
        });
      });
    }
  }

  $scope.validateAndInstall = function () {
    installerService.install();
  };

  $scope.validateAndForward = function () {
    if (this.installerForm.$valid) {
      installerService.forward();
    }
  };

  function onChangeConsent(values) {
    const result = Number(values[0]);
    $scope.installer.current.model.telemetryLevel = $scope.installer.current.model.consentLevels[result - 1].level;
  };

});
