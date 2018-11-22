(function() {
	'use strict';

	var umbRangeSlider = {
        template: '<div class="umb-range-slider"></div>',
		controller: UmbRangeSliderController,
		bindings: {
            ngModel: '<',
            options: '<',
            onSetup: '&?',
            onUpdate: '&?',
            onSlide: '&?',
            onSet: '&?',
            onChange: '&?',
            onStart: '&?',
            onEnd: '&?'
		}
    };
    
	function UmbRangeSliderController($element, $timeout, $scope, assetsService) {
        
        const ctrl = this;
        let sliderInstance = null;

		ctrl.$onInit = function() {

            // load css file for the date picker
            assetsService.loadCss('lib/nouislider/nouislider.min.css', $scope);

            // load the js file for the date picker
            assetsService.loadJs('lib/nouislider/nouislider.min.js', $scope).then(function () {
                // init date picker
                grabElementAndRun();
            });

        };

		function grabElementAndRun() {
			$timeout(function() {
                const element = $element.find('.umb-range-slider')[0];
				setSlider(element);
			}, 0, true);
        }
        
        function setSlider(element) {

            sliderInstance = element;

            const defaultOptions = {
                "start": [0],
                "step": 1,
                "range": {
                    "min": [0],
                    "max": [100]
                }
            };
            const options = ctrl.options ? ctrl.options : defaultOptions;

            // create new slider
            noUiSlider.create(sliderInstance, options);
            
			if (ctrl.onSetup) {
				ctrl.onSetup({
					slider: sliderInstance
				});
            }

            // If has ngModel set the date
			if (ctrl.ngModel) {
                sliderInstance.noUiSlider.set(ctrl.ngModel);
            }

            // destroy the flatpickr instance when the dom element is removed
			angular.element(element).on('$destroy', function() {
                sliderInstance.noUiSlider.off();
            });

            setUpCallbacks();

			// Refresh the scope
			$scope.$applyAsync();
        }
        
        function setUpCallbacks() {
			if(sliderInstance) {

                // bind hook for update
                if(ctrl.onUpdate) {
                    sliderInstance.noUiSlider.on('update', function (values, handle, unencoded, tap, positions) { 
                        $timeout(function() {
                            ctrl.onUpdate({values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions});
                        });
                    });
                }

                // bind hook for slide
                if(ctrl.onSlide) {
                    sliderInstance.noUiSlider.on('slide', function (values, handle, unencoded, tap, positions) { 
                        $timeout(function() {
                            ctrl.onSlide({values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions});
                        });
                    });
                }

                // bind hook for set
                if(ctrl.onSet) {
                    sliderInstance.noUiSlider.on('set', function (values, handle, unencoded, tap, positions) { 
                        $timeout(function() {
                            ctrl.onSet({values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions});
                        });
                    });
                }

                // bind hook for change
                if(ctrl.onChange) {
                    sliderInstance.noUiSlider.on('change', function (values, handle, unencoded, tap, positions) { 
                        $timeout(function() {
                            ctrl.onChange({values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions});
                        });
                    });
                }

                // bind hook for start
                if(ctrl.onStart) {
                    sliderInstance.noUiSlider.on('start', function (values, handle, unencoded, tap, positions) { 
                        $timeout(function() {
                            ctrl.onStart({values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions});
                        });
                    });
                }

                // bind hook for end
                if(ctrl.onEnd) {
                    sliderInstance.noUiSlider.on('end', function (values, handle, unencoded, tap, positions) { 
                        $timeout(function() {
                            ctrl.onEnd({values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions});
                        });
                    });
                }

            }
        }

    }
    
    angular.module('umbraco.directives').component('umbRangeSlider', umbRangeSlider);
    
})();