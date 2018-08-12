/**
@ngdoc directive
@name umbraco.directives.directive:umbDateTimePicker
@restrict E
@scope

@description
<b>Added in Umbraco version 7.6</b>
This directive is a wrapper of the bootstrap datetime picker version 3.1.3. Use it to render a date time picker.
For extra details about options and events take a look here: https://eonasdan.github.io/bootstrap-datetimepicker/

Use this directive to render a date time picker

<h3>Markup example</h3>
<pre>
	<div ng-controller="My.Controller as vm">

        <umb-date-time-picker
            options="vm.config"
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
                if(event.date && event.date.isValid()) {
                    var date = event.date.format(vm.datePickerConfig.format);
                }
            }

            function datePickerError(event) {
                // handle error
            }

        }

		angular.module("umbraco").controller("My.Controller", Controller);

	})();
</pre>

@param {object} options (<code>binding</code>): Config object for the date picker.
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

            scope.hasTranscludedContent = false;

            function onInit() {
                
                // check for transcluded content so we can hide the defualt markup
                scope.hasTranscludedContent = element.find('.js-datePicker__transcluded-content')[0].children.length > 0;

                // load css file for the date picker
                assetsService.loadCss('lib/datetimepicker/bootstrap-datetimepicker.min.css', scope);
                
                // load the js file for the date picker
                assetsService.loadJs('lib/datetimepicker/bootstrap-datetimepicker.js', scope).then(function () {
                    // init date picker
                    initDatePicker();
                });
            }

            function onHide(event) {
                if (scope.onHide) {
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
                if (scope.onUpdate) {
                    scope.$apply(function(){
                        // callback
                        scope.onUpdate({event: event});
                    });
                }
            }

            function initDatePicker() {
                // Open the datepicker and add a changeDate eventlistener
                element
                    .datetimepicker(scope.options)
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
            transclude: true,
            templateUrl: 'views/components/umb-date-time-picker.html',
            scope: {
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
