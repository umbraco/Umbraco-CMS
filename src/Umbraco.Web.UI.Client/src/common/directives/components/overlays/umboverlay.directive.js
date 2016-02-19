/**
@ngdoc directive
@name umbraco.directives.directive:umbOverlay
@restrict E
@scope

@description

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <button type="button" ng-click="vm.openOverlay()"></button>

        <umb-overlay
            ng-if="vm.overlay.show"
            model="vm.overlay"
            view="vm.overlay.view"
            position="right">
        </umb-overlay>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {

        "use strict";

        function Controller() {

            var vm = this;

            vm.openOverlay = openOverlay;

            function openOverlay() {

                vm.overlay = {
                    view: "mediapicker",
                    show: true,
                    submit: function(model) {

                        vm.overlay.show = false;
                        vm.overlay = null;
                    },
                    close: function(oldModel) {
                        vm.overlay.show = false;
                        vm.overlay = null;
                    }
                }

            };

        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

<h1>General Options</h1>
Lorem ipsum dolor sit amet..
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tr>
        <td>model.title</td>
        <td>String</td>
        <td>Set the title of the overlay.</td>
    </tr>
    <tr>
        <td>model.subTitle</td>
        <td>String</td>
        <td>Set the subtitle of the overlay.</td>
    </tr>
    <tr>
        <td>model.submitButtonLabel</td>
        <td>String</td>
        <td>Set an alternate submit button text</td>
    </tr>
    <tr>
        <td>model.submitButtonLabelKey</td>
        <td>String</td>
        <td>Set an alternate submit button label key for localized texts</td>
    </tr>
    <tr>
        <td>model.hideSubmitButton</td>
        <td>Boolean</td>
        <td>Hides the submit button</td>
    </tr>
    <tr>
        <td>model.closeButtonLabel</td>
        <td>String</td>
        <td>Set an alternate close button text</td>
    </tr>
    <tr>
        <td>model.closeButtonLabelKey</td>
        <td>String</td>
        <td>Set an alternate close button label key for localized texts</td>
    </tr>
    <tr>
        <td>model.show</td>
        <td>Boolean</td>
        <td>Show/hide the overlay</td>
    </tr>
    <tr>
        <td>model.submit</td>
        <td>Function</td>
        <td>Callback function when the overlay submits. Returns the overlay model object</td>
    </tr>
    <tr>
        <td>model.close</td>
        <td>Function</td>
        <td>Callback function when the overlay closes. Returns a copy of the overlay model object before being modified</td>
    </tr>
</table>


<h1>Content Picker</h1>
Opens a content picker.</br>
<strong>view: </strong>contentpicker
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tr>
        <td>model.multiPicker</td>
        <td>Boolean</td>
        <td>Pick one or multiple items</td>
    </tr>
</table>
<table>
    <thead>
        <tr>
            <th>Returns</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tr>
        <td>model.selection</td>
        <td>Array</td>
        <td>Array of content objects</td>
    </tr>
</table>


<h1>Icon Picker</h1>
Opens an icon picker.</br>
<strong>view: </strong>iconpicker
<table>
    <thead>
        <tr>
            <th>Returns</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tr>
        <td>model.icon</td>
        <td>String</td>
        <td>The icon class</td>
    </tr>
</table>

<h1>Item Picker</h1>
Opens an item picker.</br>
<strong>view: </strong>itempicker
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.availableItems</td>
            <td>Array</td>
            <td>Array of available items</td>
        </tr>
        <tr>
            <td>model.selectedItems</td>
            <td>Array</td>
            <td>Array of selected items. When passed in the selected items will be filtered from the available items.</td>
        </tr>
        <tr>
            <td>model.filter</td>
            <td>Boolean</td>
            <td>Set to false to hide the filter</td>
        </tr>
    </tbody>
</table>
<table>
    <thead>
        <tr>
            <th>Returns</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tr>
        <td>model.selectedItem</td>
        <td>Object</td>
        <td>The selected item</td>
    </tr>
</table>

<h1>Macro Picker</h1>
Opens a media picker.</br>
<strong>view: </strong>macropicker
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.dialogData</td>
            <td>Object</td>
            <td>Object which contains array of allowedMacros. Set to <code>null</code> to allow all.</td>
        </tr>
    </tbody>
</table>
<table>
    <thead>
        <tr>
            <th>Returns</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.macroParams</td>
            <td>Array</td>
            <td>Array of macro params</td>
        </tr>
        <tr>
            <td>model.selectedMacro</td>
            <td>Object</td>
            <td>The selected macro</td>
        </tr>
    </tbody>
</table>

<h1>Media Picker</h1>
Opens a media picker.</br>
<strong>view: </strong>mediapicker
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.multiPicker</td>
            <td>Boolean</td>
            <td>Pick one or multiple items</td>
        </tr>
        <tr>
            <td>model.onlyImages</td>
            <td>Boolean</td>
            <td>Only display files that have an image file-extension</td>
        </tr>
        <tr>
            <td>model.disableFolderSelect</td>
            <td>Boolean</td>
            <td>Disable folder selection</td>
        </tr>
    </tbody>
</table>
<table>
    <thead>
        <tr>
            <th>Returns</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.selectedImages</td>
            <td>Array</td>
            <td>Array of selected images</td>
        </tr>
    </tbody>
</table>

<h1>Member Group Picker</h1>
Opens a member group picker.</br>
<strong>view: </strong>membergrouppicker
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.multiPicker</td>
            <td>Boolean</td>
            <td>Pick one or multiple items</td>
        </tr>
    </tbody>
</table>
<table>
    <thead>
        <tr>
            <th>Returns</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.selectedMemberGroup</td>
            <td>String</td>
            <td>The selected member group</td>
        </tr>
        <tr>
            <td>model.selectedMemberGroups (multiPicker)</td>
            <td>Array</td>
            <td>The selected member groups</td>
        </tr>
    </tbody>
</table>

<h1>Member Picker</h1>
Opens a member picker. </br>
<strong>view: </strong>memberpicker
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.multiPicker</td>
            <td>Boolean</td>
            <td>Pick one or multiple items</td>
        </tr>
    </tbody>
</table>
<table>
    <thead>
        <tr>
            <th>Returns</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.selection</td>
            <td>Array</td>
            <td>Array of selected members/td>
        </tr>
    </tbody>
</table>

<h1>YSOD</h1>
Opens an overlay to show a custom YSOD. </br>
<strong>view: </strong>ysod
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.error</td>
            <td>Object</td>
            <td>Error object</td>
        </tr>
    </tbody>
</table>

@param {object} model Overlay options.
@param {string} view Path to view or one of the default view names.
@param {string} position The overlay position ("left", "right", "center": "target").
**/

