/**
@ngdoc directive
@name umbraco.directives.directive:umbDateTimePicker
@restrict E
@scope

@description
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
@param {callback} onChange (<code>callback</code>): Change callback.
@param {callback} onError (<code>callback</code>): Error callback.
**/

(function () {
    'use strict';

    function DateTimePickerDirective(assetsService) {

        function link(scope, element, attrs, ctrl) {

            function onInit() {
                // load css file for the date picker
                assetsService.loadCss('lib/datetimepicker/bootstrap-datetimepicker.min.css').then(function () {

                });
                
                // load the js file for the date picker
                assetsService.loadJs('lib/datetimepicker/bootstrap-datetimepicker.js').then(function () {
                    // init date picker
                    initDatePicker();
                });
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

            function initDatePicker() {
                // Open the datepicker and add a changeDate eventlistener
                element
                    .datetimepicker(angular.extend({ useCurrent: true }, scope.options))
                    .on("dp.change", onChange)
                    .on("dp.error", onError);
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
                onChange: "&",
                onError: "&"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDateTimePicker', DateTimePickerDirective);

})();
