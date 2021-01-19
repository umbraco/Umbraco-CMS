﻿/**
@ngdoc directive
@name umbraco.directives.directive:umbColorPicker
@restrict E
@scope

@description
<strong>Added in Umbraco v. 8.10:</strong> Use this directive to render a color picker.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.ColorController as vm">
        
        <umb-color-picker
            options="vm.options"
            on-show="vm.show(color)"
            on-hide="vm.hide(color)"
            on-change="vm.change(color)">
        </umb-color-picker>
    
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";
    
        function Controller() {
    
            var vm = this;

            vm.options = {
                type: "color",
                color: defaultColor,
                showAlpha: false,
                showPalette: true,
                showPaletteOnly: false,
                preferredFormat: "hex",
            };
            
            vm.show = show;
            vm.hide = hide;
            vm.change = change;

            function show(color) {
                color.toHexString().trimStart("#");
            }

            function hide(color) {
                color.toHexString().trimStart("#");
            }

            function change(color) {
                color.toHexString().trimStart("#");
            }
        }
    
        angular.module("umbraco").controller("My.ColorController", Controller);
    
    })();
</pre>

@param {string} ngModel (<code>binding</code>): Value for the color picker.
@param {object} options (<code>binding</code>): Config object for the color picker.
@param {function} onBeforeShow (<code>expression</code>): Callback function before color picker is shown.
@param {function} onChange (<code>expression</code>): Callback function when the color is changed.
@param {function} onShow (<code>expression</code>): Callback function when color picker is shown.
@param {function} onHide (<code>expression</code>): Callback function when color picker is hidden.
@param {function} onMove (<code>expression</code>): Callback function when the color is moved in color picker.

**/

(function () {
    'use strict';

    function ColorPickerController($scope, $element, $timeout, assetsService, localizationService) {

        const ctrl = this;

        let colorPickerInstance = null;
        let labels = {};

        ctrl.$onInit = function () {

            // load the separate css for the editor to avoid it blocking our js loading
            assetsService.loadCss("lib/spectrum/spectrum.css", $scope);

            // load the js file for the color picker
            assetsService.load([
                //"lib/spectrum/tinycolor.js",
                "lib/spectrum/spectrum.js"
            ], $scope).then(function () {

                // init color picker
                grabElementAndRun();
            });
        }

        ctrl.$onChanges = function (changes) {
            if (colorPickerInstance && changes.ngModel) {
                colorPickerInstance.spectrum("set", changes.ngModel.currentValue);
            }
        }

        function grabElementAndRun() {

            var labelKeys = [
                "general_cancel",
                "general_choose",
                "general_clear"
            ];

            localizationService.localizeMany(labelKeys).then(values => {
                labels.cancel = values[0];
                labels.choose = values[1];
                labels.clear = values[2];
            });

            $timeout(function () {
                const element = $element.find('.umb-color-picker > input')[0];
                setColorPicker(element, labels);
            }, 0, true);

        }

        function setColorPicker(element, labels) {

            // Spectrum options: https://seballot.github.io/spectrum/#options

            const defaultOptions = {
                type: "color",
                color: null,
                showAlpha: false,
                showInitial: false,
                showInput: true,
                cancelText: labels.cancel,
                clearText: labels.clear,
                chooseText: labels.choose,
                preferredFormat: "hex",
                clickoutFiresChange: true
            };

            // If has ngModel set the color
            if (ctrl.ngModel) {
                defaultOptions.color = ctrl.ngModel;
            }

            //const options = ctrl.options ? ctrl.options : defaultOptions;
            const options = Utilities.extend(defaultOptions, ctrl.options);

            var elem = angular.element(element);

            // Create new color pickr instance
            const colorPicker = elem.spectrum(options);

            colorPickerInstance = colorPicker;

            if (colorPickerInstance) {
                // destroy the color picker instance when the dom element is removed
                elem.on('$destroy', function () {
                    colorPickerInstance.spectrum('destroy');
                });
            }

            setUpCallbacks();

            // Refresh the scope
            $scope.$applyAsync();
        }

        // Spectrum events: https://seballot.github.io/spectrum/#events

        function setUpCallbacks() {

            if (colorPickerInstance) {

                // bind hook for beforeShow
                if (ctrl.onBeforeShow) {
                    colorPickerInstance.on('beforeShow.spectrum', (e, tinycolor) => {
                        $timeout(function () {
                            ctrl.onBeforeShow({ color: tinycolor });
                        });
                    });
                }

                // bind hook for show
                if (ctrl.onShow) {
                    colorPickerInstance.on('show.spectrum', (e, tinycolor) => {
                        $timeout(function () {
                            ctrl.onShow({ color: tinycolor });
                        });
                    });
                }

                // bind hook for hide
                if (ctrl.onHide) {
                    colorPickerInstance.on('hide.spectrum', (e, tinycolor) => {
                        $timeout(function () {
                            ctrl.onHide({ color: tinycolor });
                        });
                    });
                }

                // bind hook for change
                if (ctrl.onChange) {
                    colorPickerInstance.on('change.spectrum', (e, tinycolor) => {
                        $timeout(function () {
                            ctrl.onChange({ color: tinycolor });
                        });
                    });
                }

                // bind hook for move
                if (ctrl.onMove) {
                    colorPickerInstance.on('move.spectrum', (e, tinycolor) => {
                        $timeout(function () {
                            ctrl.onMove({ color: tinycolor });
                        });
                    });
                }

            }
        }
    }

    angular
        .module('umbraco.directives')
        .component('umbColorPicker', {
            template: '<div class="umb-color-picker"><input type="hidden" /></div>',
            controller: ColorPickerController,
            bindings: {
                ngModel: '<',
                options: '<',
                onBeforeShow: '&',
                onShow: '&',
                onHide: '&',
                onChange: '&',
                onMove: '&'
            }
        });

})();
