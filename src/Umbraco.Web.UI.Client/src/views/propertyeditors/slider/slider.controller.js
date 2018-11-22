function sliderController($scope, $log, $element, assetsService, angularHelper) {

    let sliderRef = null;

    /** configure some defaults on init */
    function configureDefaults() {

        $scope.model.config.orientation = $scope.model.config.orientation ? $scope.model.config.orientation : "horizontal";
        $scope.model.config.enableRange = $scope.model.config.enableRange ? Object.toBoolean($scope.model.config.enableRange) : false;
        $scope.model.config.initVal1 = $scope.model.config.initVal1 ? parseFloat($scope.model.config.initVal1) : 0;
        $scope.model.config.initVal2 = $scope.model.config.initVal2 ? parseFloat($scope.model.config.initVal2) : 0;
        $scope.model.config.minVal = $scope.model.config.minVal ? parseFloat($scope.model.config.minVal) : 0;
        $scope.model.config.maxVal = $scope.model.config.maxVal ? parseFloat($scope.model.config.maxVal) : 100;
        $scope.model.config.step = $scope.model.config.step ? parseFloat($scope.model.config.step) : 1;


        if (!$scope.model.config.handle) {
            $scope.model.config.handle = "round";
        }

        if (!$scope.model.config.reversed) {
            $scope.model.config.reversed = false;
        }
        else {
            $scope.model.config.reversed = Object.toBoolean($scope.model.config.reversed);
        }

        if (!$scope.model.config.tooltip) {
            $scope.model.config.tooltip = "show";
        }

        if (!$scope.model.config.tooltipSplit) {
            $scope.model.config.tooltipSplit = false;
        }
        else {
            $scope.model.config.tooltipSplit = Object.toBoolean($scope.model.config.tooltipSplit);
        }

        if ($scope.model.config.tooltipFormat) {
            $scope.model.config.formatter = function (value) {
                if (angular.isArray(value) && $scope.model.config.enableRange) {
                    return $scope.model.config.tooltipFormat.replace("{0}", value[0]).replace("{1}", value[1]);
                } else {
                    return $scope.model.config.tooltipFormat.replace("{0}", value);
                }
            }
        }

        if (!$scope.model.config.ticks) {
            $scope.model.config.ticks = [];
        }
        else if (angular.isString($scope.model.config.ticks)) {
            // returns comma-separated string to an array, e.g. [0, 100, 200, 300, 400]
            $scope.model.config.ticks = _.map($scope.model.config.ticks.split(','), function (item) {
                return parseInt(item.trim());
            });
        }

        if (!$scope.model.config.ticksPositions) {
            $scope.model.config.ticksPositions = [];
        }
        else if (angular.isString($scope.model.config.ticksPositions)) {
            // returns comma-separated string to an array, e.g. [0, 30, 60, 70, 90, 100]
            $scope.model.config.ticksPositions = _.map($scope.model.config.ticksPositions.split(','), function (item) {
                return parseInt(item.trim());
            });
        }

        if (!$scope.model.config.ticksLabels) {
            $scope.model.config.ticksLabels = [];
        }
        else if (angular.isString($scope.model.config.ticksLabels)) {
            // returns comma-separated string to an array, e.g. ['$0', '$100', '$200', '$300', '$400']
            $scope.model.config.ticksLabels = _.map($scope.model.config.ticksLabels.split(','), function (item) {
                return item.trim();
            });
        }

        if (!$scope.model.config.ticksSnapBounds) {
            $scope.model.config.ticksSnapBounds = 0;
        }
        else {
            $scope.model.config.ticksSnapBounds = parseFloat($scope.model.config.ticksSnapBounds);
        }
    }

    function getValueForSlider(val) {
        
        if (!angular.isArray(val)) {
            val = val.toString().split(",");
        }
        var val1 = val[0];
        var val2 = val.length > 1 ? val[1] : null;
        
        //configure the model value based on if range is enabled or not
        if ($scope.model.config.enableRange == true) {
            var i1 = parseFloat(val1);
            var i2 = parseFloat(val2);
            return [
                isNaN(i1) ? $scope.model.config.minVal : (i1 >= $scope.model.config.minVal ? i1 : $scope.model.config.minVal),
                isNaN(i2) ? $scope.model.config.maxVal : (i2 >= i1 ? (i2 <= $scope.model.config.maxVal ? i2 : $scope.model.config.maxVal) : $scope.model.config.maxVal)
            ];
        }
        else {
            return parseFloat(val1);
        }
    }

    /** This creates the slider with the model values - it's called on startup and returns a reference to the slider object */
    function createSlider() {

        //the value that we'll give the slider - if it's a range, we store our value as a comma separated val but this slider expects an array
        var sliderVal = null;

        //configure the model value based on if range is enabled or not
        if ($scope.model.config.enableRange == true) {
            //If no value saved yet - then use default value
            //If it contains a single value - then also create a new array value
            if (!$scope.model.value || $scope.model.value.indexOf(",") == -1) {
                sliderVal = getValueForSlider([$scope.model.config.initVal1, $scope.model.config.initVal2]);
            }
            else {
                //this will mean it's a delimited value stored in the db, convert it to an array
                sliderVal = getValueForSlider($scope.model.value.split(','));
            }
        }
        else {
            //If no value saved yet - then use default value
            if ($scope.model.value) {
                sliderVal = getValueForSlider($scope.model.value);
            }
            else {
                sliderVal = getValueForSlider($scope.model.config.initVal1);
            }
        }

        //initiate slider, add event handler and get the instance reference (stored in data)
        var slider = $element.find('.slider-item').bootstrapSlider({
            max: $scope.model.config.maxVal,
            min: $scope.model.config.minVal,
            orientation: $scope.model.config.orientation,
            selection: $scope.model.config.reversed ? "after" : "before",
            step: $scope.model.config.step,
            precision: $scope.model.config.precision,
            tooltip: $scope.model.config.tooltip,
            tooltip_split: $scope.model.config.tooltipSplit,
            tooltip_position: $scope.model.config.tooltipPosition,
            handle: $scope.model.config.handle,
            reversed: $scope.model.config.reversed,
            ticks: $scope.model.config.ticks,
            ticks_positions: $scope.model.config.ticksPositions,
            ticks_labels: $scope.model.config.ticksLabels,
            ticks_snap_bounds: $scope.model.config.ticksSnapBounds,
            formatter: $scope.model.config.formatter,
            range: $scope.model.config.enableRange,
            //set the slider val - we cannot do this with data- attributes when using ranges
            value: sliderVal
        });

        slider.on('slideStop', function (e) {
            var value = e.value;
            angularHelper.safeApply($scope, function () {
                $scope.model.value = getModelValueFromSlider(value);
            });
        }).data('slider');

        return slider;
    }

    function getModelValueFromSlider(sliderVal) {
        //Get the value from the slider and format it correctly, if it is a range we want a comma delimited value
        if ($scope.model.config.enableRange == true) {
            return sliderVal.join(",");
        }
        else {
            return sliderVal.toString();
        }
    }

    function setModelValue(values) {
        $scope.model.value = values.toString();
    }

    $scope.setup = function(slider) {
        sliderRef = slider;
    };

    $scope.update = function(values) {
        setModelValue(values);
    };

    function init() {

        configureDefaults();

        // format config to fit slider plugin
        const start = $scope.model.config.enableRange ? [$scope.model.config.initVal1, $scope.model.config.initVal2] : [$scope.model.config.initVal1];
        const step = $scope.model.config.step;
        const tooltips = $scope.model.config.enableRange ? [true, true] : [true];
        const min = $scope.model.config.minVal ? [$scope.model.config.minVal] : [$scope.model.config.minVal];
        const max = $scope.model.config.maxVal ? [$scope.model.config.maxVal] : [$scope.model.config.maxVal];

        // setup default
        $scope.sliderOptions = {
            "start": start,
            "step": step,
            "tooltips": tooltips,
            "format": {
                to: function (value) {
                    return Math.round(value);
                },
                from: function (value) {
                    return Math.round(value);
                }
            },
            "range": {
                "min": min,
                "max": max
            }
        };

        //tell the assetsService to load the bootstrap slider
        //libs from the plugin folder
        assetsService
            .loadJs("lib/slider/js/bootstrap-slider.js")
            .then(function () {

                var slider = createSlider();

                // Initialize model value if not set
                if (!$scope.model.value) {
                    var sliderVal = slider.bootstrapSlider('getValue');
                    $scope.model.value = getModelValueFromSlider(sliderVal);
                }

                //watch for the model value being changed and update the slider value when it does
                $scope.$watch("model.value", function (newVal, oldVal) {
                    if (newVal != oldVal) {
                        var sliderVal = getModelValueFromSlider(slider.bootstrapSlider('getValue'));
                        if (newVal !== sliderVal) {
                            slider.bootstrapSlider('setValue', getValueForSlider(newVal));
                        }
                    }
                });

            });

        //load the separate css for the editor to avoid it blocking our js loading
        assetsService.loadCss("lib/slider/bootstrap-slider.css", $scope);
        assetsService.loadCss("lib/slider/bootstrap-slider-custom.css", $scope);
    }

    init();

}
angular.module("umbraco").controller("Umbraco.PropertyEditors.SliderController", sliderController);
