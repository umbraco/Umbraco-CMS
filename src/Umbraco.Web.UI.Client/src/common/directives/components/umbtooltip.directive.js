(function() {
   'use strict';

   function TooltipDirective($timeout) {

      function link(scope, el, attr, ctrl) {

         scope.tooltipStyles = {};
         scope.tooltipStyles.left = 0;
         scope.tooltipStyles.top = 0;

         function activate() {

            $timeout(function() {
               setTooltipPosition(scope.event);
            });

         }

         function setTooltipPosition(event) {

            var viewportWidth = null;
            var viewportHeight = null;
            var elementHeight = null;
            var elementWidth = null;

            var position = {
               right: "inherit",
               left: "inherit",
               top: "inherit",
               bottom: "inherit"
            };

            // viewport size
            viewportWidth = $(window).innerWidth();
            viewportHeight = $(window).innerHeight();

            // element size
            elementHeight = el.context.clientHeight;
            elementWidth = el.context.clientWidth;

            position.left = event.pageX - (elementWidth / 2);
            position.top = event.pageY;

            // check to see if element is outside screen
            // outside right
            if (position.left + elementWidth > viewportWidth) {
               position.right = 0;
               position.left = "inherit";
            }

            // outside bottom
            if (position.top + elementHeight > viewportHeight) {
               position.bottom = 0;
               position.top = "inherit";
            }

            scope.tooltipStyles = position;

         }

         activate();

      }

      var directive = {
         restrict: 'E',
         transclude: true,
         replace: true,
         templateUrl: 'views/components/umb-tooltip.html',
         scope: {
            event: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbTooltip', TooltipDirective);

})();
