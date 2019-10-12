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

            addFocusLock: function() {
                getDOMNodes();
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
                console.log('add focus lock');
                domNodes.leftColumn.setAttribute('inert','');
                domNodes.appHeader.setAttribute('inert','');
                domNodes.editorsPrevSibling.setAttribute('inert','');
            },

            removeFocusLock: function() {
                console.log('remove focus lock');
                domNodes.leftColumn.removeAttribute('inert','');
                domNodes.appHeader.removeAttribute('inert','');
                domNodes.editorsPrevSibling.removeAttribute('inert','');
            }
        };

        return service;

    }

    angular.module("umbraco.services").factory("focusLockService", focusLockService);

})();

// TODO: Ved infinite fjernes alle inert på "ydre" elementer, før når den sidste er lukket...
// TODO: Så der skal laves et check for at se om removeFocus lock skal kaldes
// TODO: Make DOM method more generic...


// getDOM method that returns all of the DOM nodes we might be interested in and which can be used to cache the DOM, so 
