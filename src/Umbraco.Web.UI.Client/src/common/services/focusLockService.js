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
    var elementsToToggle = {
        'appHeader': document.querySelector('.umb-app-header'),
        'mainWrapper': document.querySelector('#mainWrapper'),
        'umbEditors': document.querySelector('.umb-editors'),
        'umbModalColumn': document.querySelector('.umb-modalcolumn')
    };

    function focusLockService() {

        // TODO: Maybe add a helper method here, which is called in both addFocusLock and removeFocusLock, which also calls another method to get the 
        // needed elements we want to toggle

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
             */

             // Add a mode param so the logic can be split into different functions!

            addFocusLock: function(element) {
                // TODO:
                // * Add mode param in order to call the proper method!
                // * Add inert attribute for appHeader, umbEditorsPrevSibiling and umbModalColum when we're dealing with infinite editing and children of course
                // * Add inert attribute for "mainWrapper" when it's "normal" overlay mode
                var appHeader = document.querySelector('.umb-app-header');
                var mainWrapper = document.querySelector('#mainwrapper'); //Only if it's an "ordinary" overlay
                var umbEditors = document.querySelector('.umb-editors');
                var umbEditorsPrevSibiling = umbEditors.previousElementSibling;
                var umbModalColumn = document.querySelector('.umb-modalcolumn');

                var children = element.children();

                appHeader.setAttribute('inert','');
                umbModalColumn.setAttribute('inert','');
                umbEditorsPrevSibiling.setAttribute('inert','');

                // console.log(elementsToLock,'elements to lock');
                // The DOM does not update synchronously so the latest triggered overlay will never be added to the array/collection
                if(children.length){
                    children.attr('inert','true');
                }

            },

            removeFocusLock: function(element) {
                console.log('remove focus lock');
                console.log('element: ', element);
            }
        };

        return service;

    }

    angular.module("umbraco.services").factory("focusLockService", focusLockService);

})();
