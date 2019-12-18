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

        function link(scope) {

            scope.dropdown = {
                isOpen: false
            };

            scope.toggleDropdown = function() {
                scope.dropdown.isOpen = !scope.dropdown.isOpen;
            };

            scope.closeDropdown = function() {
                scope.dropdown.isOpen = false;
            };

            scope.executeMenuItem = function(subButton) {
                subButton.handler();
                scope.closeDropdown();
            };

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/buttons/umb-button-group.html',
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
                labelKey: "@?"
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbButtonGroup', ButtonGroupDirective);

})();
