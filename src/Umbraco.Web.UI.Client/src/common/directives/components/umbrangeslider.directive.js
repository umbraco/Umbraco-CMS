/**
@ngdoc directive
@name umbraco.directives.directive:umbRangeSlider
@restrict E
@scope

@description
<b>Added in Umbraco version 8.0</b>
This directive is a wrapper of the noUiSlider library. Use it to render a slider.
For extra details about options and events take a look here: https://refreshless.com/nouislider/

<h3>Markup example</h3>
<pre>
	<div ng-controller="My.Controller as vm">

        <umb-range-slider
            ng-model="vm.value"
            on-end="vm.slideEnd(values)">
        </umb-range-slider>

	</div>
</pre>

<h3>Controller example</h3>
<pre>
	(function () {
		"use strict";

		function Controller() {

            var vm = this;

            vm.value = [10];

            vm.slideEnd = slideEnd;

            function slideEnd(values) {
            	// handle change
            }

        }

		angular.module("umbraco").controller("My.Controller", Controller);

	})();
</pre>

@param {object} ngModel (<code>binding</code>): Value for the slider.
@param {object} options (<code>binding</code>): Config object for the slider.
@param {callback} onSetup (<code>callback</code>): onSetup gets triggered when the slider is initialized
@param {callback} onUpdate (<code>callback</code>): onUpdate fires every time the slider values are changed.
@param {callback} onSlide (<code>callback</code>): onSlide gets triggered when the handle is being dragged.
@param {callback} onSet (<code>callback</code>): onSet will trigger every time a slider stops changing.
@param {callback} onChange (<code>callback</code>): onChange fires when a user stops sliding, or when a slider value is changed by 'tap'.
@param {callback} onDrag (<code>callback</code>): onDrag fires when a connect element between handles is being dragged, while ignoring other updates to the slider values.
@param {callback} onStart (<code>callback</code>): onStart fires when a handle is clicked (mousedown, or the equivalent touch events).
@param {callback} onEnd (<code>callback</code>): onEnd fires when a handle is released (mouseup etc), or when a slide is canceled due to other reasons.
**/


