(function() {
  'use strict';

  function ConfirmDelete() {

    function link(scope, el, attr, ctrl) {

      scope.confirmOverlayOpen = false;

      scope.toggleOverlay = function() {
        scope.confirmOverlayOpen = !scope.confirmOverlayOpen;
      };

      scope.closeOverlay = function() {
        scope.confirmOverlayOpen = false;
      };

    }

    var directive = {
      restrict: 'E',
      replace: true,
      templateUrl: 'views/components/umb-confirm-delete.html',
      scope: {
        confirmAction: "&"
      },
      link: link
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbConfirmDelete', ConfirmDelete);

})();
