angular.module("umbraco").controller("Umbraco.Editors.DatepickerController",
    function ($scope, notificationsService, assetsService) {
    
    assetsService.loadJs(
            'views/propertyeditors/datepicker/bootstrap.datepicker.js'
        ).then(
        function () {
            //The Datepicker js and css files are available and all components are ready to use.

            // Get the id of the datepicker button that was clicked
            var pickerId = $scope.model.alias;

            // Open the datepicker and add a changeDate eventlistener
            $("#" + pickerId).datepicker({
                format: "dd/mm/yyyy",
                autoclose: true
            }).on("changeDate", function (e) {
                // When a date is clicked the date is stored in model.value as a ISO 8601 date
                $scope.model.value = e.date.toIsoDateTimeString();
            });
        });


    assetsService.loadCss(
            'views/propertyeditors/datepicker/bootstrap.datepicker.css'
        );    
});
