/**
 * @ngdoc directive
 * @name umbraco.directives.directive:umbProperty
 * @restrict E
 **/

(function() {
   'use strict';

   function OverlayDirective($timeout, formHelper) {

      function link(scope, el, attr, ctrl) {

         var modelCopy = {};

         function activate() {

            var cssClass = "umb-overlay-center";

            if (scope.position) {
               cssClass = "umb-overlay-" + scope.position;
            }

            if (scope.animation) {
               cssClass += " " + scope.animation;
            }

            var shadow = "shadow-depth-3";

            if (scope.shadow) {
               shadow = "shadow-depth-" + scope.shadow;
            }

            cssClass += " " + shadow;

            scope.overlayCssClass = cssClass;

            modelCopy = makeModelCopy(scope.model);

            setOverlayIndent();

         }

         function makeModelCopy(object) {

            var newObject = {};

            for (var key in object) {
               if (key !== "event") {
                  newObject[key] = angular.copy(object[key]);
               }
            }

            return newObject;

         }

         function setOverlayIndent() {

            var firstOverlayWidth = null;

            $timeout(function() {
               $(".umb-overlay").each(function(index) {

                  var overlay = $(this);
                  var subtract = index * 20;

                  if (index === 0) {
                     firstOverlayWidth = overlay.context.clientWidth;
                  }

                  var overlayNewWidth = Math.floor(firstOverlayWidth - (index * 20));

                  overlay.css('width', overlayNewWidth);

               });
            });

         }

         function setTargetPosition() {

            var viewportWidth = null;
            var viewportHeight = null;
            var mousePositionClickX = null;
            var mousePositionClickY = null;
            var elementHeight = null;
            var elementWidth = null;

            var position = {
               right: "inherit",
               left: "inherit",
               top: "inherit",
               bottom: "inherit"
            };

            // if mouse click position is know place element with mouse in center
            if (scope.model.event.pageX && scope.model.event.pageY) {

               // viewport size
               viewportWidth = $(window).innerWidth();
               viewportHeight = $(window).innerHeight();

               // click position
               mousePositionClickX = scope.model.event.pageX;
               mousePositionClickY = scope.model.event.pageY;

               // element size
               elementHeight = el.context.clientHeight;
               elementWidth = el.context.clientWidth;

               // move element to this position
               position.left = mousePositionClickX - (elementWidth / 2);
               position.top = mousePositionClickY - (elementHeight / 2);

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

               el.css(position);

               // else change overlay to center position
            } else {

               scope.position = "center";
               activate();

            }

         }

         activate();

         $timeout(function() {
            if (scope.position === "target") {
               setTargetPosition();
            }
         });

         scope.submitForm = function(model) {

            if (formHelper.submitForm({
                  scope: scope
               })) {

               formHelper.resetForm({
                  scope: scope
               });

               scope.model.submit(model);

            }

         };

         scope.closeOverLay = function() {
            if (scope.model.close) {
               scope.model = modelCopy;
               scope.model.close(scope.model);
            } else {
               scope.model = null;
            }
         };

      }

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/overlays/umb-overlay.html',
         scope: {
            model: "=",
            view: "=",
            position: "@",
            animation: "@",
            shadow: "@"
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbOverlay', OverlayDirective);

})();
