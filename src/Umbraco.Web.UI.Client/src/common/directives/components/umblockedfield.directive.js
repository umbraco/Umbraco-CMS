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
                    scope.lockedFieldForm.lockedField.$setViewValue(newValue);
                    scope.lockedFieldForm.lockedField.$render();
                }
	        });

			var input = el.find('.umb-locked-field__input');

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
				input.unbind("blur");
			};

			scope.unlock = function() {
				scope.locked = false;
				autoFocusField();
			};

			function autoFocusField() {

				var onBlurHandler = function() {
					scope.$apply(function(){
						scope.lock();
					});
				};

				$timeout(function() {
					input.focus();
					input.select();
					input.on("blur", onBlurHandler);
				});

			}

			activate();

			scope.$on('$destroy', function() {
				input.unbind('blur');
			});

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
