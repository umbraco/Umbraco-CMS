/**
@ngdoc directive
@name umbraco.directives.directive:umbDateTimePicker
@restrict E
@scope

@description
This directive is a wrapper of the bootstrap datetime picker version 3.1.3. Use it to render a date time picker.
For extra details about settings and events take a look here: https://github.com/Eonasdan/bootstrap-datetimepicker

Use this directive to render a date time picker

<h3>Markup example</h3>
<pre>
	<div ng-controller="My.Controller as vm">

        <umb-date-time-picker
            options="vm.config"
            ng-model="vm.date"
            on-change="vm.datePickerChange(event)"
            on-error="vm.datePickerError(event)">
        </umb-date-time-picker>

	</div>
</pre>

<h3>Controller example</h3>
<pre>
	(function () {
		"use strict";

		function Controller() {

            var vm = this;

            vm.date = "";

            vm.config = {
                pickDate: true,
                pickTime: true,
                useSeconds: true,
                format: "YYYY-MM-DD HH:mm:ss",
                icons: {
                    time: "icon-time",
                    date: "icon-calendar",
                    up: "icon-chevron-up",
                    down: "icon-chevron-down"
                }
            };

            vm.datePickerChange = datePickerChange;
            vm.datePickerError = datePickerError;

            function datePickerChange(event) {
                // handle change
            }

            function datePickerError(event) {
                // handle error
            }

        }

		angular.module("umbraco").controller("My.Controller", Controller);

	})();
</pre>

@param {object} options (<code>binding</code>): Config object for the date picker.
@param {string} ngModel (<code>binding</code>): Date value.
@param {callback} onHide (<code>callback</code>): Hide callback.
@param {callback} onShow (<code>callback</code>): Show callback.
@param {callback} onChange (<code>callback</code>): Change callback.
@param {callback} onError (<code>callback</code>): Error callback.
@param {callback} onUpdate (<code>callback</code>): Update callback.
**/

(function () {
    'use strict';

    function DateTimePickerDirective(assetsService) {

        function link(scope, element, attrs, ctrl) {

            function onInit() {
                // load css file for the date picker
                assetsService.loadCss('lib/datetimepicker/bootstrap-datetimepicker.min.css');
                
                // load the js file for the date picker
                assetsService.loadJs('lib/datetimepicker/bootstrap-datetimepicker.js').then(function () {
                    // init date picker
                    initDatePicker();
                });
            }

            function onHide(event) {
                if (scope.onChange) {
                    scope.$apply(function(){
                        // callback
                        scope.onHide({event: event});
                    });
                }
            }

            function onShow() {
                if (scope.onShow) {
                    scope.$apply(function(){
                        // callback
                        scope.onShow();
                    });
                }
            }

            function onChange(event) {
                if (scope.onChange && event.date && event.date.isValid()) {
                    scope.$apply(function(){
                        // Update ngModel
                        scope.ngModel = event.date.format(scope.options.format);
                        // callback
                        scope.onChange({event: event});
                    });
                }
            }

            function onError(event) {
                if (scope.onError) {
                    scope.$apply(function(){
                        // callback
                        scope.onError({event:event});
                    });
                }
            }

            function onUpdate(event) {
                if (scope.onShow) {
                    scope.$apply(function(){
                        // callback
                        scope.onUpdate({event: event});
                    });
                }
            }

            function initDatePicker() {
                // Open the datepicker and add a changeDate eventlistener
                element
                    .datetimepicker(angular.extend({ useCurrent: true }, scope.options))
                    .on("dp.hide", onHide)
                    .on("dp.show", onShow)
                    .on("dp.change", onChange)
                    .on("dp.error", onError)
                    .on("dp.update", onUpdate);
            }

            onInit();

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-date-time-picker.html',
            scope: {
                ngModel: "=",
                options: "=",
                onHide: "&",
                onShow: "&",
                onChange: "&",
                onError: "&",
                onUpdate: "&"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDateTimePicker', DateTimePickerDirective);

})();
