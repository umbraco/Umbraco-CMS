(function() {
  'use strict';

  function UmbDisableFormValidation() {

      var directive = {
          restrict: 'A',
          require: '?form',
          link: function (scope, elm, attrs, ctrl) {
              //override the $setValidity function of the form to disable validation
              ctrl.$setValidity = function () { };
          }
      };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbDisableFormValidation', UmbDisableFormValidation);

})();
