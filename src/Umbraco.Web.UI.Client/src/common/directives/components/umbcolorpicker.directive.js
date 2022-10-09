/**
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
@param {function} onBeforeShow (<code>expression</code>): You can prevent the color picker from showing up if you return false in the beforeShow event. This event is ignored on a flat color picker.
@param {function} onChange (<code>expression</code>): Called as the original input changes. Only happens when the input is closed or the 'Choose' button is clicked.
@param {function} onShow (<code>expression</code>): Called after the color picker is opened. This is ignored on a flat color picker. Note, when any color picker on the page is shown it will hide any that are already open.
@param {function} onHide (<code>expression</code>): Called after the color picker is hidden. This happens when clicking outside of the picker while it is open. Note, when any color picker on the page is shown it will hide any that are already open. This event is ignored on a flat color picker.
@param {function} onMove (<code>expression</code>): Called as the user moves around within the color picker.
@param {function} onDragStart (<code>expression</code>): Called at the beginning of a drag event on either hue slider, alpha slider, or main color picker areas.
@param {function} onDragStop (<code>expression</code>): Called at the end of a drag event on either hue slider, alpha slider, or main color picker areas.

**/

(function () {
    'use strict';

    function ColorPickerController($scope, $element, $timeout, assetsService, localizationService, $attrs) {

        const ctrl = this;

        let colorPickerInstance = null;
        let labels = {};
        let options = null;

        ctrl.readonly = false;
        ctrl.value = null;

        ctrl.$onInit = function () {

            // load the separate css for the editor to avoid it blocking our js loading
            assetsService.loadCss("lib/spectrum/spectrum.min.css", $scope);

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

        $attrs.$observe('readonly', (value) => {
            ctrl.readonly = value !== undefined;

            if (!colorPickerInstance) {
                return;
            }

            if (ctrl.readonly) {
                colorPickerInstance.spectrum('disable');
            } else {
                colorPickerInstance.spectrum('enable');
            }
        });

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
            options = Utilities.extend(defaultOptions, ctrl.options);

            var elem = angular.element(element);

            // Create new color pickr instance
            const colorPicker = elem.spectrum(options);

            colorPickerInstance = colorPicker;

            if (colorPickerInstance) {
                
                if (ctrl.readonly) {
                    colorPickerInstance.spectrum('disable');
                }

                const tinyColor = colorPickerInstance.spectrum("get");
                ctrl.value = getColorString(tinyColor, options.preferredFormat);

                colorPickerInstance.on('change', (e, tinyColor) => {
                    ctrl.value = getColorString(tinyColor, options.preferredFormat);
                    $scope.$applyAsync();
                });

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

                // bind hook for drag start
                if (ctrl.onDragStart) {
                    colorPickerInstance.on('dragstart.spectrum', (e, tinycolor) => {
                        $timeout(function () {
                            ctrl.onDragStart({ color: tinycolor });
                        });
                    });
                }

                // bind hook for drag stop
                if (ctrl.onDragStop) {
                    colorPickerInstance.on('dragstop.spectrum', (e, tinycolor) => {
                        $timeout(function () {
                            ctrl.onDragStop({ color: tinycolor });
                        });
                    });
                }

            }
        }

        function getColorString (tinyColor, format) {
            if (!tinyColor) {
                return;
            }

            switch(format) {
                case 'rgb':
                  return tinyColor.toRgbString();
                case 'hsv':
                  return tinyColor.toHsvString();
                case 'hsl':
                    return tinyColor.toHslString();
                case 'name':
                    return tinyColor.toName();
                default:
                    return tinyColor.toHexString();
              }
        }
    }

    angular
        .module('umbraco.directives')
        .component('umbColorPicker', {
            template: '<div class="flex items-center"><div class="umb-color-picker"><input type="hidden" /></div><small ng-if="$ctrl.readonly">{{ $ctrl.value }}</small></div>',
            controller: ColorPickerController,
            bindings: {
                ngModel: '<',
                options: '<',
                onBeforeShow: '&?',
                onShow: '&?',
                onHide: '&?',
                onChange: '&?',
                onMove: '&?',
                onDragStart: '&?',
                onDragStop: '&?'
            }
        });

})();
