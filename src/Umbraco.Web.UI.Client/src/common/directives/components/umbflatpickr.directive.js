(function() {
	'use strict';

	var umbFlatpickr = {
		template: '<ng-transclude>' +
			'<input type="text" ng-if="!$ctrl.options.inline" ng-model="$ctrl.ngModel" placeholder="Select Date.."></input>' +
			'<div ng-if="$ctrl.options.inline"></div>' +
		'</ng-transclude>',
		controller: umbFlatpickrCtrl,
		transclude: true,
		bindings: {
			ngModel: '<',
			options: '<',
			onSetup: '&',
			onChange: '&?',
			onOpen: '&?',
			onClose: '&?',
			onMonthChange: '&?'
		}
    };
    
	function umbFlatpickrCtrl($element, $timeout, $scope, assetsService) {
        var ctrl = this;
        var loaded = false;

		ctrl.$onInit = function() {

            // load css file for the date picker
            assetsService.loadCss('lib/flatpickr/flatpickr.css', $scope);

            // load the js file for the date picker
            assetsService.loadJs('lib/flatpickr/flatpickr.js', $scope).then(function () {
                // init date picker
                loaded = true;
                grabElementAndRunFlatpickr();
            });

		};

		ctrl.$onChanges = function() {
            if(loaded) {
                //grabElementAndRunFlatpickr();
            }
        };

		function grabElementAndRunFlatpickr() {
			$timeout(function() {
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

			setUpCallbacks();

            var fpInstance = new fpLib(element, ctrl.options);
            
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
			angular.element(element).on('$destroy', function() {
				fpInstance.destroy();
			});

			// Refresh the scope
			$scope.$applyAsync();
		}

		function setUpCallbacks() {
			// bind hook for onChange
			if(ctrl.options && ctrl.onChange) {
				ctrl.options.onChange = function(selectedDates, dateStr, instance) {
					$timeout(function() {
						ctrl.onChange({selectedDates: selectedDates, dateStr: dateStr, instance: instance});
					});
				};
			}

			// bind hook for onOpen
			if(ctrl.options && ctrl.onOpen) {
				ctrl.options.onOpen = function(selectedDates, dateStr, instance) {
					$timeout(function() {
						ctrl.onOpen({selectedDates: selectedDates, dateStr: dateStr, instance: instance});
					});
				};
			}

			// bind hook for onOpen
			if(ctrl.options && ctrl.onClose) {
				ctrl.options.onClose = function(selectedDates, dateStr, instance) {
					$timeout(function() {
						ctrl.onClose({selectedDates: selectedDates, dateStr: dateStr, instance: instance});
					});
				};
			}

			// bind hook for onMonthChange
			if(ctrl.options && ctrl.onMonthChange) {
				ctrl.options.onMonthChange = function(selectedDates, dateStr, instance) {
					$timeout(function() {
						ctrl.onMonthChange({selectedDates: selectedDates, dateStr: dateStr, instance: instance});
					});
				};
			}

			// bind hook for onYearChange
			if(ctrl.options && ctrl.onYearChange) {
				ctrl.options.onYearChange = function(selectedDates, dateStr, instance) {
					$timeout(function() {
						ctrl.onYearChange({selectedDates: selectedDates, dateStr: dateStr, instance: instance});
					});
				};
			}

			// bind hook for onReady
			if(ctrl.options && ctrl.onReady) {
				ctrl.options.onReady = function(selectedDates, dateStr, instance) {
					$timeout(function() {
						ctrl.onReady({selectedDates: selectedDates, dateStr: dateStr, instance: instance});
					});
				};
			}

			// bind hook for onValueUpdate
			if(ctrl.onValueUpdate) {
				ctrl.options.onValueUpdate = function(selectedDates, dateStr, instance) {
					$timeout(function() {
						ctrl.onValueUpdate({selectedDates: selectedDates, dateStr: dateStr, instance: instance});
					});
				};
			}

			// bind hook for onDayCreate
			if(ctrl.onDayCreate) {
				ctrl.options.onDayCreate = function(selectedDates, dateStr, instance) {
					$timeout(function() {
						ctrl.onDayCreate({selectedDates: selectedDates, dateStr: dateStr, instance: instance});
					});
				};
			}

		}
    }
    
    angular.module('umbraco.directives').component('umbFlatpickr', umbFlatpickr);
    
})();