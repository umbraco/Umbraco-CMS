function dateTimePickerController($scope, notificationsService, assetsService, angularHelper, userService, $element) {

    //setup the default config
    var config = {
        pickDate: true,
        pickTime: true,
        useMinutes: true,
        useSeconds: true,
        minuteStepping: 1,
        calendarWeeks: false,
        showToday: false,
        format: "YYYY-MM-DD HH:mm:ss",
		icons: {
            time: "icon-time",
            date: "icon-calendar",
            up: "icon-chevron-up",
            down: "icon-chevron-down"
		},
		daysOfWeekDisabled: []
    };

    //map the user config
    $scope.model.config = angular.extend(config, $scope.model.config);

    if ($scope.model.config.showToday !== undefined || $scope.model.config.showToday !== null) {
        $scope.model.config.showToday = $scope.model.config.showToday == 0 ? false : true;
    }

    if ($scope.model.config.calendarWeeks !== undefined || $scope.model.config.calendarWeeks !== null) {
        $scope.model.config.calendarWeeks = $scope.model.config.calendarWeeks == 0 ? false : true;
    }

    if ($scope.model.config.minuteStepping !== undefined || $scope.model.config.minuteStepping !== null) {
        $scope.model.config.minuteStepping = parseInt($scope.model.config.minuteStepping) > 0 ? parseInt($scope.model.config.minuteStepping) : 1;
    }

    if ($scope.model.config.minDate !== "" || $scope.model.config.minDate !== undefined || $scope.model.config.minDate !== null) {
        $scope.model.config.minDate = new Date($scope.model.config.minDate);
    }

    if ($scope.model.config.maxDate !== "" || $scope.model.config.maxDate !== undefined || $scope.model.config.maxDate !== null) {
        $scope.model.config.maxDate = new Date($scope.model.config.maxDate);
    }

    //ensure the format doesn't get overwritten with an empty string
    if ($scope.model.config.format === "" || $scope.model.config.format === undefined || $scope.model.config.format === null) {
        $scope.model.config.format = $scope.model.config.pickTime ? "YYYY-MM-DD HH:mm:ss" : "YYYY-MM-DD";
    }

    //set value of these boolean properties based on config format - in v4 and v5 of the datepicker plugin it decide the "viewModes" based on format
    $scope.model.config.useSeconds = containsChar($scope.model.config.format, "s") ? true : false;
    $scope.model.config.useMinutes = containsChar($scope.model.config.format, "m") ? true : false;
    $scope.model.config.pickTime = containsChar($scope.model.config.format, "H") || containsChar($scope.model.config.format, "m") ? true : false;
    $scope.model.config.pickDate = containsChar($scope.model.config.format, "Y") || containsChar($scope.model.config.format, "M") || containsChar($scope.model.config.format, "D") ? true : false;

    $scope.hasDatetimePickerValue = $scope.model.value ? true : false;
    $scope.datetimePickerValue = null;

    $scope.onlyTimePicker = $scope.model.config.pickTime && !$scope.model.config.pickDate;
    $scope.datetimePickerIcon = $scope.onlyTimePicker ? "time" : "calendar";

    //hide picker if clicking on the document 
    $scope.hidePicker = function () {
        //$element.find("div:first").datetimepicker("hide");
        // Sometimes the statement above fails and generates errors in the browser console. The following statements fix that.
        var dtp = $element.find("div:first");
        if (dtp && dtp.datetimepicker) {
            dtp.datetimepicker("hide");
        }
    };
    $(document).bind("click", $scope.hidePicker);

    //handles the date changing via the api
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
            
            if (!$scope.model.config.pickTime) {
                $element.find("div:first").datetimepicker("hide", 0);
            }
        });
    }

    function containsChar(string, it) {
        return string.indexOf(it) != -1;
    };

    var picker = null;

    $scope.clearDate = function() {
        $scope.hasDatetimePickerValue = false;
        $scope.datetimePickerValue = null;
        $scope.model.value = null;
        $scope.datePickerForm.datepicker.$setValidity("pickerError", true);
    }

    //get the current user to see if we can localize this picker
    userService.getCurrentUser().then(function (user) {

        assetsService.loadCss('lib/datetimepicker/bootstrap-datetimepicker.min.css').then(function() {

        var filesToLoad = ["lib/moment/moment-with-locales.js",
						   "lib/datetimepicker/bootstrap-datetimepicker.js"];
        
		$scope.model.config.language = user.locale;

		assetsService.load(filesToLoad, $scope).then(
            function () {
				//The Datepicker js and css files are available and all components are ready to use.

				// Get the id of the datepicker button that was clicked
				var pickerId = $scope.model.alias;

			    var element = $element.find("div:first");

				// Open the datepicker and add a changeDate eventlistener
			    element
			        .datetimepicker(angular.extend({ useCurrent: true }, $scope.model.config))
			        .on("dp.change", applyDate)
			        .on("dp.error", function(a, b, c) {
			            $scope.hasDatetimePickerValue = false;
			            $scope.datePickerForm.datepicker.$setValidity("pickerError", false);
			        });

			    if ($scope.hasDatetimePickerValue) {

			        //assign value to plugin/picker
			        var dateVal = $scope.model.value ? moment($scope.model.value, $scope.model.config.format) : moment();
			        element.datetimepicker("setValue", dateVal);
			        $scope.datetimePickerValue = moment($scope.model.value).format($scope.model.config.format);
			    }

			    element.find("input").bind("blur", function() {
			        //we need to force an apply here
			        $scope.$apply();
			    });

				//Ensure to remove the event handler when this instance is destroyted
			    $scope.$on('$destroy', function () {
			        element.find("input").unbind("blur");
					element.datetimepicker("destroy");
			    });

			    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
			        if ($scope.hasDatetimePickerValue) {
			            var elementData = $element.find("div:first").data().DateTimePicker;
			            if ($scope.model.config.pickDate && $scope.model.config.pickTime) {
			                if ($scope.model.config.useSeconds) {
			                    $scope.model.value = elementData.getDate().format("YYYY-MM-DD HH:mm:ss");
			                }
			                else {
			                    $scope.model.value = elementData.getDate().format("YYYY-MM-DD HH:mm");
			                }
			            }
			            else if ($scope.model.config.pickDate && !$scope.model.config.pickTime) {
			                $scope.model.value = elementData.getDate().format("YYYY-MM-DD");
			            }
			            else if (!$scope.model.config.pickDate && $scope.model.config.pickTime) {
			                if ($scope.model.config.useSeconds) {
			                    $scope.model.value = elementData.getDate().format("HH:mm:ss");
			                }
			                else {
			                    $scope.model.value = elementData.getDate().format("HH:mm");
			                }
			            }
			            else {
			                $scope.model.value = elementData.getDate().format("YYYY-MM-DD");
			            }
			        }
			        else {
			            $scope.model.value = null;
			        }
			    });
			    //unbind doc click event!
			    $scope.$on('$destroy', function () {
			        unsubscribe();
			    });


			});
        });
        
    });

    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
        if ($scope.hasDatetimePickerValue) {
            var elementData = $element.find("div:first").data().DateTimePicker;
            if ($scope.model.config.pickDate && $scope.model.config.pickTime) {
                if ($scope.model.config.useSeconds) {
                    $scope.model.value = elementData.getDate().format("YYYY-MM-DD HH:mm:ss");
                }
                else {
                    $scope.model.value = elementData.getDate().format("YYYY-MM-DD HH:mm");
                }
            }
            else if ($scope.model.config.pickDate && !$scope.model.config.pickTime) {
                $scope.model.value = elementData.getDate().format("YYYY-MM-DD");
            }
            else if (!$scope.model.config.pickDate && $scope.model.config.pickTime) {
                if ($scope.model.config.useSeconds) {
                    $scope.model.value = elementData.getDate().format("HH:mm:ss");
                }
                else {
                    $scope.model.value = elementData.getDate().format("HH:mm");
                }
            }
            else {
                $scope.model.value = elementData.getDate().format("YYYY-MM-DD");
            }
        }
        else {
            $scope.model.value = null;
        }
    });

    //unbind doc click event!
    $scope.$on('$destroy', function () {
        $(document).unbind("click", $scope.hidePicker);
        unsubscribe();
    });
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.DatepickerController", dateTimePickerController);
