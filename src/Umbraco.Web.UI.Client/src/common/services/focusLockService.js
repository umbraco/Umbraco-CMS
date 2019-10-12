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

             // Add a mode param so the logic can be split into different functions!

            addFocusLock: function(overlays) {
                getDOMNodes();

                console.log('add focus lock');

                // // If the string "infinite-overlay" is passed we activate the methods needed for 
                // if(mode === 'infinite-overlay'){
                //     var children = element.children();

                //     // The DOM does not update synchronously so the latest triggered overlay will never be added to the array/collection
                //     if(children.length){
                //         children.attr('inert','true');
                //     }

                //     //TODO: Set some kind of variable to ensure this call only happens once
                //     getElementsToToggle(elementsToToggleForInfiniteOverlayMode, true);
                // }

                // // if we get a value of "overlay" or if it's empty
                // if(mode === 'overlay' || !mode){
                //     getElementsToToggle(elementsToTogleForOverlayMode, true);
                // }

                // IF we deal with an ordinary overlay
                // addOuterFocusLock

                // IF we deal with and infinite editor
                // addOuterFocusLock
                // addInnerFocusLock
                if(overlays === 0){
                    domNodes.leftColumn.setAttribute('inert','');
                    domNodes.appHeader.setAttribute('inert','');
                    domNodes.editorsPrevSibling.setAttribute('inert','');
                }
            },

            removeFocusLock: function(overlays) {
                console.log('remove focus lock');

                // IF we deal with an ordinary overlay
                // removeOuterFocusLock

                // IF we deal with an infinite editor
                // addOuterFocusLock
                // addInnerFocusLock
                if(overlays === 1){
                    domNodes.leftColumn.removeAttribute('inert','');
                    domNodes.appHeader.removeAttribute('inert','');
                    domNodes.editor.removeAttribute('inert','');
                }
            }
        };

        return service;

    }

    angular.module("umbraco.services").factory("focusLockService", focusLockService);

})();
