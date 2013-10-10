angular.module("umbraco").controller("Umbraco.PropertyEditors.ChangePasswordController",
    function($scope) {
        
        //the model config will contain an object, if it does not we'll create defaults
        //NOTE: We will not support doing the password regex on the client side because the regex on the server side
        //based on the membership provider cannot always be ported to js from .net directly.        
        /*
        {
            hasPassword: true/false,
            requiresQuestionAnswer: true/false,
            enableReset: true/false,
            minPasswordLength: 10
        }
        */

        //set defaults if they are not available
        if (!$scope.model.config || !$scope.model.config.hasPassword) {
            $scope.model.config.hasPassword = false;
        }
        if (!$scope.model.config || !$scope.model.config.requiresQuestionAnswer) {
            $scope.model.config.requiresQuestionAnswer = false;
        }
        if (!$scope.model.config || !$scope.model.config.enableReset) {
            $scope.model.config.enableReset = true;
        }
        if (!$scope.model.config || !$scope.model.config.minPasswordLength) {
            $scope.model.config.minPasswordLength = 0;
        }
       
        $scope.confirm = "";

        $scope.changing = !$scope.model.config.hasPassword;

        $scope.doChange = function() {
            $scope.changing = true;
        };

        $scope.cancelChange = function() {
            $scope.changing = false;
        };
    });
