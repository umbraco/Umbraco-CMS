/**
@ngdoc directive
@name umbraco.directives.directive:umbConfirm
@restrict E
@scope

@description
A confirmation dialog


<h3>Markup example</h3>
<pre>
	<div ng-controller="My.Controller as vm">

       <umb-confirm caption="Title" on-confirm="vm.onConfirm()" on-cancel="vm.onCancel()"></umb-confirm>

	</div>
</pre>

<h3>Controller example</h3>
<pre>
	(function () {
		"use strict";

		function Controller() {

            var vm = this;

            vm.onConfirm = function() {
                alert('Confirm clicked');
            };

            vm.onCancel = function() {
                alert('Cancel clicked');
            }


        }

		angular.module("umbraco").controller("My.Controller", Controller);

	})();
</pre>

@param {string} caption (<code>attribute</code>): The caption shown above the buttons
@param {callback} on-confirm (<code>attribute</code>): The call back when the "OK" button is clicked. If not set the button will not be shown
@param {callback} on-cancel (<code>atribute</code>): The call back when the "Cancel" button is clicked. If not set the button will not be shown
**/
function confirmDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/components/umb-confirm.html',
        scope: {
            onConfirm: '=',
            onCancel: '=',
            caption: '@',
            confirmButtonStyle: '@',
            confirmDisabled: '<?',
            confirmLabelKey: '@'
        },
        link: function (scope, element, attr, ctrl) {
            scope.showCancel = false;
            scope.showConfirm = false;
            scope.confirmButtonState = "init";

            if (scope.onConfirm) {
                scope.showConfirm = true;
            }

            if (scope.onCancel) {
                scope.showCancel = true;
            }

            scope.confirm = function () {
                if (!scope.onConfirm) {
                    return;
                }

                scope.confirmButtonState = "busy";
                scope.onConfirm();
            }
        }
    };
}
angular.module('umbraco.directives').directive("umbConfirm", confirmDirective);
