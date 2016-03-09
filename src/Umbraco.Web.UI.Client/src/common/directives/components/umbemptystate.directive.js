(function() {
   'use strict';

   function EmptyStateDirective() {

      var directive = {
         restrict: 'E',
         replace: true,
         transclude: true,
         templateUrl: 'views/components/umb-empty-state.html',
         scope: {
            size: '@',
            position: '@'
         }
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEmptyState', EmptyStateDirective);

})();
