function dateTimePickerController($scope, notificationsService, assetsService, angularHelper, userService, $element, dateHelper) {

    //setup the default config
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

    //map the user config
    $scope.model.config = angular.extend(config, $scope.model.config);
    //ensure the format doesn't get overwritten with an empty string
    if ($scope.model.config.format === "" || $scope.model.config.format === undefined || $scope.model.config.format === null) {
        $scope.model.config.format = $scope.model.config.pickTime ? "YYYY-MM-DD HH:mm:ss" : "YYYY-MM-DD";
    }

    $scope.hasDatetimePickerValue = $scope.model.value ? true : false;
    $scope.datetimePickerValue = null;

    //hide picker if clicking on the document 
    $scope.hidePicker = function () {
        //$element.find("div:first").datetimepicker("hide");
        // Sometimes the statement above fails and generates errors in the browser console. The following statements fix that.
        var dtp = $element.find("div:first");
        if (dtp && dtp.datetimepicker) {
            dtp.datetimepicker("hide");
        }
    };
    
    //here we declare a special method which will be called whenever the value has changed from the server
    //this is instead of doing a watch on the model.value = faster
    $scope.model.onValueChanged = function (newVal, oldVal) {
        if (newVal != oldVal) {
            //check for c# System.DateTime.MinValue being passed as the clear indicator
            var minDate = moment('0001-01-01');
            var newDate = moment(newVal);

            if (newDate.isAfter(minDate)) {
                applyDate({ date: moment(newVal) });
            } else {
                $scope.clearDate();
            }
        }
    };
    
    //handles the date changing via the date picker
    function applyDate(e) {
        angularHelper.safeApply($scope, function() {
            // when a date is changed, update the model
            if (e.date && e.date.isValid()) {
                $scope.datePickerForm.datepicker.$setValidity("pickerError", true);
                $scope.hasDatetimePickerValue = true;
                $scope.datetimePickerValue = e.date.format($scope.model.config.format);                
            }
            else {
                $scope.hasDatetimePickerValue = false;
                $scope.datetimePickerValue = null;
            }

            setModelValue();

            if (!$scope.model.config.pickTime) {
                $element.find("div:first").datetimepicker("hide", 0);
            }
        });
    }

    //sets the scope model value accordingly - this is the value to be sent up to the server and depends on 
    // if the picker is configured to offset time. We always format the date/time in a specific format for sending
    // to the server, this is different from the format used to display the date/time.
    function setModelValue() {
        if ($scope.hasDatetimePickerValue) {
            var elementData = $element.find("div:first").data().DateTimePicker;
            if ($scope.model.config.pickTime) {
                //check if we are supposed to offset the time
                if ($scope.model.value && Object.toBoolean($scope.model.config.offsetTime) && Umbraco.Sys.ServerVariables.application.serverTimeOffset !== undefined) {
                    $scope.model.value = dateHelper.convertToServerStringTime(elementData.getDate(), Umbraco.Sys.ServerVariables.application.serverTimeOffset);
                    $scope.serverTime = dateHelper.convertToServerStringTime(elementData.getDate(), Umbraco.Sys.ServerVariables.application.serverTimeOffset, "YYYY-MM-DD HH:mm:ss Z");
                }
                else {
                    $scope.model.value = elementData.getDate().format("YYYY-MM-DD HH:mm:ss");
                }
            }
            else {
                $scope.model.value = elementData.getDate().format("YYYY-MM-DD");
            }
        }
        else {
            $scope.model.value = null;
        }
    }

    /** Sets the value of the date picker control adn associated viewModel objects based on the model value */
    function setDatePickerVal(element) {
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

            element.datetimepicker("setValue", dateVal);
            $scope.datetimePickerValue = dateVal.format($scope.model.config.format);
        }
        else {
            $scope.clearDate();
        }
    }

    $scope.clearDate = function() {
        $scope.hasDatetimePickerValue = false;
        $scope.datetimePickerValue = null;
        $scope.model.value = null;
        $scope.datePickerForm.datepicker.$setValidity("pickerError", true);
    }

    $scope.serverTime = null;
    $scope.serverTimeNeedsOffsetting = false;
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

    //get the current user to see if we can localize this picker
    userService.getCurrentUser().then(function (user) {

        assetsService.loadCss('lib/datetimepicker/bootstrap-datetimepicker.min.css', $scope).then(function() {

        var filesToLoad = ["lib/datetimepicker/bootstrap-datetimepicker.js"];

            
		$scope.model.config.language = user.locale;
		

		assetsService.load(filesToLoad, $scope).then(
            function () {
				//The Datepicker js and css files are available and all components are ready to use.

				// Get the id of the datepicker button that was clicked
				var pickerId = $scope.model.alias;

			    var element = $element.find("div:first");

				// Create the datepicker and add a changeDate eventlistener
			    element
			        .datetimepicker(angular.extend({ useCurrent: true }, $scope.model.config))
			        .on("dp.change", applyDate)
			        .on("dp.error", function(a, b, c) {
			            $scope.hasDatetimePickerValue = false;
			            $scope.datePickerForm.datepicker.$setValidity("pickerError", false);
			        });

                $(document).bind("click", $scope.hidePicker);

                setDatePickerVal(element);
			    
			    element.find("input").bind("blur", function() {
			        //we need to force an apply here
			        $scope.$apply();
                });

                $scope.$watch("model.value", function(newVal, oldVal) {
                    if (newVal !== oldVal) {
                        $scope.hasDatetimePickerValue = newVal ? true : false;
                        setDatePickerVal(element);
                    }
                });

                var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
                    setModelValue();
                });

				//Ensure to remove the event handler when this instance is destroyted
			    $scope.$on('$destroy', function () {
			        element.find("input").unbind("blur");
                    element.datetimepicker("destroy");
                    unsubscribe();
			        $(document).unbind("click", $scope.hidePicker);
			    });

			});
        });
        
    });
    
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.DatepickerController", dateTimePickerController);