(function() {
   'use strict';

   function OverlayDirective($timeout, formHelper, overlayHelper, localizationService) {

      function link(scope, el, attr, ctrl) {

          scope.directive = {
              enableConfirmButton: false
          };

         var overlayNumber = 0;
         var numberOfOverlays = 0;
         var isRegistered = false;

         var modelCopy = {};

         function activate() {

            setView();

            setButtonText();

            modelCopy = makeModelCopy(scope.model);

            $timeout(function() {

               if (scope.position === "target") {
                  setTargetPosition();
               }

               // this has to be done inside a timeout to ensure the destroy
               // event on other overlays is run before registering a new one
               registerOverlay();

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
                 scope.model.closeButtonLabel = localizationService.localize("general_close");
             }
             if (!scope.model.submitButtonLabelKey && !scope.model.submitButtonLabel) {
                 scope.model.submitButtonLabel = localizationService.localize("general_submit");
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

                    if(scope.model.confirmSubmit && scope.model.confirmSubmit.enable && !scope.directive.enableConfirmButton) {
                        scope.model.submit(model, modelCopy, scope.directive.enableConfirmButton);
                    } else {
                        unregisterOverlay();
                        scope.model.submit(model, modelCopy, scope.directive.enableConfirmButton);
                    }

                 }
             }
         };

         scope.cancelConfirmSubmit = function() {
             scope.model.confirmSubmit.show = false;
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
