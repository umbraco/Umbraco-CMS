(function() {
   'use strict';

   function ContentGridDirective() {

      function link(scope, el, attr, ctrl) {

         scope.clickItem = function(item) {
            if(scope.onClick) {
               scope.onClick(item);
            }
         };

         scope.selectItem = function(item, $event, $index) {
            if(scope.onSelect) {
               scope.onSelect(item, $event, $index);
               $event.stopPropagation();
            }
         };

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/umb-content-grid.html',
         scope: {
            content: '=',
            contentProperties: "=",
            onSelect: '=',
            onClick: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbContentGrid', ContentGridDirective);

})();