(function () {
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
            onDrag: '&?',
            onStart: '&?',
            onEnd: '&?'
        }
    };

    function UmbRangeSliderController($element, $timeout, $scope, assetsService, $attrs) {

        const ctrl = this;
        let sliderInstance = null;

        ctrl.$onInit = function () {

            // load css file for the date picker
            assetsService.loadCss('lib/nouislider/nouislider.min.css', $scope);

            // load the js file for the date picker
            assetsService.loadJs('lib/nouislider/nouislider.min.js', $scope).then(function () {
                // init date picker
                grabElementAndRun();
            });

        };

        $attrs.$observe('readonly', (value) => {
            ctrl.readonly = value !== undefined;

            if (!sliderInstance) {
                return;
            }

            if (ctrl.readonly) {
                sliderInstance.setAttribute('disabled', true);
            } else {
                sliderInstance.removeAttribute('disabled');
            }
        });

        function grabElementAndRun() {
            $timeout(function () {
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

            mergeTooltips(sliderInstance, 15, ' - ');

            if (ctrl.onSetup) {
                ctrl.onSetup({
                    slider: sliderInstance
                });
            }

            // If has ngModel set the date
            if (ctrl.ngModel) {
                sliderInstance.noUiSlider.set(ctrl.ngModel);
            }

            if (ctrl.readonly) {
                sliderInstance.setAttribute('disabled', true);
            } else {
                sliderInstance.removeAttribute('disabled');
            }

            // destroy the slider instance when the dom element is removed
            $(element).on('$destroy', function () {
                sliderInstance.noUiSlider.off();
            });

            setUpCallbacks();
            setUpActivePipsHandling();
            addPipClickHandler();

            // Refresh the scope
            $scope.$applyAsync();
        }

        function setUpCallbacks() {
            if (sliderInstance) {

                // bind hook for update
                if (ctrl.onUpdate) {
                    sliderInstance.noUiSlider.on('update', function (values, handle, unencoded, tap, positions) {
                        $timeout(function () {
                            ctrl.onUpdate({ values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions });
                        });
                    });
                }

                // bind hook for slide
                if (ctrl.onSlide) {
                    sliderInstance.noUiSlider.on('slide', function (values, handle, unencoded, tap, positions) {
                        $timeout(function () {
                            ctrl.onSlide({ values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions });
                        });
                    });
                }

                // bind hook for set
                if (ctrl.onSet) {
                    sliderInstance.noUiSlider.on('set', function (values, handle, unencoded, tap, positions) {
                        $timeout(function () {
                            ctrl.onSet({ values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions });
                        });
                    });
                }

                // bind hook for change
                if (ctrl.onChange) {
                    sliderInstance.noUiSlider.on('change', function (values, handle, unencoded, tap, positions) {
                        $timeout(function () {
                            ctrl.onChange({ values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions });
                        });
                    });
                }

                // bind hook for drag
                if (ctrl.onDrag) {
                    sliderInstance.noUiSlider.on('drag', function (values, handle, unencoded, tap, positions) {
                        $timeout(function () {
                            ctrl.onDrag({ values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions });
                        });
                    });
                }

                // bind hook for start
                if (ctrl.onStart) {
                    sliderInstance.noUiSlider.on('start', function (values, handle, unencoded, tap, positions) {
                        $timeout(function () {
                            ctrl.onStart({ values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions });
                        });
                    });
                }

                // bind hook for end
                if (ctrl.onEnd) {
                    sliderInstance.noUiSlider.on('end', function (values, handle, unencoded, tap, positions) {
                        $timeout(function () {
                            ctrl.onEnd({ values: values, handle: handle, unencoded: unencoded, tap: tap, positions: positions });
                        });
                    });
                }

            }
        }

        // Merging overlapping tooltips: https://refreshless.com/nouislider/examples/#section-merging-tooltips

        /**
         * @param slider HtmlElement with an initialized slider
         * @param threshold Minimum proximity (in percentages) to merge tooltips
         * @param separator String joining tooltips
         */
        function mergeTooltips(slider, threshold, separator) {

            var textIsRtl = getComputedStyle(slider).direction === 'rtl';
            var isRtl = slider.noUiSlider.options.direction === 'rtl';
            var isVertical = slider.noUiSlider.options.orientation === 'vertical';
            var tooltips = slider.noUiSlider.getTooltips();
            var origins = slider.noUiSlider.getOrigins();
            
            // Move tooltips into the origin element. The default stylesheet handles this.
            if(tooltips && tooltips.length !== 0){
              tooltips.forEach(function (tooltip, index) {
                if (tooltip) {
                  origins[index].appendChild(tooltip);
                }
              });
            }

            slider.noUiSlider.on('update', function (values, handle, unencoded, tap, positions) {

                var pools = [[]];
                var poolPositions = [[]];
                var poolValues = [[]];
                var atPool = 0;

                // Assign the first tooltip to the first pool, if the tooltip is configured
                if (tooltips[0]) {
                    pools[0][0] = 0;
                    poolPositions[0][0] = positions[0];
                    poolValues[0][0] = values[0];
                }

                for (var i = 1; i < positions.length; i++) {
                    if (!tooltips[i] || (positions[i] - positions[i - 1]) > threshold) {
                        atPool++;
                        pools[atPool] = [];
                        poolValues[atPool] = [];
                        poolPositions[atPool] = [];
                    }

                    if (tooltips[i]) {
                        pools[atPool].push(i);
                        poolValues[atPool].push(values[i]);
                        poolPositions[atPool].push(positions[i]);
                    }
                }

                pools.forEach(function (pool, poolIndex) {
                    var handlesInPool = pool.length;

                    for (var j = 0; j < handlesInPool; j++) {
                        var handleNumber = pool[j];
                        
                        if (j === handlesInPool - 1) {
                            var offset = 0;
                            
                            poolPositions[poolIndex].forEach(function (value) {
                                offset += 1000 - value;
                            });
                            
                            var direction = isVertical ? 'bottom' : 'right';
                            var last = isRtl ? 0 : handlesInPool - 1;
                            var lastOffset = 1000 - poolPositions[poolIndex][last];
                            offset = (textIsRtl && !isVertical ? 100 : 0) + (offset / handlesInPool) - lastOffset;

                            // Filter to unique values
                            var tooltipValues = poolValues[poolIndex].filter((v, i, a) => a.indexOf(v) === i);

                            // Center this tooltip over the affected handles
                            tooltips[handleNumber].innerHTML = tooltipValues.join(separator);
                            tooltips[handleNumber].style.display = 'block';
                            tooltips[handleNumber].style[direction] = offset + '%';
                        } else {
                            // Hide this tooltip
                            tooltips[handleNumber].style.display = 'none';
                        }
                    }
                });
            });
        }
      function setUpActivePipsHandling() {
        sliderInstance.noUiSlider.on('update', function (values,handle) {
          sliderInstance.querySelectorAll('.noUi-value').forEach(pip => {
            pip.classList.remove("noUi-value-active");
            if (Number(values[handle]) === Number(pip.getAttribute('data-value'))) {
              pip.classList.add("noUi-value-active");
            }
          });
        });
      }
      function addPipClickHandler(){
          sliderInstance.querySelectorAll('.noUi-value').forEach(function(pip){
            pip.addEventListener('click', function () {
              const value = pip.getAttribute('data-value');
              sliderInstance.noUiSlider.set(value);
            });
          });
      }
    }

    angular.module('umbraco.directives').component('umbRangeSlider', umbRangeSlider);

})();
