/**
@ngdoc directive
@name umbraco.directives.directive:umbToggle
@restrict E
@scope

@description
<b>Added in Umbraco version 7.7.0</b> Use this directive to render an umbraco toggle.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-toggle
            checked="vm.checked"
            on-click="vm.toggle()">
        </umb-toggle>

        <umb-toggle
            checked="vm.checked"
            disabled="vm.disabled"
            on-click="vm.toggle()"
            show-labels="true"
            label-on="Start"
            label-off="Stop"
            label-position="right"
            hide-icons="true">
        </umb-toggle>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;
            vm.checked = false;
            vm.disabled = false;

            vm.toggle = toggle;

            function toggle() {
                vm.checked = !vm.checked;
            }
        }

        angular.module("umbraco").controller("My.Controller", Controller);

    })();
</pre>

@param {boolean} checked Set to <code>true</code> or <code>false</code> to toggle the switch.
@param {callback} onClick The function which should be called when the toggle is clicked.
@param {string=} showLabels Set to <code>true</code> or <code>false</code> to show a "On" or "Off" label next to the switch.
@param {string=} labelOn Set a custom label for when the switched is turned on. It will default to "On".
@param {string=} labelOff Set a custom label for when the switched is turned off. It will default to "Off".
@param {string=} labelPosition Sets the label position to the left or right of the switch. It will default to "left" ("left", "right").
@param {string=} hideIcons Set to <code>true</code> or <code>false</code> to hide the icons on the switch.

**/

(function () {
    'use strict';

    function ToggleDirective(localizationService, eventsService) {

        function link(scope, el, attr, ctrl) {

            scope.displayLabelOn = "";
            scope.displayLabelOff = "";

            function onInit() {
                setLabelText();
                eventsService.emit("toggleValue", { value: scope.checked });
            }

            function setLabelText() {

                // set default label for "on"
                if (scope.labelOn) {
                    scope.displayLabelOn = scope.labelOn;
                } else {
                    localizationService.localize("general_on").then(function (value) {
                        scope.displayLabelOn = value;
                    });
                }

                // set default label for "Off"
                if (scope.labelOff) {
                    scope.displayLabelOff = scope.labelOff;
                } else {
                    localizationService.localize("general_off").then(function (value) {
                        scope.displayLabelOff = value;
                    });
                }

            }

            scope.click = function() {
                if (scope.onClick) {
                    eventsService.emit("toggleValue", { value: !scope.checked });
                    scope.onClick();
                }
            };

            onInit();

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/buttons/umb-toggle.html',
            scope: {
                checked: "=",
                disabled: "=",
                onClick: "&",
                labelOn: "@?",
                labelOff: "@?",
                labelPosition: "@?",
                showLabels: "@?",
                hideIcons: "@?"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbToggle', ToggleDirective);

})();



