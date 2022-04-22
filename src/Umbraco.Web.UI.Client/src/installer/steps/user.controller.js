angular.module("umbraco.install").controller("Umbraco.Install.UserController", function ($scope, installerService) {

  $scope.majorVersion = Umbraco.Sys.ServerVariables.application.version;
  $scope.passwordPattern = /.*/;
  $scope.installer.current.model.subscribeToNewsLetter = false;

  if ($scope.installer.current.model.minNonAlphaNumericLength > 0) {
    var exp = "";
    for (var i = 0; i < $scope.installer.current.model.minNonAlphaNumericLength; i++) {
      exp += ".*[\\W].*";
    }
    //replace duplicates
    exp = exp.replace(".*.*", ".*");
    $scope.passwordPattern = new RegExp(exp);
  }

  $scope.loading = true;
  let sliderRef = null;
  $scope.consentLevel = 'Basic';
  $scope.sliderValue = 2;
  $scope.sliderVal = $scope.installer.current.model.consentLevels[1];
  $scope.sliderOptions =
    {
      "start": 2,
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
            return $scope.consentLevels[value - 1];
          },
          from: function (value) {
            return Number(value);
          }
        }
      }
    };

  $scope.loading = false;
  $scope.validateAndInstall = function () {
    installerService.install();
  };

  $scope.validateAndForward = function () {
    if (this.myForm.$valid) {
      installerService.forward();
    }
  };

  $scope.setup = function (slider) {
    sliderRef = slider;
  }

  $scope.sliderChange = function (values) {
    const result = Number(values[0]);
    $scope.sliderVal = $scope.consentLevels[result - 1];
  };

});
