/**
@ngdoc directive
@name umbraco.directives.directive:umbDateTimePicker
@restrict E
@scope

@description
<b>Added in Umbraco version 8.0</b>
This directive is a wrapper of the flatpickr library. Use it to render a date time picker.
For extra details about options and events take a look here: https://flatpickr.js.org

Use this directive to render a date time picker

<h3>Markup example</h3>
<pre>
	<div ng-controller="My.Controller as vm">

		<umb-date-time-picker
			ng-model="vm.date"
            options="vm.config"
            on-change="vm.datePickerChange(selectedDates, dateStr, instance)">
        </umb-date-time-picker>

	</div>
</pre>

<h3>Controller example</h3>
<pre>
	(function () {
		"use strict";

		function Controller() {

            var vm = this;

            vm.date = "2018-10-10 10:00";

            vm.config = {
				enableTime: true,
				dateFormat: "Y-m-d H:i",
				time_24hr: true
            };

            vm.datePickerChange = datePickerChange;

            function datePickerChange(selectedDates, dateStr, instance) {
            	// handle change
            }

        }

		angular.module("umbraco").controller("My.Controller", Controller);

	})();
</pre>

@param {object} ngModel (<code>binding</code>): Config object for the date picker.
@param {object} options (<code>binding</code>): Config object for the date picker.
@param {callback} onSetup (<code>callback</code>): onSetup gets triggered when the date picker is initialized
@param {callback} onChange (<code>callback</code>): onChange gets triggered when the user selects a date, or changes the time on a selected date.
@param {callback} onOpen (<code>callback</code>): onOpen gets triggered when the calendar is opened.
@param {callback} onClose (<code>callback</code>): onClose gets triggered when the calendar is closed.
@param {callback} onMonthChange (<code>callback</code>): onMonthChange gets triggered when the month is changed, either by the user or programmatically.
@param {callback} onYearChange (<code>callback</code>): onMonthChange gets triggered when the year is changed, either by the user or programmatically.
@param {callback} onReady (<code>callback</code>): onReady gets triggered once the calendar is in a ready state.
@param {callback} onValueUpdate (<code>callback</code>): onValueUpdate gets triggered when the input value is updated with a new date string.
@param {callback} onDayCreate (<code>callback</code>): Take full control of every date cell with theonDayCreate()hook.
**/

