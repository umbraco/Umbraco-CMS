function dateTimePickerController($scope, notificationsService, assetsService, angularHelper, userService) {

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
        });
    }

    //get the current user to see if we can localize this picker
    userService.getCurrentUser().then(function (user) {
        
        var filesToLoad = ["lib/datetimepicker/bootstrap-datetimepicker.min.js"];

        //if we support this custom culture, set it, then we'll need to load in that lang file
        if (_.contains(customLangs, user.locale)) {
            $scope.model.config.language = user.locale;
            filesToLoad.push("lib/datetimepicker/langs/datetimepicker." + user.locale + ".js");
        }
        
        assetsService.load(filesToLoad).then(
            function() {
                //The Datepicker js and css files are available and all components are ready to use.

                // Get the id of the datepicker button that was clicked
                var pickerId = $scope.model.alias;
                // Open the datepicker and add a changeDate eventlistener
                $("#datepicker" + pickerId)
                    //.datetimepicker(config);
                    .datetimepicker($scope.model.config)
                    .on("changeDate", applyDate);

                //now assign the date
                $("#datepicker" + pickerId).val($scope.model.value);

            });

    });
    
    assetsService.loadCss(
        'lib/datetimepicker/bootstrap-datetimepicker.min.css'
    );
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.DatepickerController", dateTimePickerController);
