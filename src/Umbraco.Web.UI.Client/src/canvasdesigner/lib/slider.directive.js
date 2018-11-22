
/*********************************************************************************************************/
/* jQuery UI Slider plugin wrapper */
/*********************************************************************************************************/

angular.module('ui.slider', []).value('uiSliderConfig', {}).directive('uiSlider', ['uiSliderConfig', '$timeout', function (uiSliderConfig, $timeout) {
    uiSliderConfig = uiSliderConfig || {};
    return {
        require: 'ngModel',
        template: '<div><div class="slider" /><input class="slider-input" style="display:none" ng-model="value"></div>',
        replace: true,
        compile: function () {
            return function (scope, elm, attrs, ngModel) {

                scope.value = ngModel.$viewValue;

                function parseNumber(n, decimals) {
                    return (decimals) ? parseFloat(n) : parseInt(n);
                };

                var options = angular.extend(scope.$eval(attrs.uiSlider) || {}, uiSliderConfig);
                // Object holding range values
                var prevRangeValues = {
                    min: null,
                    max: null
                };

                // convenience properties
                var properties = ['min', 'max', 'step'];
                var useDecimals = (!angular.isUndefined(attrs.useDecimals)) ? true : false;

                var init = function () {
                    // When ngModel is assigned an array of values then range is expected to be true.
                    // Warn user and change range to true else an error occurs when trying to drag handle
                    if (angular.isArray(ngModel.$viewValue) && options.range !== true) {
                        console.warn('Change your range option of ui-slider. When assigning ngModel an array of values then the range option should be set to true.');
                        options.range = true;
                    }

                    // Ensure the convenience properties are passed as options if they're defined
                    // This avoids init ordering issues where the slider's initial state (eg handle
                    // position) is calculated using widget defaults
                    // Note the properties take precedence over any duplicates in options
                    angular.forEach(properties, function (property) {
                        if (angular.isDefined(attrs[property])) {
                            options[property] = parseNumber(attrs[property], useDecimals);
                        }
                    });

                    elm.find(".slider").slider(options);
                    init = angular.noop;
                };

                // Find out if decimals are to be used for slider
                angular.forEach(properties, function (property) {
                    // support {{}} and watch for updates
                    attrs.$observe(property, function (newVal) {
                        if (!!newVal) {
                            init();
                            elm.find(".slider").slider('option', property, parseNumber(newVal, useDecimals));
                        }
                    });
                });
                attrs.$observe('disabled', function (newVal) {
                    init();
                    elm.find(".slider").slider('option', 'disabled', !!newVal);
                });

                // Watch ui-slider (byVal) for changes and update
                scope.$watch(attrs.uiSlider, function (newVal) {
                    init();
                    if (newVal != undefined) {
                        elm.find(".slider").slider('option', newVal);
                        elm.find(".ui-slider-handle").html("<span>" + ui.value + "px</span>")
                    }
                }, true);

                // Late-bind to prevent compiler clobbering
                $timeout(init, 0, true);

                // Update model value from slider
                elm.find(".slider").bind('slidestop', function (event, ui) {
                    ngModel.$setViewValue(ui.values || ui.value);
                    scope.$apply();
                });

                elm.bind('slide', function (event, ui) {
                    event.stopPropagation();
                    elm.find(".slider-input").val(ui.value);
                    elm.find(".ui-slider-handle").html("<span>" + ui.value + "px</span>")
                });

                // Update slider from model value
                ngModel.$render = function () {
                    init();
                    var method = options.range === true ? 'values' : 'value';

                    if (isNaN(ngModel.$viewValue) && !(ngModel.$viewValue instanceof Array))
                        ngModel.$viewValue = 0;

                    if (ngModel.$viewValue == '')
                        ngModel.$viewValue = 0;

                    scope.value = ngModel.$viewValue;

                    // Do some sanity check of range values
                    if (options.range === true) {

                        // Check outer bounds for min and max values
                        if (angular.isDefined(options.min) && options.min > ngModel.$viewValue[0]) {
                            ngModel.$viewValue[0] = options.min;
                        }
                        if (angular.isDefined(options.max) && options.max < ngModel.$viewValue[1]) {
                            ngModel.$viewValue[1] = options.max;
                        }

                        // Check min and max range values
                        if (ngModel.$viewValue[0] >= ngModel.$viewValue[1]) {
                            // Min value should be less to equal to max value
                            if (prevRangeValues.min >= ngModel.$viewValue[1])
                                ngModel.$viewValue[0] = prevRangeValues.min;
                            // Max value should be less to equal to min value
                            if (prevRangeValues.max <= ngModel.$viewValue[0])
                                ngModel.$viewValue[1] = prevRangeValues.max;
                        }



                        // Store values for later user
                        prevRangeValues.min = ngModel.$viewValue[0];
                        prevRangeValues.max = ngModel.$viewValue[1];

                    }
                    elm.find(".slider").slider(method, ngModel.$viewValue);
                    elm.find(".ui-slider-handle").html("<span>" + ngModel.$viewValue + "px</span>")
                };

                scope.$watch("value", function () {
                    ngModel.$setViewValue(scope.value);
                }, true);

                scope.$watch(attrs.ngModel, function () {
                    if (options.range === true) {
                        ngModel.$render();
                    }
                }, true);

                function destroy() {
                    elm.find(".slider").slider('destroy');
                }
                elm.find(".slider").bind('$destroy', destroy);
            };
        }
    };
}]);

