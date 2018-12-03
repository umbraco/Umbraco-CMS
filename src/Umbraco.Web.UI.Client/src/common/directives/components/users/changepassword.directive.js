(function () {
  'use strict';

  function ChangePasswordController($scope) {

    function resetModel(isNew) {
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

      $scope.showReset = false;

      //set defaults if they are not available
      if ($scope.config.disableToggle === undefined) {
        $scope.config.disableToggle = false;
      }
      if ($scope.config.hasPassword === undefined) {
        $scope.config.hasPassword = false;
      }
      if ($scope.config.enablePasswordRetrieval === undefined) {
        $scope.config.enablePasswordRetrieval = true;
      }
      if ($scope.config.requiresQuestionAnswer === undefined) {
        $scope.config.requiresQuestionAnswer = false;
      }
      //don't enable reset if it is new - that doesn't make sense
      if (isNew === "true") {
          $scope.config.enableReset = false;
      }
      else if ($scope.config.enableReset === undefined) {
        $scope.config.enableReset = true;
      }      
        
      if ($scope.config.minPasswordLength === undefined) {
        $scope.config.minPasswordLength = 0;
      }
        
      //set the model defaults
      if (!angular.isObject($scope.passwordValues)) {
        //if it's not an object then just create a new one
        $scope.passwordValues = {
          newPassword: null,
          oldPassword: null, 
          reset: null,
          answer: null
        };
      }
      else {
        //just reset the values

        if (!isNew) {
          //if it is new, then leave the generated pass displayed
          $scope.passwordValues.newPassword = null;
          $scope.passwordValues.oldPassword = null;
        }
        $scope.passwordValues.reset = null;
        $scope.passwordValues.answer = null;
      }

      //the value to compare to match passwords
      if (!isNew) {
          $scope.passwordValues.confirm = "";
      }
      else if ($scope.passwordValues.newPassword && $scope.passwordValues.newPassword.length > 0) {
        //if it is new and a new password has been set, then set the confirm password too
          $scope.passwordValues.confirm = $scope.passwordValues.newPassword;
      }

    }

    resetModel($scope.isNew);

    //if there is no password saved for this entity , it must be new so we do not allow toggling of the change password, it is always there
    //with validators turned on.
    $scope.changing = $scope.config.disableToggle === true || !$scope.config.hasPassword;

    //we're not currently changing so set the model to null
    if (!$scope.changing) {
      $scope.passwordValues = null;
    }

    $scope.doChange = function () {
      resetModel();
      $scope.changing = true;
      //if there was a previously generated password displaying, clear it
      $scope.passwordValues.generatedPassword = null;
      $scope.passwordValues.confirm = null;
    };

    $scope.cancelChange = function () {
      $scope.changing = false;
      //set model to null
      $scope.passwordValues = null;
    };

    var unsubscribe = [];

    //listen for the saved event, when that occurs we'll 
    //change to changing = false;
    unsubscribe.push($scope.$on("formSubmitted", function () {
      if ($scope.config.disableToggle === false) {
        $scope.changing = false;
      }
    }));
    unsubscribe.push($scope.$on("formSubmitting", function () {
      //if there was a previously generated password displaying, clear it
      if ($scope.changing && $scope.passwordValues) {
        $scope.passwordValues.generatedPassword = null;
      }
      else if (!$scope.changing) {
        //we are not changing, so the model needs to be null
        $scope.passwordValues = null;
      }
    }));

    //when the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
      for (var u in unsubscribe) {
        unsubscribe[u]();
      }
    });
      
    $scope.showOldPass = function () {
      return $scope.config.hasPassword &&
        !$scope.config.allowManuallyChangingPassword &&
        !$scope.config.enablePasswordRetrieval && !$scope.showReset;
    };
      
    //TODO: I don't think we need this or the cancel button, this can be up to the editor rendering this directive
    $scope.showCancelBtn = function () {
      return $scope.config.disableToggle !== true && $scope.config.hasPassword;
    };

  }

  function ChangePasswordDirective() {

    var directive = {
      restrict: 'E',
      replace: true,
      templateUrl: 'views/components/users/change-password.html',
      controller: 'Umbraco.Editors.Users.ChangePasswordDirectiveController',
      scope: {
        isNew: "=?",
        passwordValues: "=",
        config: "="        
      }
    };

    return directive;

  }

  angular.module('umbraco.directives').controller('Umbraco.Editors.Users.ChangePasswordDirectiveController', ChangePasswordController);
  angular.module('umbraco.directives').directive('changePassword', ChangePasswordDirective);


})();
