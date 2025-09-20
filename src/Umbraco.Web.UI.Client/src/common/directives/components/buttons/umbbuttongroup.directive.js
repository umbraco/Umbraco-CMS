/**
@ngdoc directive
@name umbraco.directives.directive:umbButtonGroup
@restrict E
@scope

@description
Use this directive to render a button with a dropdown of alternative actions.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-button-group
           ng-if="vm.buttonGroup"
           default-button="vm.buttonGroup.defaultButton"
           sub-buttons="vm.buttonGroup.subButtons"
           direction="down"
           float="right">
        </umb-button-group>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;

            vm.buttonGroup = {
                defaultButton: {
                    labelKey: "general_defaultButton",
                    hotKey: "ctrl+d",
                    hotKeyWhenHidden: true,
                    handler: function() {
                        // do magic here
                    }
                },
                subButtons: [
                    {
                        labelKey: "general_subButton",
                        hotKey: "ctrl+b",
                        hotKeyWhenHidden: true,
                        handler: function() {
                            // do magic here
                        }
                    }
                ]
            };
        }

        angular.module("umbraco").controller("My.Controller", Controller);

    })();
</pre>

<h3>Button model description</h3>
<ul>
    <li>
        <strong>labekKey</strong>
        <small>(string)</small> -
        Set a localization key to make a multi lingual button ("general_buttonText").
    </li>
    <li>
        <strong>hotKey</strong>
        <small>(array)</small> -
        Set a keyboard shortcut for the button ("ctrl+c").
    </li>
    <li>
        <strong>hotKeyWhenHidden</strong>
        <small>(boolean)</small> -
        As a default the hotkeys only works on elements visible in the UI. Set to <code>true</code> to set a hotkey on the hidden sub buttons.
    </li>
    <li>
        <strong>handler</strong>
        <small>(callback)</small> -
        Set a callback to handle button click events.
    </li>
</ul>

@param {object} defaultButton The model of the default button.
@param {array} subButtons Array of sub buttons.
@param {string=} state Set a progress state on the button ("init", "busy", "success", "error").
@param {string=} direction Set the direction of the dropdown ("up", "down").
@param {string=} float Set the float of the dropdown. ("left", "right").
**/
(function () {
  'use strict';
  function ButtonGroupDirective() {
    function controller($scope, localizationService) {
      $scope.toggleStyle = null;
      $scope.blockElement = false;

      var buttonStyle = $scope.buttonStyle;
      if (buttonStyle) {
        // Make it possible to pass in multiple styles
        if (buttonStyle.startsWith("[") && buttonStyle.endsWith("]")) {
          // when using an attr it will always be a string so we need to remove square brackets and turn it into and array
          var withoutBrackets = buttonStyle.replace(/[\[\]']+/g, '');
          // split array by , + make sure to catch whitespaces
          var array = withoutBrackets.split(/\s?,\s?/g);

          Utilities.forEach(array, item => {
            if (item === "block") {
              $scope.blockElement = true;
            } else {
              $scope.toggleStyle = ($scope.toggleStyle ? $scope.toggleStyle + " " : "") + "btn-" + item;
            }
          });
        } else {
          if (buttonStyle === "block") {
            $scope.blockElement = true;
          } else {
            $scope.toggleStyle = "btn-" + buttonStyle;
          }
        }
      }

      // As the <localize /> directive doesn't support Angular expressions as fallback, we instead listen for changes
      // to the label key of the default button, and if detected, we update the button label with the localized value
      // received from the localization service
      $scope.$watch("defaultButton", localizeDefaultButtonLabel);
      $scope.$watch("defaultButton.labelKey", localizeDefaultButtonLabel);

      function localizeDefaultButtonLabel() {
        if (!$scope.defaultButton?.labelKey) return;
        localizationService.localize($scope.defaultButton.labelKey).then(value => {
          if (value && value.indexOf("[") === 0) return;
          $scope.defaultButton.label = value;
        });
      }

      // In a similar way, we must listen for changes to the sub buttons (or their label keys), and if detected, update
      // the label with the localized value received from the localization service
      $scope.$watch("subButtons", localizeSubButtons, true);
      $scope.$watch("defaultButton.subButtons", localizeSubButtons, true);

      function localizeSubButtons() {
        if (!$scope.subButtons || !Array.isArray($scope.subButtons)) return;
        $scope.subButtons.forEach(function (sub) {
          if (!sub.labelKey) return;
          localizationService.localize(sub.labelKey).then(value => {
            if (value && value.indexOf("[") === 0) return;
            sub.label = value;
          });
        });
      }

    }

    function link(scope) {
      scope.dropdown = {
        isOpen: false
      };

      scope.toggleDropdown = function () {
        scope.dropdown.isOpen = !scope.dropdown.isOpen;
      };

      scope.closeDropdown = function () {
        scope.dropdown.isOpen = false;
      };

      scope.executeMenuItem = function (subButton) {
        subButton.handler();
        scope.closeDropdown();
      };
    }

    var directive = {
      restrict: 'E',
      replace: true,
      templateUrl: 'views/components/buttons/umb-button-group.html',
      controller: controller,
      scope: {
        defaultButton: "=",
        subButtons: "=",
        state: "=?",
        direction: "@?",
        float: "@?",
        buttonStyle: "@?",
        size: "@?",
        icon: "@?",
        label: "@?",
        labelKey: "@?",
        disabled: "<?"
      },
      link: link
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbButtonGroup', ButtonGroupDirective);
})();
