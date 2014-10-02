function dateTimePickerController($scope, notificationsService, assetsService, angularHelper, userService, $element) {

    //lists the custom language files that we currently support
    var customLangs = ["pt-BR"];

    //setup the default config
    var config = {
        pickDate: true,
        pickTime: true,
        pick12HourFormat: false,
        format: "yyyy-MM-dd hh:mm:ss"
    };

    //map the user config
    $scope.model.config = angular.extend(config, $scope.model.config);

    //hide picker if clicking on the document 
    $scope.hidePicker = function () {
        $element.find("div:first").datetimepicker("hide");
    };
    $(document).click(function (event) {
        $scope.hidePicker();
    });

    //handles the date changing via the api
    function applyDate(e) {
        angularHelper.safeApply($scope, function() {
            // when a date is changed, update the model
            if (e.localDate) {
                if ($scope.model.config.format == "yyyy-MM-dd hh:mm:ss") {
                    $scope.model.value = e.localDate.toIsoDateTimeString();
                }
                else {
                    $scope.model.value = e.localDate.toIsoDateString();
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
            var filesToLoad = ["lib/datetimepicker/bootstrap-datetimepicker.min.js"];

            //if we support this custom culture, set it, then we'll need to load in that lang file
            if (_.contains(customLangs, user.locale)) {
                $scope.model.config.language = user.locale;
                filesToLoad.push("lib/datetimepicker/langs/datetimepicker." + user.locale + ".js");
            }

            assetsService.load(filesToLoad).then(
                function () {
                    //The Datepicker js and css files are available and all components are ready to use.

                    // Get the id of the datepicker button that was clicked
                    var pickerId = $scope.model.alias;

                    // Open the datepicker and add a changeDate eventlistener
                    $element.find("div:first")
                        .datetimepicker($scope.model.config)
                        .on("changeDate", applyDate);

                    if ($scope.model.value) {
                        //manually assign the date to the plugin
                        $element.find("div:first").datetimepicker("setValue", $scope.model.value);
                    }

                    //Ensure to remove the event handler when this instance is destroyted
                    $scope.$on('$destroy', function () {
                        $element.find("div:first").datetimepicker("destroy");
                    });
                });
        });

        
    });
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.DatepickerController", dateTimePickerController);
