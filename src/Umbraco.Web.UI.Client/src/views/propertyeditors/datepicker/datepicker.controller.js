angular.module("umbraco").controller("Umbraco.Editors.DatepickerController",
    function ($scope, notificationsService, assetsService, userService) {

        function applyDate(e){
                // when a date is changed, update the model
                if (e.localDate) {
                    if ($scope.model.config.format == "yyyy-MM-dd HH:mm:ss") {
                        $scope.$apply(function(){
                            $scope.model.value = e.localDate.toIsoDateTimeString();
                        });
                    }else{
                        $scope.model.value = e.localDate.toIsoDateString();
                    }
                }
        }

        function initEditor(){
            // Get the id of the datepicker button that was clicked
            var pickerId = $scope.model.alias;
            // Open the datepicker and add a changeDate eventlistener
            
            $("#" + pickerId)
                .datetimepicker($scope.model.config)
                .on("changeDate", applyDate)
                .on("hide", applyDate);
        }
        
        userService.getCurrentUser().then(function(user){

            //setup the default config
            var config = {
                pickDate: true,
                pickTime: true,
                language: user.locale,
                format: "yyyy-MM-dd HH:mm:ss"
            };

            //format:"yyyy-MM-dd HH:mm:ss"

            //map the user config
            angular.extend(config, $scope.model.config);
            //map back to the model
            $scope.model.config = config;
            $scope.model.viewvalue = $scope.model.value;

            assetsService.loadJs(
                    'views/propertyeditors/datepicker/bootstrap-datetimepicker.js'
                ).then(initEditor);
        });
 
        assetsService.loadCss(
                'views/propertyeditors/datepicker/bootstrap-datetimepicker.min.css'
        );
    });