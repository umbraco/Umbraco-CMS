/**
* @ngdoc directive
* @name umbraco.directives.directive:umbContentName 
* @restrict E
* @function
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/

angular.module("umbraco.directives")
	.directive('umbLockedField', function ($timeout, localizationService) {
	    return {
			require: "ngModel",
			restrict: 'E',
			replace: true,
			templateUrl: 'views/components/umb-locked-field.html',
			scope: {
				model: '=ngModel',
				locked: "=?",
				placeholderText: "=?"
			},

			link: function(scope, element, attrs, ngModel) {

				// if locked state is not defined as an attr set default state
				if(scope.locked === undefined || scope.locked === null) {
					scope.locked = true;
				}

				// if locked state is not defined as an attr set default state
				if(scope.placeholderText === undefined || scope.placeholderText === null) {
					scope.placeholderText = "Enter value...";
				}

				scope.toggleLock = function(){

					scope.locked = !scope.locked;

					if(scope.locked === false) {
						autoFocusField();
					}

				};

				function autoFocusField() {

					// timeout to make sure dom has updated from a disabled field
					$timeout(function() {
						var input = element.children('.umb-locked-field__input');
						input.focus();
					});

				}

			}

		};
		});
