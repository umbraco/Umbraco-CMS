(function () {
  "use strict";

  function AnalyticsController($q, analyticResource) {

    var vm = this;
    vm.getConsentLevel = getConsentLevel;
    vm.getAllConsentLevels = getAllConsentLevels;
    vm.saveConsentLevel = saveConsentLevel;
    vm.sliderChange = sliderChange;
    vm.consentLevel = '';
    vm.consentLevels = [];
    vm.val = 1;
    vm.sliderOptions = {};
    $q.all(
      [getConsentLevel(),
      getAllConsentLevels()
    ]).then( () => {
      vm.startPos = calculateStartPositionForSlider();
      vm.val = calculateStartPositionForSlider();
      vm.sliderVal = vm.consentLevels[vm.startPos];
      vm.sliderOptions =
        {
        "start": vm.startPos,
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
    });

    function getConsentLevel() {
      return analyticResource.getConsentLevel().then(function (response) {
        vm.consentLevel = response;
      })
    }
    function getAllConsentLevels(){
      return analyticResource.getAllConsentLevels().then(function (response) {
        vm.consentLevels = response;
      })
    }
    function saveConsentLevel(){
      analyticResource.saveConsentLevel(vm.sliderVal);
    }

    function sliderChange(values) {
      vm.sliderVal = values[0];
    }

    function calculateStartPositionForSlider(){
      vm.sliderVal = vm.consentLevel;
      let startPosition = vm.consentLevels.indexOf(vm.consentLevel) + 1;
      if(startPosition === -1){
        return 2;
      }
      return startPosition;
    }
  }
    angular.module("umbraco").controller("Umbraco.Dashboard.AnalyticsController", AnalyticsController);
  })();
