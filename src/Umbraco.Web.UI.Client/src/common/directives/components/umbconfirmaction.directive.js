(function() {
  'use strict';

  function ConfirmAction() {

    function link(scope, el, attr, ctrl) {

      scope.clickConfirm = function() {
          if(scope.onConfirm) {
              scope.onConfirm();
          }
      };

      scope.clickCancel = function() {
          if(scope.onCancel) {
              scope.onCancel();
          }
      };

    }

    var directive = {
      restrict: 'E',
      replace: true,
      templateUrl: 'views/components/umb-confirm-action.html',
      scope: {
        direction: "@",
        onConfirm: "&",
        onCancel: "&"
      },
      link: link
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbConfirmAction', ConfirmAction);

})();
