(function () {
  "use strict";

  function AnalyticsController(analyticResource) {

    var vm = this;
    vm.getConsentLevel = getConsentLevel;
    vm.getAllConsentLevels = getAllConsentLevels;
    vm.consentLevel = '';
    vm.consentLevels = [];

    vm.value = ["minimal", "basic", "detailed"];

    vm.val = "minimal";
    getConsentLevel();
    getAllConsentLevels();

    vm.sliderOptions = {
      "start": 2,
      "step": 1,
      "tooltips": [true],
      "format": {
        to: function (value) {
          return vm.consentLevels[value.toFixed(0) - 1];
        },
        from: function (value) {
          return Number(value);
        }
      },
      "range": {
        "min": 1,
        "max": 3
      }
    };


    function getConsentLevel() {
      analyticResource.getConsentLevel().then(function (response) {
        vm.consentLevel = response;
      })
    }
    function getAllConsentLevels(){
      analyticResource.getAllConsentLevels().then(function (response) {
        vm.consentLevels = response;
      })
    }
  }
    angular.module("umbraco").controller("Umbraco.Dashboard.AnalyticsController", AnalyticsController);
  })();
