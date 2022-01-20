angular.module("umbraco").controller("Umbraco.PropertyEditors.ChangePasswordController",
  function ($scope, $routeParams) {

    $scope.isNew = $routeParams.create;

    function resetModel() {
      //the model config will contain an object, if it does not we'll create defaults      
      /*
      {
          hasPassword: true/false,          
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
