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

            var container = $("#contentwrapper");
            var containerLeft = container[0].offsetLeft;
            var containerRight = containerLeft + container[0].offsetWidth;
            var containerTop = container[0].offsetTop;
            var containerBottom = containerTop + container[0].offsetHeight;

            var elementHeight = null;
            var elementWidth = null;

            var position = {
               right: "inherit",
               left: "inherit",
               top: "inherit",
               bottom: "inherit"
            };

            // element size
            elementHeight = el.context.clientHeight;
            elementWidth = el.context.clientWidth;

            position.left = event.pageX - (elementWidth / 2);
            position.top = event.pageY;

            // check to see if element is outside screen
            // outside right
            if (position.left + elementWidth > containerRight) {
               position.right = 10;
               position.left = "inherit";
            }

            // outside bottom
            if (position.top + elementHeight > containerBottom) {
               position.bottom = 10;
               position.top = "inherit";
            }

            // outside left
            if (position.left < containerLeft) {
               position.left = containerLeft + 10;
               position.right = "inherit";
            }

            // outside top
            if (position.top < containerTop) {
               position.top = 10;
               position.bottom = "inherit";
            }

            scope.tooltipStyles = position;

            el.css(position);

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
