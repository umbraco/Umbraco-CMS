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
            language="'csharp'">
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

@param {string=} language Language of the code snippet, e.g csharp, html, css.
**/


(function () {
    'use strict';

    var umbCodeSnippet = {
        templateUrl: 'views/components/umb-code-snippet.html',
        controller: UmbCodeSnippetController,
        controllerAs: 'vm',
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

            if (vm.language)
            {
                switch (vm.language.toLowerCase()) {
                    case "csharp":
                    case "c#":
                        vm.language = "C#";
                        break;
                    case "html":
                        vm.language = "HTML";
                        break;
                    case "css":
                        vm.language = "CSS";
                        break;
                    case "javascript":
                        vm.language = "JavaScript";
                        break;
                }
            }
            
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
