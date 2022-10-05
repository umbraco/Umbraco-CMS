angular.module("umbraco.install").controller("Umbraco.Install.UserController", function ($scope, $sce, installerService) {

  $scope.majorVersion = Umbraco.Sys.ServerVariables.application.version;
  $scope.passwordPattern = /.*/;
  $scope.installer.current.model.subscribeToNewsLetter = $scope.installer.current.model.subscribeToNewsLetter || false;
  setTelemetryLevelAndDescription($scope.installer.current.model.telemetryIndex ?? 1);

  if ($scope.installer.current.model.minNonAlphaNumericLength > 0) {
    var exp = "";
    for (var i = 0; i < $scope.installer.current.model.minNonAlphaNumericLength; i++) {
      exp += ".*[\\W].*";
    }
    //replace duplicates
    exp = exp.replace(".*.*", ".*");
    $scope.passwordPattern = new RegExp(exp);
  }

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
      behaviour: 'smooth-steps-tap',
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

      consentSlider.noUiSlider.on('update', function (values,handle) {
        consentSlider.querySelectorAll('.noUi-value').forEach(pip => {
          pip.classList.remove("noUi-value-active");
          if (Number(values[handle]) === Number(pip.getAttribute('data-value'))) {
            pip.classList.add("noUi-value-active");
          }
        });
      });

      $(consentSlider).on('$destroy', function () {
        consentSlider.noUiSlider.off();
      });

      pips.forEach(function (pip) {
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
    if (this.myForm.$valid) {
      installerService.forward();
    }
  };

  function onChangeConsent(values) {
    const result = Math.round(Number(values[0]) - 1);

    $scope.$apply(() => {
      setTelemetryLevelAndDescription(result);
    });
  };

  function setTelemetryLevelAndDescription(idx) {
    $scope.telemetryDescription = $sce.trustAsHtml($scope.installer.current.model.consentLevels[idx].description);
    $scope.installer.current.model.telemetryIndex = idx;
    $scope.installer.current.model.telemetryLevel = $scope.installer.current.model.consentLevels[idx].level;
  }

});
