/**
 @ngdoc service
 * @name umbraco.services.focusLockService
 *
 * @description
 * <b>Added in Umbraco 8.2</b>. Application-wide service for locking focus inside an overlday/modal when open.
 *
 */

(function () {
    "use strict";

    var domNodes = {};

    function getDOMNodes (){
        domNodes.appHeader = document.querySelector('.umb-app-header');
        domNodes.leftColumn = document.querySelector('#leftcolumn');
        domNodes.mainWrapper = document.querySelector('#mainwrapper');
        domNodes.editorsPrevSibling = document.querySelector('.umb-editors').previousElementSibling;
        domNodes.editor = document.querySelector('.umb-editor');
    }

    function resetFocus(elm){
        setTimeout(function(){
            elm.focus();
        }, 100);
    }

    function focusLockService() {

        var service = {
            /**
             * @ngdoc function
             * @name umbraco.services.focusLockService#addFocusLock
             * @methodOf umbraco.services.focusLockService
             * @function
             *
             * @description
             * Call this before a new overlay/modal is activated, to ensure focus is locked inside the opened overlay/modal
             *
             * @param {Object} element angularJS object containing the element
             * @param {String} mode can be either "overlay" or "infinite-overlay"
             *
             */

            addInfiniteFocusLock: function(elm, overlays) {
                var children = elm.children();

                // Get the DOM nodes we need to add/remove the inert attribute for
                getDOMNodes();

                // Disable "outer" elements once
                if(overlays === 0){
                    domNodes.leftColumn.setAttribute('inert','');
                    domNodes.appHeader.setAttribute('inert','');
                    domNodes.editorsPrevSibling.setAttribute('inert','');
                }

                // Disable infinite editors if they're not the current editor
                if(children.length){
                    children.attr('inert','');
                }
            },

            removeInfiniteFocusLock: function(elm, overlays) {
                var children = elm.children();
                var secondLastChildIndex = children.length - 2;
                var secondLastChild = children[secondLastChildIndex];
                var focusableElementsString = 'a[href], area[href], input:not([disabled]):not(.ng-hide), select:not([disabled]), textarea:not([disabled]), button:not([disabled]):not([tabindex="-1"]), iframe, object, embed, [tabindex="0"], [contenteditable]';

                // Enable "outer" elements once
                if(overlays === 1){
                    domNodes.leftColumn.removeAttribute('inert','');
                    domNodes.appHeader.removeAttribute('inert','');
                    domNodes.editor.removeAttribute('inert','');
                }

                if(secondLastChild) {
                    var firstFocusableElement = secondLastChild.querySelector(focusableElementsString);
                    secondLastChild.removeAttribute('inert','');

                    // Set focus on the first possible element in the editor that is unlocked
                    resetFocus(firstFocusableElement);
                }
            }
        };

        return service;

    }

    angular.module("umbraco.services").factory("focusLockService", focusLockService);

})();
