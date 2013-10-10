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
            enablePasswordRetrieval: true/false,
            minPasswordLength: 10
        }
        */

        //set defaults if they are not available
        if (!$scope.model.config || $scope.model.config.hasPassword === undefined) {
            $scope.model.config.hasPassword = false;
        }
        if (!$scope.model.config || $scope.model.config.enablePasswordRetrieval === undefined) {
            $scope.model.config.enablePasswordRetrieval = true;
        }
        if (!$scope.model.config || $scope.model.config.requiresQuestionAnswer === undefined) {
            $scope.model.config.requiresQuestionAnswer = false;
        }
        if (!$scope.model.config || $scope.model.config.enableReset === undefined) {
            $scope.model.config.enableReset = true;
        }
        if (!$scope.model.config || $scope.model.config.minPasswordLength === undefined) {
            $scope.model.config.minPasswordLength = 0;
        }
       
        //set the model defaults - we never get supplied a password from the server so this is ok to overwrite.
        $scope.model.value = {
            newPassword: "",
            oldPassword: null,
            reset: null,
            answer: null
        };
        //the value to compare to match passwords
        $scope.confirm = "";

        //if there is no password saved for this entity , it must be new so we do not allow toggling of the change password, it is always there
        //with validators turned on.
        $scope.changing = !$scope.model.config.hasPassword;

        $scope.doChange = function() {
            $scope.changing = true;
        };

        $scope.cancelChange = function() {
            $scope.changing = false;
        };
    });
