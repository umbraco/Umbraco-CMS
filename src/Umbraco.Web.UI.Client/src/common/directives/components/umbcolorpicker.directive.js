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
                const element = $element.find('.umb-color-picker input')[0];
                setColorPicker(element, labels);
            }, 0, true);

        }

        function setColorPicker(element, labels) {

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

            // Create new color pickr instance
            const colorPicker = elem.spectrum(options);

            colorPickerInstance = colorPicker;

            console.log("colorPickerInstance", colorPickerInstance);

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

                console.log("setUpCallbacks", colorPickerInstance);

                // bind hook for beforeShow
                if (ctrl.onBeforeShow) {
                    colorPickerInstance.on('beforeShow.spectrum', (e, color) => {
                        $timeout(function () {
                            console.log("beforeShow", color);
                            ctrl.onBeforeShow({ color: color });
                        });
                    });
                }

                // bind hook for show
                if (ctrl.onShow) {
                    colorPickerInstance.on('show.spectrum', (e, color) => {
                        $timeout(function () {
                            console.log("onShow", color);
                            ctrl.onShow({ color: color });
                        });
                    });
                }

                // bind hook for hide
                if (ctrl.onHide) {
                    colorPickerInstance.on('hide.spectrum', (e, color) => {
                        $timeout(function () {
                            console.log("onHide", color);
                            ctrl.onHide({ color: color });
                        });
                    });
                }

                // bind hook for change
                if (ctrl.onChange) {
                    colorPickerInstance.on('change.spectrum', (e, color) => {
                        $timeout(function () {
                            console.log("onChange", color);
                            ctrl.onChange({ color: color });
                        });
                    });
                }

                // bind hook for move
                if (ctrl.onMove) {
                    colorPickerInstance.on('move.spectrum', (e, color) => {
                        $timeout(function () {
                            console.log("onMove", color);
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
