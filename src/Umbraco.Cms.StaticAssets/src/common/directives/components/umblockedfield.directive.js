/**
@ngdoc directive
@name umbraco.directives.directive:umbLockedField
@restrict E
@scope

@description
Use this directive to render a value with a lock next to it. When the lock is clicked the value gets unlocked and can be edited.

<h3>Markup example</h3>
<pre>
	<div ng-controller="My.Controller as vm">

		<umb-locked-field
			ng-model="vm.value"
			placeholder-text="'Click to unlock...'">
		</umb-locked-field>

	</div>
</pre>

<h3>Controller example</h3>
<pre>
	(function () {
		"use strict";

		function Controller() {

			var vm = this;
			vm.value = "My locked text";

        }

		angular.module("umbraco").controller("My.Controller", Controller);

	})();
</pre>

@param {string} ngModel (<code>binding</code>): The locked text.
@param {boolean=} locked (<code>binding</code>): <Code>true</code> by default. Set to <code>false</code> to unlock the text.
@param {string=} placeholderText (<code>binding</code>): If ngModel is empty this text will be shown.
@param {string=} regexValidation (<code>binding</code>): Set a regex expression for validation of the field.
@param {string} validationPosition (<code>binding</code>): The position of the validation. Set to <code>'left'</code> or <code>'right'</code>.
@param {string=} serverValidationField (<code>attribute</code>): Set a server validation field.
**/

(function() {
	'use strict';

	function LockedFieldDirective($timeout, localizationService) {

	    function link(scope, el, attr, ngModelCtrl) {

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

                if (scope.validationPosition === undefined || scope.validationPosition === null) {
                    scope.validationPosition = "left";
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
                validationPosition: "=?",
				serverValidationField: "@"
			},
			link: link
		};

		return directive;

	}

	angular.module('umbraco.directives').directive('umbLockedField', LockedFieldDirective);

})();
