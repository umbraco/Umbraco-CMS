/**
@ngdoc directive
@name umbraco.directives.directive:umbColorPicker
@restrict E
@scope

@description
<strong>Added in Umbraco v. 8.8:</strong> Use this directive to render a color picker.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.ColorController as vm">
        
        <umb-color-picker
            on-select="vm.selectItem"
            on-cancel="vm.cancel">
        </umb-table>
    
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";
    
        function Controller() {
    
            var vm = this;
    
            vm.change = change;

            function change(item, $index, $event) {
                
            }
            
    
        }
    
        angular.module("umbraco").controller("My.ColorController", Controller);
    
    })();
</pre>

@param {function} beforeShow (<code>expression</code>): Callback function before color picker is shown.
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

        ctrl.$onInit = function() {
            
            //load the separate css for the editor to avoid it blocking our js loading
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
                const element = $element.find('.umb-color-picker')[0];
                setColorPicker(element, labels);
            }, 0, true);

        }

        function setColorPicker(element, labels) {

            //colorPickerInstance = element;
            //console.log("colorPickerInstance", colorPickerInstance);

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
            const options = angular.extend({}, defaultOptions, ctrl.options);

            var elem = angular.element(element);

            // Create new color pickr
            const colorPicker = elem.spectrum(options);
            console.log("colorPicker", colorPicker);

            colorPickerInstance = colorPicker;

            // destroy the color picker instance when the dom element is removed
            elem.on('$destroy', function () {
                colorPickerInstance.destroy();
            });

            setUpCallbacks();

            // Refresh the scope
            $scope.$applyAsync();
        }

        function setUpCallbacks() {
            
            if (colorPickerInstance) {

                // bind hook for beforeShow
                if (ctrl.beforeShow) {
                    colorPickerInstance.on('beforeShow', (color) => {
                        $timeout(function () {
                            ctrl.beforeShow({ color: color });
                        });
                    });
                }

                // bind hook for show
                if (ctrl.onShow) {
                    colorPickerInstance.on('show', (color) => {
                        $timeout(function () {
                            ctrl.onShow({ color: color });
                        });
                    });
                }

                // bind hook for hide
                if (ctrl.onHide) {
                    pickrInstance.on('hide', (color) => {
                        $timeout(function () {
                            ctrl.onHide({ color: color });
                        });
                    });
                }

                // bind hook for change
                if (ctrl.onChange) {
                    colorPickerInstance.on('change', (color) => {
                        $timeout(function () {
                            ctrl.onChange({ color: color });
                        });
                    });
                }

                // bind hook for move
                if (ctrl.onMove) {
                    colorPickerInstance.on('move', (color) => {
                        $timeout(function () {
                            ctrl.onMove({ color: color });
                        });
                    });
                }

            }
        }
    }

    angular
        .module('umbraco.directives')
        .component('umbColorPicker', {
            template: '<div class="umb-color-picker"></div>',
            controller: ColorPickerController,
            controllerAs: 'vm',
            bindings: {
                ngModel: '<',
                options: '<',
                beforeShow: '&',
                onShow: '&',
                onHide: '&',
                onChange: '&',
                onMove: '&'
            }
        });

})();
