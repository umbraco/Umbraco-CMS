(function () {
  "use strict";

  function AnalyticsController(analyticResource) {

    var vm = this;
    vm.getConsentLevel = getConsentLevel;
    vm.getAllConsentLevels = getAllConsentLevels;
    vm.saveConsentLevel = saveConsentLevel;
    vm.sliderChange = sliderChange;
    vm.consentLevel = '';
    vm.consentLevels = [];
    vm.val = 2;
    vm.sliderVal = '';
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
    function saveConsentLevel(){
      analyticResource.saveConsentLevel(vm.sliderVal);
    }

    function sliderChange(values) {
      vm.sliderVal = values[0];
      console.log(vm.sliderVal);
    }
  }
    angular.module("umbraco").controller("Umbraco.Dashboard.AnalyticsController", AnalyticsController);
  })();
