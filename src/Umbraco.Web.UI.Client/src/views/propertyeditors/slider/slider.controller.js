function sliderController($scope, $log, $element, assetsService, angularHelper) {

    //configure some defaults
    if (!$scope.model.config.orientation) {
        $scope.model.config.orientation = "horizontal";
    }
    if (!$scope.model.config.initVal1) {
        $scope.model.config.initVal1 = 0;
    }
    else {
        $scope.model.config.initVal1 = parseFloat($scope.model.config.initVal1);
    }
    if (!$scope.model.config.initVal2) {
        $scope.model.config.initVal2 = 0;
    }
    else {
        $scope.model.config.initVal2 = parseFloat($scope.model.config.initVal2);
    }
    if (!$scope.model.config.minVal) {
        $scope.model.config.minVal = 0;
    }
    else {
        $scope.model.config.minVal = parseFloat($scope.model.config.minVal);
    }
    if (!$scope.model.config.maxVal) {
        $scope.model.config.maxVal = 100;
    }
    else {
        $scope.model.config.maxVal = parseFloat($scope.model.config.maxVal);
    }
    if (!$scope.model.config.step) {
        $scope.model.config.step = 1;
    }
    else {
        $scope.model.config.step = parseFloat($scope.model.config.step);
    }
    
    /** This creates the slider with the model values - it's called on startup and if the model value changes */
    function createSlider() {

        //the value that we'll give the slider - if it's a range, we store our value as a comma separated val but this slider expects an array
        var sliderVal = null;

        //configure the model value based on if range is enabled or not
        if ($scope.model.config.enableRange === "1") {
            //If no value saved yet - then use default value
            if (!$scope.model.value) {
                var i1 = parseFloat($scope.model.config.initVal1);
                var i2 = parseFloat($scope.model.config.initVal2);
                sliderVal = [
                    isNaN(i1) ? $scope.model.config.minVal : (i1 >= $scope.model.config.minVal ? i1 : $scope.model.config.minVal),
                    isNaN(i2) ? $scope.model.config.maxVal : (i2 > i1 ? (i2 <= $scope.model.config.maxVal ? i2 : $scope.model.config.maxVal) : $scope.model.config.maxVal)
                ];
            }
            else {
                //this will mean it's a delimited value stored in the db, convert it to an array
                sliderVal = _.map($scope.model.value.split(','), function (item) {
                    return parseFloat(item);
                });
            }
        }
        else {
            //If no value saved yet - then use default value
            if ($scope.model.value) {
                sliderVal = parseFloat($scope.model.value);
            }
            else {
                sliderVal = $scope.model.config.initVal1;
            }
        }

        // Initialise model value if not set
        if (!$scope.model.value) {
            setModelValueFromSlider(sliderVal);
        }

        //initiate slider, add event handler and get the instance reference (stored in data)
        var slider = $element.find('.slider-item').slider({
            max: $scope.model.config.maxVal,
            min: $scope.model.config.minVal,
            orientation: $scope.model.config.orientation,
            selection: "after",
            step: $scope.model.config.step,
            tooltip: "show",
            //set the slider val - we cannot do this with data- attributes when using ranges
            value: sliderVal
        }).on('slideStop', function () {
            angularHelper.safeApply($scope, function () {
                setModelValueFromSlider(slider.getValue());
            });
        }).data('slider');
    }

    /** Called on start-up when no model value has been applied and on change of the slider via the UI - updates
        the model with the currently selected slider value(s) **/
    function setModelValueFromSlider(sliderVal) {
        //Get the value from the slider and format it correctly, if it is a range we want a comma delimited value
        if ($scope.model.config.enableRange === "1") {
            $scope.model.value = sliderVal.join(",");
        }
        else {
            $scope.model.value = sliderVal.toString();
        }
    }

    //tell the assetsService to load the bootstrap slider
    //libs from the plugin folder
    assetsService
        .loadJs("lib/slider/js/bootstrap-slider.js")
        .then(function () {

            createSlider();
            
            //here we declare a special method which will be called whenever the value has changed from the server
            //this is instead of doing a watch on the model.value = faster
            $scope.model.onValueChanged = function (newVal, oldVal) {                
                if (newVal != oldVal) {
                    createSlider();
                }
            };

        });

    //load the separate css for the editor to avoid it blocking our js loading
    assetsService.loadCss("lib/slider/slider.css");

}
angular.module("umbraco").controller("Umbraco.PropertyEditors.SliderController", sliderController);