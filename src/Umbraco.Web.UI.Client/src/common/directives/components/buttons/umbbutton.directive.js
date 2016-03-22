/**
@ngdoc directive
@name umbraco.directives.directive:umbButton
@restrict E
@scope

@description
Use this directive to render an umbraco button. The directive can be used to generate all types of buttons, set type, style, translation, shortcut and much more.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-button
            action="vm.clickButton()"
            type="button"
            button-style="success"
            state="vm.buttonState"
            shortcut="ctrl+c"
            label="My button"
            disabled="vm.buttonState === 'busy'">
        </umb-button>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller(myService) {

            var vm = this;
            vm.buttonState = "init";

            vm.clickButton = clickButton;

            function clickButton() {

                vm.buttonState = "busy";

                myService.clickButton().then(function() {
                    vm.buttonState = "success";
                }, function() {
                    vm.buttonState = "error";
                });

            }
        }

        angular.module("umbraco").controller("My.Controller", Controller);

    })();
</pre>

@param {callback} action The button action which should be performed when the button is clicked.
@param {string=} href Url/Path to navigato to.
@param {string=} type Set the button type ("button" or "submit").
@param {string=} buttonStyle Set the style of the button. The directive uses the default bootstrap styles ("primary", "info", "success", "warning", "danger", "inverse", "link").
@param {string=} state Set a progress state on the button ("init", "busy", "success", "error").
@param {string=} shortcut Set a keyboard shortcut for the button ("ctrl+c").
@param {string=} label Set the button label.
@param {string=} labelKey Set a localization key to make a multi lingual button ("general_buttonText").
@param {string=} icon Set a button icon. Can only be used when buttonStyle is "link".
@param {boolean=} disabled Set to <code>true</code> to disable the button.
**/

(function() {
   'use strict';

   function ButtonDirective($timeout) {

      function link(scope, el, attr, ctrl) {

         scope.style = null;

         function activate() {

            if (!scope.state) {
               scope.state = "init";
            }

            if (scope.buttonStyle) {
               scope.style = "btn-" + scope.buttonStyle;
            }

         }

         activate();

         var unbindStateWatcher = scope.$watch('state', function(newValue, oldValue) {

            if (newValue === 'success' || newValue === 'error') {
               $timeout(function() {
                  scope.state = 'init';
               }, 2000);
            }

         });

         scope.$on('$destroy', function() {
            unbindStateWatcher();
         });

      }

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/buttons/umb-button.html',
         link: link,
         scope: {
            action: "&?",
            href: "@?",
            type: "@",
            buttonStyle: "@?",
            state: "=?",
            shortcut: "@?",
            shortcutWhenHidden: "@",
            label: "@?",
            labelKey: "@?",
            icon: "@?",
            disabled: "="
         }
      };

      return directive;

   }

   angular.module('umbraco.directives').directive('umbButton', ButtonDirective);

})();
