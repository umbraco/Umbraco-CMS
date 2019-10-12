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

    function focusLockService() {

        var service = {
            elementsToLock: {
                appHeader = document.querySelector('.umb-app-header'),
                mainWrapper = document.querySelector('#mainWrapper'),
                umbEditors = document.querySelector('.umb-editors'),
                umbModalColumn = document.querySelector('.umb-modalcolumn')
            },

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

            addFocusLock: function(element) {
                var elm = element[0];
                var children = element.children();

                console.log(elementsToLock,'elements to lock');
                // The DOM does not update synchronously so the latest triggered overlay will never be added to the array/collection
                if(children.length){
                    console.log(children, ' children');
                    children.attr('inert','true');
                }

                // TODO: Maybe Push each child into a new array except the last one?
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
