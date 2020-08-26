/**
@ngdoc directive
@name umbraco.directives.directive:umbTooltip
@restrict E
@scope

@description
Use this directive to render a tooltip.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <div
            ng-mouseover="vm.mouseOver($event)"
            ng-mouseleave="vm.mouseLeave()">
            Hover me
        </div>

        <umb-tooltip
           ng-if="vm.tooltip.show"
           event="vm.tooltip.event">
           // tooltip content here
        </umb-tooltip>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;
            vm.tooltip = {
                show: false,
                event: null
            };

            vm.mouseOver = mouseOver;
            vm.mouseLeave = mouseLeave;

            function mouseOver($event) {
                vm.tooltip = {
                    show: true,
                    event: $event
                };
            }

            function mouseLeave() {
                vm.tooltip = {
                    show: false,
                    event: null
                };
            }

        }

        angular.module("umbraco").controller("My.Controller", Controller);

    })();
</pre>

@param {string} event Set the $event from the target element to position the tooltip relative to the mouse cursor.
**/

(function() {
   'use strict';

   function TooltipDirective() {

      function link(scope, el, attr, ctrl) {

         scope.tooltipStyles = {};
         scope.tooltipStyles.left = 0;
         scope.tooltipStyles.top = 0;

          function setTooltipPosition(event) {

            var overlay = $(event.target).closest('.umb-overlay');
            var container = overlay.length > 0 ? overlay : $("#contentwrapper");

            let rect = container[0].getBoundingClientRect();

            var containerLeft = rect.left;
            var containerRight = containerLeft + rect.width;
            var containerTop = rect.top;
            var containerBottom = containerTop + rect.height;

            var elementHeight = null;
            var elementWidth = null;

            var position = {
               right: "inherit",
               left: "inherit",
               top: "inherit",
               bottom: "inherit"
            };

            // element size
            elementHeight = el[0].clientHeight;
            elementWidth = el[0].clientWidth;

            position.left = event.pageX - (elementWidth / 2);
            position.top = event.pageY;

            if (overlay.length > 0) {
                position.left = event.pageX - rect.left - (elementWidth / 2);
                position.top = event.pageY - rect.top;
            }
            else {
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
            }

            scope.tooltipStyles = position;

            el.css(position);
         }

         setTooltipPosition(scope.event);
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
