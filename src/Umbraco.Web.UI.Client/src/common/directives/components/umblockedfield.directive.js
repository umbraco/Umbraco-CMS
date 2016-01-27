/**
* @ngdoc directive
* @name umbraco.directives.directive:umbContentName 
* @restrict E
* @function
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/
(function() {
	'use strict';

	function LockedFieldDirective($timeout, localizationService) {

	    function link(scope, el, attr, ngModelCtrl) {
            
	        //watch the ngModel so we can manually update the textbox view value when it changes
	        // this ensures that the normal flow (i.e. a user editing the text box) occurs so that
            // the parsers, validators and viewchangelisteners execute
	        scope.$watch("ngModel", function (newValue, oldValue) {
	            if (newValue !== oldValue) {
	                //Hack: in order for the pipeline to execute for setViewValue, the underlying $modelValue cannot
	                // match the value being set with the newValue, so we'll se it to undefined first.
	                // We could avoid this hack by setting the ngModel of the lockedField input field to a custom
	                // scope object, but that would mean we'd have to watch that value too in order to set the outer
                    // ngModelCtrl.$modelValue. It's seems like less overhead to just do this and not have 2x watches.
	                scope.lockedFieldForm.lockedField.$modelValue = undefined;
					scope.lockedFieldForm.lockedField.$render();
                }
				scope.lockedFieldForm.lockedField.$setViewValue(scope.lockedFieldForm.lockedField.$modelValue);
	        });

			function activate() {

				// if locked state is not defined as an attr set default state
				if (scope.locked === undefined || scope.locked === null) {
					scope.locked = true;
				}

			    // if regex validation is not defined as an attr set default state
                // if this is set to an empty string then regex validation can be ignored.
				if (scope.regexValidation === undefined || scope.regexValidation === null) {
				    scope.regexValidation = "^[a-zA-Z]\\w.*$";
				}

				if (scope.serverValidationField === undefined || scope.serverValidationField === null) {
				    scope.serverValidationField = "";
				}

				// if locked state is not defined as an attr set default state
				if (scope.placeholderText === undefined || scope.placeholderText === null) {
					scope.placeholderText = "Enter value...";
				}

			}

			scope.lock = function() {
				scope.locked = true;
			};

			scope.unlock = function() {
				scope.locked = false;
			};

			activate();

		}

		var directive = {
			require: "ngModel",
			restrict: 'E',
			replace: true,
			templateUrl: 'views/components/umb-locked-field.html',
			scope: {
			    ngModel: "=",
				locked: "=?",
				placeholderText: "=?",
				regexValidation: "=?",
				serverValidationField: "@"
			},
			link: link
		};

		return directive;

	}

	angular.module('umbraco.directives').directive('umbLockedField', LockedFieldDirective);

})();
