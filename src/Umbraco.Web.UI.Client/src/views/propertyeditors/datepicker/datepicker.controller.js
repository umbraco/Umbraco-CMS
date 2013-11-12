angular.module("umbraco").controller("Umbraco.PropertyEditors.DatepickerController",
    function ($scope, notificationsService, assetsService, angularHelper) {

        //setup the default config
        var config = {
            pickDate: true,
            pickTime: true,
            format: "yyyy-MM-dd HH:mm:ss"
        };

        //map the user config
        angular.extend(config, $scope.model.config);
        //map back to the model
        $scope.model.config = config;

        if($scope.model.value){
            $scope.model.datetime = moment($scope.model.value).toDate();
        }
        
        assetsService.loadCss(
            'views/propertyeditors/datepicker/bootstrap-datetimepicker.min.css'
        );

        $scope.$on("formSubmitting", function (ev, args) {

            //hack to set the hours right
            //since an hour is subtracted on the server? 
            var t = angular.copy($scope.model.datetime);
            t.setHours(t.getHours()+1);
            $scope.model.value = t;
        });

    });