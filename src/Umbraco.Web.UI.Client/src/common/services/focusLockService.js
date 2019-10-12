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

    /**
     * Helper method to fetch the DOM nodes that needs to be disabled/enabled
     */
    function getDOMNodes (){
        domNodes.appHeader = document.querySelector('.umb-app-header');
        domNodes.leftColumn = document.querySelector('#leftcolumn');
        domNodes.mainWrapper = document.querySelector('#mainwrapper');
        domNodes.editorsPrevSibling = document.querySelector('.umb-editors').previousElementSibling;
        domNodes.editor = document.querySelector('.umb-editor');
    }

    /**
     * Helper method to reset focus on the first possible element in an overlay
     * @param {HTMLElement} elm 
     */
    function resetFocus(elm){
        setTimeout(function(){
            elm.focus();
        }, 100);
    }

    function focusLockService() {

        var service = {

            /**
             * @ngdoc function
             * @name umbraco.services.focusLockService#addInfiniteFocusLock
             * @methodOf umbraco.services.focusLockService
             * @function
             *
             * @description
             * Call this method before a new infinite overlay is activated to ensure focus is locked inside it
             *
             * @param {Array} element array containing an element
             * @param {Number} overlays a number of overlays that are open
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

            /**
             * @ngdoc function
             * @name umbraco.services.focusLockService#removeInfiniteFocusLock
             * @methodOf umbraco.services.focusLockService
             * @function
             *
             * @description
             * Call this method when an infinite editor is closed to enable the locked elements in the DOM again
             *
             * @param {Array} element array containing an element
             * @param {Number} overlays a number of overlays that are open
             *
             */
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
            },

            /**
             * @ngdoc function
             * @name umbraco.services.focusLockService#addFocusLock
             * @methodOf umbraco.services.focusLockService
             * @function
             *
             * @description
             * Call this method before a new overlay is activated to ensure focus is locked inside it
             *
             */
            addFocusLock: function() {
                // Get the DOM nodes we need to add/remove the inert attribute for
                getDOMNodes();

                domNodes.mainWrapper.setAttribute('inert','');
            },

            /**
             * @ngdoc function
             * @name umbraco.services.focusLockService#removeFocusLock
             * @methodOf umbraco.services.focusLockService
             * @function
             *
             * @description
             * Call this method when an overlay is closed to enable the locked element in the DOM again
             *
             */
            removeFocusLock: function(){
                domNodes.mainWrapper.removeAttribute('inert');
            }
        };

        return service;

    }

    angular.module("umbraco.services").factory("focusLockService", focusLockService);

})();
