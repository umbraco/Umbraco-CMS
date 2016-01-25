(function() {
   'use strict';

   function ContentGridDirective() {

      function link(scope, el, attr, ctrl) {

         scope.clickItem = function(item, $event, $index) {
            if(scope.onClick) {
               scope.onClick(item, $event, $index);
            }
         };

         scope.clickItemName = function(item, $event, $index) {
            if(scope.onClickName) {
               scope.onClickName(item, $event, $index);
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
            onClick: "=",
            onClickName: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbContentGrid', ContentGridDirective);

})();
