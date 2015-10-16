(function() {
   'use strict';

   function ButtonDirective($timeout) {

      function link(scope, el, attr, ctrl) {

         scope.style = null;

         function activate() {

            if (!scope.state) {
               scope.state = "init";
            }

            if (scope.buttonStyle) {
               scope.style = "btn-" + scope.buttonStyle;
            }

         }

         activate();

         var unbindStateWatcher = scope.$watch('state', function(newValue, oldValue) {

            if (newValue === 'success' || newValue === 'error') {
               $timeout(function() {
                  scope.state = 'init';
               }, 2000);
            }

         });

         scope.$on('$destroy', function() {
            unbindStateWatcher();
         });

      }

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/buttons/umb-button.html',
         link: link,
         scope: {
            action: "&?",
            href: "@?",
            type: "@",
            buttonStyle: "@?",
            state: "=?",
            shortcut: "@?",
            label: "@?",
            labelKey: "@?",
            icon: "@?",
            disabled: "="
         }
      };

      return directive;

   }

   angular.module('umbraco.directives').directive('umbButton', ButtonDirective);

})();
