/**
@ngdoc directive
@name umbraco.directives.directive:umbCodeSnippet
@restrict E
@scope

@description

<h3>Markup example</h3>
<pre>
	<div ng-controller="My.Controller as vm">

        <umb-code-snippet 
            language="charp">
            {{code}}
        </umb-code-snippet>

	</div>
</pre>

<h3>Controller example</h3>
<pre>
	(function () {
		"use strict";

		function Controller() {

            var vm = this;

        }

		angular.module("umbraco").controller("My.Controller", Controller);

	})();
</pre>

@param {object} language (<code>binding</code>): Language of the code snippet.
**/


(function () {
    'use strict';

    var umbCodeSnippet = {
        templateUrl: 'views/components/umb-code-snippet.html',
        controller: UmbCodeSnippetController,
        controllerAs: 'vm',
        //replace: true,
        transclude: true,
        bindings: {
            language: '<'
        }
    };

    function UmbCodeSnippetController($timeout) {

        const vm = this;

        vm.page = {};

        vm.$onInit = onInit;
        vm.copySuccess = copySuccess;
        vm.copyError = copyError;

        function onInit() {
            vm.guid = String.CreateGuid();
        }

        // copy to clip board success
        function copySuccess() {
            if (vm.page.copyCodeButtonState !== "success") {
                $timeout(function () {
                    vm.page.copyCodeButtonState = "success";
                });
                $timeout(function () {
                    resetClipboardButtonState();
                }, 1000);
            }
        }

        // copy to clip board error
        function copyError() {
            if (vm.page.copyCodeButtonState !== "error") {
                $timeout(function () {
                    vm.page.copyCodeButtonState = "error";
                });
                $timeout(function () {
                    resetClipboardButtonState();
                }, 1000);
            }
        }

        function resetClipboardButtonState() {
            vm.page.copyCodeButtonState = "init";
        }
    }

    angular.module('umbraco.directives').component('umbCodeSnippet', umbCodeSnippet);

})();
