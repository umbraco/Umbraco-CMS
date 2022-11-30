(function () {
  "use strict";

  function AnalyticsController($q, analyticResource, localizationService, notificationsService) {

    let sliderRef = null;

    const vm = this;
    vm.getConsentLevel = getConsentLevel;
    vm.getAllConsentLevels = getAllConsentLevels;
    vm.saveConsentLevel = saveConsentLevel;
    vm.sliderChange = sliderChange;
    vm.setup = setup;
    vm.loading = true;
    vm.consentLevel = '';
    vm.consentLevels = [];
    vm.val = 1;
    vm.sliderOptions =
      {
        "start": 1,
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
          format: {
            to: function (value) {
              return vm.consentLevels[value - 1];
            },
            from: function (value) {
              return Number(value);
            }
          }
        }
      };
    $q.all(
      [getConsentLevel(),
      getAllConsentLevels()
    ]).then( () => {
      vm.startPos = calculateStartPositionForSlider();
      vm.sliderVal = vm.consentLevels[vm.startPos - 1];
      vm.sliderOptions.start = vm.startPos;
      vm.val = vm.startPos;
      vm.sliderOptions.pips.format = {
        to: function (value) {
          return vm.consentLevels[value - 1];
        },
        from: function (value) {
          return Number(value);
        }
      }
      vm.loading = false;
      if (sliderRef) {
        sliderRef.noUiSlider.set(vm.startPos);
      }

    });

    function setup(slider) {
      sliderRef = slider;
    }

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
      localizationService.localize("analytics_analyticsLevelSavedSuccess").then(function(value) {
        notificationsService.success(value);
      });
    }

    function sliderChange(values) {
      const result = Math.round(Number(values[0]) - 1);
      vm.sliderVal = vm.consentLevels[result];
    }

    function calculateStartPositionForSlider(){
      let startPosition = vm.consentLevels.indexOf(vm.consentLevel) + 1;
      if (startPosition === 0) {
         return 2;// Default start value
      }
      return startPosition;
    }
  }
    angular.module("umbraco").controller("Umbraco.Dashboard.AnalyticsController", AnalyticsController);
  })();
