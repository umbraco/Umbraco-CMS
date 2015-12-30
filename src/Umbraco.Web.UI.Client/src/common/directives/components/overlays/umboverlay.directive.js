/**
 * @ngdoc directive
 * @name umbraco.directives.directive:umbProperty
 * @restrict E
 **/

(function() {
   'use strict';

   function OverlayDirective($timeout, formHelper, overlayHelper, localizationService) {

      function link(scope, el, attr, ctrl) {

         var overlayNumber = 0;
         var numberOfOverlays = 0;
         var isRegistered = false;

         var modelCopy = {};

         function activate() {

            setView();

            setButtonText();

            registerOverlay();

            modelCopy = makeModelCopy(scope.model);

            $timeout(function() {

               if (scope.position === "target") {
                  setTargetPosition();
               }

               setOverlayIndent();

            });

         }

         function setView() {

            if (scope.view) {

               if (scope.view.indexOf(".html") === -1) {
                  var viewAlias = scope.view.toLowerCase();
                  scope.view = "views/common/overlays/" + viewAlias + "/" + viewAlias + ".html";
               }

            }

         }

         function setButtonText() {
             if (!scope.model.closeButtonLabelKey && !scope.model.closeButtonLabel) {
                 scope.model.closeButtonLabel = localizationService.localize("general_close").then(function (value) {return value;});
             }
             if (!scope.model.submitButtonLabelKey && !scope.model.submitButtonLabel) {
                 scope.model.submitButtonLabel = localizationService.localize("general_submit").then(function (value) {return value;});
             }
         }

         function registerOverlay() {

            overlayNumber = overlayHelper.registerOverlay();

            $(document).bind("keydown.overlay-" + overlayNumber, function(event) {

               if (event.which === 27) {

                  numberOfOverlays = overlayHelper.getNumberOfOverlays();

                  if(numberOfOverlays === overlayNumber) {
                     scope.closeOverLay();
                  }

                  event.preventDefault();
               }

               if (event.which === 13) {

                  numberOfOverlays = overlayHelper.getNumberOfOverlays();

                  if(numberOfOverlays === overlayNumber) {

                     var activeElementType = document.activeElement.tagName;
                     var clickableElements = ["A", "BUTTON"];
                     var submitOnEnter = document.activeElement.hasAttribute("overlay-submit-on-enter");

                     if(clickableElements.indexOf(activeElementType) === 0) {
                        document.activeElement.click();
                        event.preventDefault();
                     } else if(activeElementType === "TEXTAREA" && !submitOnEnter) {


                     } else {
                        scope.$apply(function () {
                           scope.submitForm(scope.model);
                        });
                        event.preventDefault();
                     }

                  }

               }

            });

            isRegistered = true;

         }

         function unregisterOverlay() {

            if(isRegistered) {

               overlayHelper.unregisterOverlay();

               $(document).unbind("keydown.overlay-" + overlayNumber);

               isRegistered = false;
            }

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

            var overlayIndex = overlayNumber - 1;
            var indentSize = overlayIndex * 20;
            var overlayWidth = el.context.clientWidth;

            el.css('width', overlayWidth - indentSize);

            if(scope.position === "center" || scope.position === "target") {
               var overlayTopPosition = el.context.offsetTop;
               el.css('top', overlayTopPosition + indentSize);
            }

         }

         function setTargetPosition() {

            var container = $("#contentwrapper");
            var containerLeft = container[0].offsetLeft;
            var containerRight = containerLeft + container[0].offsetWidth;
            var containerTop = container[0].offsetTop;
            var containerBottom = containerTop + container[0].offsetHeight;

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
            if (scope.model.event && scope.model.event) {

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

               el.css(position);

            }

         }

         scope.submitForm = function(model) {

            if(scope.model.submit) {

               if (formHelper.submitForm({scope: scope})) {

                  formHelper.resetForm({ scope: scope });

                  unregisterOverlay();

                  scope.model.submit(model);

               }

            }

         };

         scope.closeOverLay = function() {

            unregisterOverlay();

            if (scope.model.close) {
               scope.model = modelCopy;
               scope.model.close(scope.model);
            } else {
                scope.model.show = false;
               scope.model = null;
            }

         };

         // angular does not support ng-show on custom directives
         // width isolated scopes. So we have to make our own.
         if (attr.hasOwnProperty("ngShow")) {
            scope.$watch("ngShow", function(value) {
               if (value) {
                  el.show();
                  activate();
               } else {
                  unregisterOverlay();
                  el.hide();
               }
            });
         } else {
            activate();
         }

         scope.$on('$destroy', function(){
            unregisterOverlay();
         });

      }

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/overlays/umb-overlay.html',
         scope: {
            ngShow: "=",
            model: "=",
            view: "=",
            position: "@"
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbOverlay', OverlayDirective);

})();
