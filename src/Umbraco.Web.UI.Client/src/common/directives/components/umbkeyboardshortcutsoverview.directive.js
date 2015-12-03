(function() {
  'use strict';

  function KeyboardShortcutsOverviewDirective() {

    function link(scope, el, attr, ctrl) {

      scope.shortcutOverlay = false;

      scope.toggleShortcutsOverlay = function() {
        scope.shortcutOverlay = !scope.shortcutOverlay;
      };

    }

    var directive = {
      restrict: 'E',
      replace: true,
      templateUrl: 'views/components/umb-keyboard-shortcuts-overview.html',
      link: link,
      scope: {
        model: "="
      }
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbKeyboardShortcutsOverview', KeyboardShortcutsOverviewDirective);

})();
