function dateTimePickerController($scope, notificationsService, assetsService, angularHelper, userService, $element, dateHelper) {

    let flatPickr = null;

    function onInit() {

        $scope.hasDatetimePickerValue = $scope.model.value ? true : false;
        $scope.model.datetimePickerValue = null;
        $scope.serverTime = null;
        $scope.serverTimeNeedsOffsetting = false;

        // setup the default config
        var config = {
            pickDate: true,
            pickTime: true,
            useSeconds: true,
            format: "YYYY-MM-DD HH:mm:ss",
            icons: {
                time: "icon-time",
                date: "icon-calendar",
                up: "icon-chevron-up",
                down: "icon-chevron-down"
            }
        };

        // map the user config
        $scope.model.config = angular.extend(config, $scope.model.config);
        
        // ensure the format doesn't get overwritten with an empty string
        if ($scope.model.config.format === "" || $scope.model.config.format === undefined || $scope.model.config.format === null) {
            $scope.model.config.format = $scope.model.config.pickTime ? "YYYY-MM-DD HH:mm:ss" : "YYYY-MM-DD";
        }

        // check whether a server time offset is needed
        if (Umbraco.Sys.ServerVariables.application.serverTimeOffset !== undefined) {
            // Will return something like 120
            var serverOffset = Umbraco.Sys.ServerVariables.application.serverTimeOffset;
    
            // Will return something like -120
            var localOffset = new Date().getTimezoneOffset();
    
            // If these aren't equal then offsetting is needed
            // note the minus in front of serverOffset needed 
            // because C# and javascript return the inverse offset
            $scope.serverTimeNeedsOffsetting = (-serverOffset !== localOffset);
        }

        const dateFormat = $scope.model.config.pickTime ? "Y-m-d H:i:S" : "Y-m-d";

        // date picker config
        $scope.datePickerConfig = {
            enableTime: $scope.model.config.pickTime,
            dateFormat: dateFormat,
            time_24hr: true
        };

        // Don't show calendar if date format has been set to only time
        if ($scope.model.config.format === "HH:mm:ss" || $scope.model.config.format === "HH:mm" || $scope.model.config.format === "HH") {
            $scope.datePickerConfig.enableTime = true;
            $scope.datePickerConfig.noCalendar = true;
        }
            
        setDatePickerVal();

    }

    $scope.clearDate = function() {
        $scope.hasDatetimePickerValue = false;
        if($scope.model) {
            $scope.model.datetimePickerValue = null;
            $scope.model.value = null;
        }
        if($scope.datePickerForm && $scope.datePickerForm.datepicker) {
            $scope.datePickerForm.datepicker.$setValidity("pickerError", true);
        }
    }

    $scope.datePickerSetup = function(instance) {
        flatPickr = instance;
    };

    $scope.datePickerChange = function(date) {
        setDate(date);
        setDatePickerVal();
    };

    $scope.inputChanged = function() {
        setDate($scope.model.datetimePickerValue);
        setDatePickerVal();
    }
    
    //here we declare a special method which will be called whenever the value has changed from the server
    //this is instead of doing a watch on the model.value = faster
    $scope.model.onValueChanged = function (newVal, oldVal) {
        if (newVal != oldVal) {
            //check for c# System.DateTime.MinValue being passed as the clear indicator
            var minDate = moment('0001-01-01');
            var newDate = moment(newVal);

            if (newDate.isAfter(minDate)) {
                setDate(newVal);
            } else {
                $scope.clearDate();
            }
        }
    };

    function setDate(date) {
        const momentDate = moment(date);
        angularHelper.safeApply($scope, function() {
            // when a date is changed, update the model
            if (momentDate && momentDate.isValid()) {
                $scope.datePickerForm.datepicker.$setValidity("pickerError", true);
                $scope.hasDatetimePickerValue = true;
                $scope.model.datetimePickerValue = momentDate.format($scope.model.config.format);
            }
            else {
                $scope.hasDatetimePickerValue = false;
                $scope.model.datetimePickerValue = null;
            }
            updateModelValue(date);
        });
    }

    function updateModelValue(date) {
        const momentDate = moment(date);
        if ($scope.hasDatetimePickerValue) {
            if ($scope.model.config.pickTime) {
                //check if we are supposed to offset the time
                if ($scope.model.value && Object.toBoolean($scope.model.config.offsetTime) && Umbraco.Sys.ServerVariables.application.serverTimeOffset !== undefined) {
                    $scope.model.value = dateHelper.convertToServerStringTime(momentDate, Umbraco.Sys.ServerVariables.application.serverTimeOffset);
                    $scope.serverTime = dateHelper.convertToServerStringTime(momentDate, Umbraco.Sys.ServerVariables.application.serverTimeOffset, "YYYY-MM-DD HH:mm:ss Z");
                }
                else {
                    $scope.model.value = momentDate.format("YYYY-MM-DD HH:mm:ss");
                }
            }
            else {
                $scope.model.value = momentDate.format("YYYY-MM-DD");
            }
        }
        else {
            $scope.model.value = null;
        }
        angularHelper.getCurrentForm($scope).$setDirty();
    }

    /** Sets the value of the date picker control adn associated viewModel objects based on the model value */
    function setDatePickerVal() {
        if ($scope.model.value) {
            var dateVal;
            //check if we are supposed to offset the time
            if ($scope.model.value && Object.toBoolean($scope.model.config.offsetTime) && $scope.serverTimeNeedsOffsetting) {
                //get the local time offset from the server
                dateVal = dateHelper.convertToLocalMomentTime($scope.model.value, Umbraco.Sys.ServerVariables.application.serverTimeOffset);
                $scope.serverTime = dateHelper.convertToServerStringTime(dateVal, Umbraco.Sys.ServerVariables.application.serverTimeOffset, "YYYY-MM-DD HH:mm:ss Z");
            }
            else {
                //create a normal moment , no offset required
                var dateVal = $scope.model.value ? moment($scope.model.value, "YYYY-MM-DD HH:mm:ss") : moment();
            }
            $scope.model.datetimePickerValue = dateVal.format($scope.model.config.format);
        }
        else {
            $scope.clearDate();
        }
    }

    $scope.$watch("model.value", function(newVal, oldVal) {
        if (newVal !== oldVal) {
            $scope.hasDatetimePickerValue = newVal ? true : false;
            setDatePickerVal();
        }
    });

    onInit();
    
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.DatepickerController", dateTimePickerController);
