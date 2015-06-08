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

    $scope.datetimePickerValue = $scope.model.value;

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
            if (e.date) {
                if ($scope.model.config.pickTime) {
                    $scope.model.value = e.date.format("YYYY-MM-DD HH:mm:ss");
                }
                else {
                    $scope.model.value = e.date.format("YYYY-MM-DD");
                }
            }
            
            if (!$scope.model.config.pickTime) {
                $element.find("div:first").datetimepicker("hide", 0);
            }
        });
    }

    //get the current user to see if we can localize this picker
    userService.getCurrentUser().then(function (user) {

        assetsService.loadCss('lib/datetimepicker/bootstrap-datetimepicker.min.css').then(function() {

        var filesToLoad = ["lib/moment/moment-with-locales.js",
						   "lib/datetimepicker/bootstrap-datetimepicker.js"];

            
		$scope.model.config.language = user.locale;
		

		assetsService.load(filesToLoad).then(
			function () {
				//The Datepicker js and css files are available and all components are ready to use.

				// Get the id of the datepicker button that was clicked
				var pickerId = $scope.model.alias;

				// Open the datepicker and add a changeDate eventlistener
				$element.find("div:first")
					.datetimepicker($scope.model.config)
					.on("dp.change", applyDate);

			    //manually assign the date to the plugin
				if (!$scope.model.config.format) {
				    $element.find("div:first").datetimepicker("setValue", $scope.model.value ? $scope.model.value : null);
				}
				else {
				    $element.find("div:first").datetimepicker("setValue", $scope.model.value ? new Date($scope.model.value) : null);
				    if ($scope.model.value && $scope.model.config.format) {
				        $scope.datetimePickerValue = moment($scope.model.value).format($scope.model.config.format);
				    }
				}

				//Ensure to remove the event handler when this instance is destroyted
				$scope.$on('$destroy', function () {
					$element.find("div:first").datetimepicker("destroy");
				});
			});
        });

        
    });

    //unbind doc click event!
    $scope.$on('$destroy', function () {
        $(document).unbind("click", $scope.hidePicker);
    });
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.DatepickerController", dateTimePickerController);