(function () {
    'use strict';

    var umbDateTimePicker = {
        template: '<ng-transclude>' +
            '<input type="text" ng-if="!$ctrl.options.inline" ng-model="$ctrl.ngModel" placeholder="Select Date.."></input>' +
            '<div ng-if="$ctrl.options.inline"></div>' +
            '</ng-transclude>',
        controller: umbDateTimePickerCtrl,
        transclude: true,
        bindings: {
            ngModel: '<',
            options: '<',
            onSetup: '&?',
            onChange: '&?',
            onOpen: '&?',
            onClose: '&?',
            onMonthChange: '&?',
            onYearChange: '&?',
            onReady: '&?',
            onValueUpdate: '&?',
            onDayCreate: '&?'
        }
    };

    function umbDateTimePickerCtrl($element, $timeout, $scope, assetsService, userService) {

        var ctrl = this;
        var userLocale = null;

        ctrl.$onInit = function () {

            // load css file for the date picker
            assetsService.loadCss('lib/flatpickr/flatpickr.min.css', $scope).then(function () {
                userService.getCurrentUser().then(function (user) {

                    // init date picker
                    userLocale = user.locale;
                    if (userLocale.indexOf('-') > -1) {
                        userLocale = userLocale.split('-')[0];
                    }

                    grabElementAndRunFlatpickr();
                });
            });

        };

        function grabElementAndRunFlatpickr() {
            $timeout(function () {
                var transcludeEl = $element.find('ng-transclude')[0];
                var element = transcludeEl.children[0];

                setDatepicker(element);
            }, 0, true);
        }

        function setDatepicker(element) {
            var fpLib = flatpickr ? flatpickr : FlatpickrInstance;

            if (!fpLib) {
                return console.warn('Unable to find any flatpickr installation');
            }

            var fpInstance;

            setUpCallbacks();

            if (!ctrl.options.locale) {
                ctrl.options.locale = userLocale;
            }

            // handle special keydown events
            ctrl.options.onKeyDown = function (selectedDates, dateStr, instance, event) {
                var code = event.keyCode || event.which;
                if (code === 13) {
                    // close the datepicker on enter (this happens when entering time)
                    fpInstance.close()
                }
            };

            fpInstance = new fpLib(element, ctrl.options);

            if (ctrl.onSetup) {
                ctrl.onSetup({
                    fpItem: fpInstance
                });
            }

            // If has ngModel set the date
            if (ctrl.ngModel) {
                fpInstance.setDate(ctrl.ngModel);
            }

            // destroy the flatpickr instance when the dom element is removed
            $(element).on('$destroy', function () {
                fpInstance.destroy();
            });

            // Refresh the scope
            $scope.$applyAsync();
        }

        function setUpCallbacks() {
            // bind hook for onChange
            if (ctrl.options && ctrl.onChange) {
                ctrl.options.onChange = function (selectedDates, dateStr, instance) {
                    $timeout(function () {
                        ctrl.onChange({ selectedDates: selectedDates, dateStr: dateStr, instance: instance });
                    });
                };
            }

            // bind hook for onOpen
            if (ctrl.options && ctrl.onOpen) {
                ctrl.options.onOpen = function (selectedDates, dateStr, instance) {
                    $timeout(function () {
                        ctrl.onOpen({ selectedDates: selectedDates, dateStr: dateStr, instance: instance });
                    });
                };
            }

			// bind hook for onOpen
            if (ctrl.options && ctrl.onClose) {
                ctrl.options.onClose = function (selectedDates, dateStr, instance) {
                    $timeout(function () {
                        ctrl.onClose({ selectedDates: selectedDates, dateStr: dateStr, instance: instance });
                    });
                };
            }

            // bind hook for onMonthChange
            if (ctrl.options && ctrl.onMonthChange) {
                ctrl.options.onMonthChange = function (selectedDates, dateStr, instance) {
                    $timeout(function () {
                        ctrl.onMonthChange({ selectedDates: selectedDates, dateStr: dateStr, instance: instance });
                    });
                };
            }

            // bind hook for onYearChange
            if (ctrl.options && ctrl.onYearChange) {
                ctrl.options.onYearChange = function (selectedDates, dateStr, instance) {
                    $timeout(function () {
                        ctrl.onYearChange({ selectedDates: selectedDates, dateStr: dateStr, instance: instance });
                    });
                };
            }

            // bind hook for onReady
            if (ctrl.options && ctrl.onReady) {
                ctrl.options.onReady = function (selectedDates, dateStr, instance) {
                    $timeout(function () {
                        ctrl.onReady({ selectedDates: selectedDates, dateStr: dateStr, instance: instance });
                    });
                };
            }

            // bind hook for onValueUpdate
            if (ctrl.onValueUpdate) {
                ctrl.options.onValueUpdate = function (selectedDates, dateStr, instance) {
                    $timeout(function () {
                        ctrl.onValueUpdate({ selectedDates: selectedDates, dateStr: dateStr, instance: instance });
                    });
                };
            }

            // bind hook for onDayCreate
            if (ctrl.onDayCreate) {
                ctrl.options.onDayCreate = function (selectedDates, dateStr, instance) {
                    $timeout(function () {
                        ctrl.onDayCreate({ selectedDates: selectedDates, dateStr: dateStr, instance: instance });
                    });
                };
            }

        }
    }

    // umbFlatpickr (umb-flatpickr) is deprecated, but we keep it for backwards compatibility
    angular.module('umbraco.directives').component('umbFlatpickr', umbDateTimePicker);
    angular.module('umbraco.directives').component('umbDateTimePicker', umbDateTimePicker);
})();
