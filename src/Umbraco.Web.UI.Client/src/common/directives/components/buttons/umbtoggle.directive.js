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
@param {string} inputId Set the <code>id</code> of the toggle.
@param {callback} onClick The function which should be called when the toggle is clicked.
@param {string=} showLabels Set to <code>true</code> or <code>false</code> to show a "On" or "Off" label next to the switch.
@param {string=} labelOn Set a custom label for when the switched is turned on. It will default to "On".
@param {string=} labelOff Set a custom label for when the switched is turned off. It will default to "Off".
@param {string=} labelPosition Sets the label position to the left or right of the switch. It will default to "left" ("left", "right").
@param {string=} hideIcons Set to <code>true</code> or <code>false</code> to hide the icons on the switch.

**/

(function () {
    'use strict';

    function ToggleDirective(localizationService, eventsService, $timeout) {

        function link(scope, el, attrs, ctrl) {

            scope.displayLabelOn = "";
            scope.displayLabelOff = "";

            function onInit() {
                scope.inputId = scope.inputId || "umb-toggle_" + String.CreateGuid();

                setLabelText();

                // Must wait until the current digest cycle is finished before we emit this event on init, 
                // otherwise other property editors might not yet be ready to receive the event
                $timeout(function () {
                    eventsService.emit("toggleValue", { value: scope.checked });
                }, 100);
            }

            function setLabelText() {
                
                if (scope.labelOn) {
                    scope.displayLabelOn = scope.labelOn;
                }
                
                if (scope.labelOff) {
                    scope.displayLabelOff = scope.labelOff;
                }

                if (scope.displayLabelOn.length === 0 && scope.displayLabelOff.length === 0)
                {
                    var labelKeys = [
                        "general_on",
                        "general_off"
                    ];

                    localizationService.localizeMany(labelKeys).then(function (data) {
                        // Set default label for "On"
                        scope.displayLabelOn = data[0];

                        // Set default label for "Off"
                        scope.displayLabelOff = data[1];
                    });
                }

            }

            scope.click = function($event) {
                if (scope.readonly) {
                    $event.preventDefault();
                    $event.stopPropagation();
                    return;
                }

                if (scope.onClick) {
                    eventsService.emit("toggleValue", { value: !scope.checked });
                    scope.onClick();
                }
            };

            attrs.$observe('readonly', (value) => {
                scope.readonly = value !== undefined;
            });

            onInit();
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/buttons/umb-toggle.html',
            scope: {
                // TODO: This should have required ngModel so we can track and validate user input correctly
                // https://docs.angularjs.org/api/ng/type/ngModel.NgModelController#custom-control-example
                checked: "=",
                disabled: "=",
                inputId: "@",
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



