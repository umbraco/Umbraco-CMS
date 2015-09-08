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

		function link(scope, el, attr, ngModel) {

			var input = el.children('.umb-locked-field__input');

			function activate() {

				// if locked state is not defined as an attr set default state
				if (scope.locked === undefined || scope.locked === null) {
					scope.locked = true;
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
				model: '=ngModel',
				locked: "=?",
				placeholderText: "=?"
			},
			link: link
		};

		return directive;

	}

	angular.module('umbraco.directives').directive('umbLockedField', LockedFieldDirective);

})();
