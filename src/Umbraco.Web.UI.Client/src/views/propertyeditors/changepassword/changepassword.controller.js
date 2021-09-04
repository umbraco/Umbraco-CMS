angular.module("umbraco").controller("Umbraco.PropertyEditors.ChangePasswordController",
  function ($scope, $routeParams) {

    $scope.isNew = $routeParams.create;

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
      if (!$scope.model.config || $scope.model.config.minNonAlphaNumericChars === undefined) {
          $scope.model.config.minNonAlphaNumericChars = 0;
      }

      //set the model defaults
      if (!Utilities.isObject($scope.model.value)) {
        //if it's not an object then just create a new one
        $scope.model.value = {
          newPassword: null,
          oldPassword: null,
          reset: null,
          answer: null
        };
      }
      else {
        //just reset the values

        if (!$scope.isNew) {
          //if it is new, then leave the generated pass displayed
          $scope.model.value.newPassword = null;
          $scope.model.value.oldPassword = null;
        }
        $scope.model.value.reset = null;
        $scope.model.value.answer = null;
      }
    }

    resetModel();    

  });
