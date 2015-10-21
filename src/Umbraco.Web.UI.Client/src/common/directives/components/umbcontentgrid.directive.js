(function() {
   'use strict';

   function ContentGridDirective() {

      function link(scope, el, attr, ctrl) {

         scope.contentItemSortingOptions = {
            distance: 10,
            tolerance: "pointer",
            opacity: 0.7,
            scroll: true,
            cursor: "move",
            zIndex: 6000,
            placeholder: "umb-content-grid__placeholder",
            start: function(e, ui) {
              ui.placeholder.height(ui.item.height());
              ui.placeholder.width(ui.item.width());
            }
         };

         scope.clickItem = function(item) {
            if(scope.onClick) {
               scope.onClick(item);
            }
         };

         scope.selectItem = function(item, $event) {
            if(scope.onSelect) {
               scope.onSelect(item);
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
