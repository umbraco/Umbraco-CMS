angular.module("umbraco").controller("Umbraco.PropertyEditors.ChangePasswordController",
    function($scope) {
        
        function resetModel() {
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
            if (!$scope.model.config || $scope.model.config.disableToggle === undefined) {
                $scope.model.config.disableToggle = false;
            }
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
            
            //set the model defaults
            if (!angular.isObject($scope.model.value)) {
                //if it's not an object then just create a new one
                $scope.model.value = {
                    newPassword: "",
                    oldPassword: null,
                    reset: null,
                    answer: null
                };
            }
            else {
                //just reset the values we need to
                $scope.model.value.newPassword = "";
                $scope.model.value.oldPassword = null;
                $scope.model.value.reset = null;
                $scope.model.value.answer = null;
            }

            //the value to compare to match passwords
            $scope.model.confirm = "";
        }

        resetModel();

        //if there is no password saved for this entity , it must be new so we do not allow toggling of the change password, it is always there
        //with validators turned on.
        $scope.changing = $scope.model.config.disableToggle === true || !$scope.model.config.hasPassword;

        $scope.doChange = function() {
            $scope.changing = true;
            //if there was a previously generated password displaying, clear it
            $scope.model.value.generatedPassword = null;
        };

        $scope.cancelChange = function() {
            $scope.changing = false;
        };
        
        //listen for the saved event, when that occurs we'll 
        //change to changing = false;
        $scope.$on("formSubmitted", function () {
            if ($scope.model.config.disableToggle === false) {
                $scope.changing = false;
            }
            resetModel();
        });
        $scope.$on("formSubmitting", function() {
            //if there was a previously generated password displaying, clear it
            $scope.model.value.generatedPassword = null;
        });

        $scope.showReset = function() {
            return $scope.model.config.hasPassword && $scope.model.config.enableReset;
        };

        $scope.showOldPass = function() {
            return $scope.model.config.hasPassword && !$scope.model.config.enablePasswordRetrieval && !$scope.model.value.reset;
        };

        $scope.showNewPass = function () {
            return !$scope.model.value.reset;
        };

        $scope.showConfirmPass = function() {
            return !$scope.model.value.reset;
        };
        
        $scope.showCancelBtn = function() {
            return $scope.model.config.disableToggle !== true && $scope.model.config.hasPassword;
        };

        $scope.oldPassRequired = function() {
            return !$scope.model.value.reset && $scope.model.config.hasPassword && !$scope.model.config.enablePasswordRetrieval;
        };

    });
