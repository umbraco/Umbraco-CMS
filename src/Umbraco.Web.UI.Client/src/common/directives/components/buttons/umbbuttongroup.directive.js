(function() {
   'use strict';

   function ButtonGroupDirective() {

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/buttons/umb-button-group.html',
         scope: {
            actions: "=",
            model: "="
         }
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbButtonGroup', ButtonGroupDirective);

})();
