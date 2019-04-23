function sliderController($scope, angularHelper) {

    let sliderRef = null;

    /** configure some defaults on init */
    function configureDefaults() {
        $scope.model.config.enableRange = $scope.model.config.enableRange ? Object.toBoolean($scope.model.config.enableRange) : false;
        $scope.model.config.initVal1 = $scope.model.config.initVal1 ? parseFloat($scope.model.config.initVal1) : 0;
        $scope.model.config.initVal2 = $scope.model.config.initVal2 ? parseFloat($scope.model.config.initVal2) : 0;
        $scope.model.config.minVal = $scope.model.config.minVal ? parseFloat($scope.model.config.minVal) : 0;
        $scope.model.config.maxVal = $scope.model.config.maxVal ? parseFloat($scope.model.config.maxVal) : 100;
        $scope.model.config.step = $scope.model.config.step ? parseFloat($scope.model.config.step) : 1;
    }

    function setModelValue(values) {
        $scope.model.value = values ? values.toString() : null;
        angularHelper.getCurrentForm($scope).$setDirty();
    }

    $scope.setup = function(slider) {
        sliderRef = slider;
    };

    $scope.change = function (values) {
        setModelValue(values);
    };

    function init() {

        // convert to array
        $scope.sliderValue = $scope.model.value ? $scope.model.value.split(',') : null;

        configureDefaults();

        // format config to fit slider plugin
        const start = $scope.model.config.enableRange ? [$scope.model.config.initVal1, $scope.model.config.initVal2] : [$scope.model.config.initVal1];
        const step = $scope.model.config.step;
        const tooltips = $scope.model.config.enableRange ? [true, true] : [true];
        const min = $scope.model.config.minVal ? [$scope.model.config.minVal] : [$scope.model.config.minVal];
        const max = $scope.model.config.maxVal ? [$scope.model.config.maxVal] : [$scope.model.config.maxVal];
        // don't render values with decimal places if the step increment in a whole number
        var stepDecimalPlaces = $scope.model.config.step % 1 == 0
            ? 0
            : _.last($scope.model.config.step.toString().replace(",", ".").split(".")).length;
        // setup default
        $scope.sliderOptions = {
            "start": start,
            "step": step,
            "tooltips": tooltips,
            "format": {
                to: function (value) {
                    return value.toFixed(stepDecimalPlaces);
                },
                from: function (value) {
                    return value;
                }
            },
            "range": {
                "min": min,
                "max": max
            },
            "pips": {
                mode: 'steps',
                density: 100,
                filter: filterPips
            }
        };

        function filterPips(value) {
            // show a pip for min and maximum value
            return value === $scope.model.config.minVal || value === $scope.model.config.maxVal ? 1 : -1;
        }

    }
    
    $scope.$watch('model.value', function(newValue, oldValue){
        if(newValue && newValue !== oldValue) {
            $scope.sliderValue = newValue.split(',');
            sliderRef.noUiSlider.set($scope.sliderValue);
        }
    })

    init();

}
angular.module("umbraco").controller("Umbraco.PropertyEditors.SliderController", sliderController);
