/**
@ngdoc directive
@name umbraco.directives.directive:umbClipboard
@restrict E
@scope

@description
<strong>Added in Umbraco v. 7.7:</strong> Use this directive to copy content to the clipboard

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.ClipBoardController as vm">
        
        <!-- Copy text from an element -->
        <div id="copy-text">Copy me!</div>
        
        <umb-button
            umb-clipboard
            umb-clipboard-success="vm.copySuccess()"
            umb-clipboard-error="vm.copyError()"
            umb-clipboard-target="#copy-text"
            state="vm.clipboardButtonState"
            type="button"
            label="Copy">
        </umb-button>

        <!-- Cut text from a textarea -->
        <textarea id="cut-text" ng-model="vm.cutText"></textarea>

        <umb-button
            umb-clipboard
            umb-clipboard-success="vm.copySuccess()"
            umb-clipboard-error="vm.copyError()"
            umb-clipboard-target="#cut-text"
            umb-clipboard-action="cut"
            state="vm.clipboardButtonState"
            type="button"
            label="Copy">
        </umb-button>

        <!-- Copy text without an element -->
        <umb-button
            ng-if="vm.copyText"
            umb-clipboard
            umb-clipboard-success="vm.copySuccess()"
            umb-clipboard-error="vm.copyError()"
            umb-clipboard-text="vm.copyText"
            state="vm.clipboardButtonState"
            type="button"
            label="Copy">
        </umb-button>
    
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;

            vm.copyText = "Copy text without element";
            vm.cutText = "Text to cut";

            vm.copySuccess = copySuccess;
            vm.copyError = copyError;

            function copySuccess() {
                vm.clipboardButtonState = "success";
            }
            
            function copyError() {
                vm.clipboardButtonState = "error";
            }

        }

        angular.module("umbraco").controller("My.ClipBoardController", Controller);

    })();
</pre>

@param {callback} umbClipboardSuccess (<code>expression</code>): Callback function when the content is copied.
@param {callback} umbClipboardError (<code>expression</code>): Callback function if the copy fails.
@param {string} umbClipboardTarget (<code>attribute</code>): The target element to copy.
@param {string} umbClipboardAction (<code>attribute</code>): Specify if you want to copy or cut content ("copy", "cut"). Cut only works on <code>input</code> and <code>textarea</code> elements.
@param {string} umbClipboardText (<code>attribute</code>): Use this attribute if you don't have an element to copy from.

**/

(function () {
    'use strict';

    function umbClipboardDirective($timeout, assetsService, $parse) {

        function link(scope, element, attrs, ctrl) {

            var clipboard;
            var target = element[0];

            assetsService.loadJs("lib/clipboard/clipboard.min.js", scope)
                .then(function () {


                    if (attrs.umbClipboardTarget) {
                        target.setAttribute("data-clipboard-target", attrs.umbClipboardTarget);
                    }

                    if (attrs.umbClipboardAction) {
                        target.setAttribute("data-clipboard-action", attrs.umbClipboardAction);
                    }

                    if (attrs.umbClipboardText) {
                        target.setAttribute("data-clipboard-text", attrs.umbClipboardText);
                    }

                    clipboard = new ClipboardJS(target);

                    var expressionHandlerSuccess = $parse(attrs.umbClipboardSuccess);
                    clipboard.on('success', function (e) {
                        e.clearSelection();
                        if (attrs.umbClipboardSuccess) {

                            expressionHandlerSuccess(scope, { msg: "success" });
                        }

                    });

                    var expressionHandlerError = $parse(attrs.umbClipboardError);
                    clipboard.on('error', function (e) {
                        if (attrs.umbClipboardError) {

                            expressionHandlerError(scope, { msg: "error" });
                        }
                    });

                });

            // clean up
            scope.$on('$destroy', function () {
                clipboard.destroy();
            });

        }

        ////////////

        var directive = {
            restrict: 'A',
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbClipboard', umbClipboardDirective);

})();
