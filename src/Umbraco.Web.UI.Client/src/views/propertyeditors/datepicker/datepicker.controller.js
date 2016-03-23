function dateTimePickerController($scope, notificationsService, assetsService, angularHelper, userService, $element) {

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
    $(document).bind("click", $scope.hidePicker);

    //handles the date changing via the api
    function applyDate(e) {
        angularHelper.safeApply($scope, function() {
            // when a date is changed, update the model
            if (e.date && e.date.isValid()) {
                $scope.datePickerForm.datepicker.$setValidity("pickerError", true);
                $scope.hasDatetimePickerValue = true;
                $scope.datetimePickerValue = e.date.format($scope.model.config.format);
                $scope.model.value = $scope.datetimePickerValue;
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
			        var dateVal = $scope.model.value ? moment($scope.model.value, "YYYY-MM-DD HH:mm:ss") : moment();

			        element.datetimepicker("setValue", dateVal);
			        $scope.datetimePickerValue = dateVal.format($scope.model.config.format);
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
			            if ($scope.model.config.pickTime) {
			                $scope.model.value = elementData.getDate().format("YYYY-MM-DD HH:mm:ss");
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
            if ($scope.model.config.pickTime) {
                $scope.model.value = $element.find("div:first").data().DateTimePicker.getDate().format("YYYY-MM-DD HH:mm:ss");
            }
            else {
                $scope.model.value = $element.find("div:first").data().DateTimePicker.getDate().format("YYYY-MM-DD");
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
