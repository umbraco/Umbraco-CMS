/**
 @ngdoc service
 * @name umbraco.services.focusService
 *
 * @description
 * <b>Added in Umbraco 8.1</b>. Application-wide service for focus related stuff.
 * 
 */

(function () {
    "use strict";

    function focusLockService(eventsService) {
    
        eventsService.on("appState.editors.open", (name, args) => {
            console.log('editor open', args);
        });

        eventsService.on("appState.editors.close", (name, args) => {
            console.log('editor close:', args);
        });

        eventsService.on("appState.overlay", (name, args) => {
            console.log('overlay', args);
        });

        var service = {
            
            /**
            * @ngdoc function
            * @name umbraco.services.focusLockService#addInertAttribute
            * @methodOf umbraco.services.focusLockService
            * @function
             *
             * @description
             * Adds the "inert" attribute, which will set <code>aria-hidden="true"</code> and <code>tabindex="-1"</code> attributes on all
             * interactive elements within the passed container to support the "focus-lock" directive so focus can be locked within the overlay that is open
             * 
             */
            addInertAttribute: function() {
                console.log('add inert attribute');
            },

            /**
            * @ngdoc function
            * @name umbraco.services.focusLockService#removeInertAttribute
            * @methodOf umbraco.services.focusLockService
            * @function
             *
             * @description
             * Removes the "inert" attribute, which will also remove the  <code>aria-hidden="true"</code> and <code>tabindex="-1"</code> attributes and remove the fockus lock
             * 
             */
            removeInertAttribute: function() {
                console.log('remove inert attribute');
            }
        };

        return service;

    }
    
    angular.module("umbraco.services").factory("focusLockService", focusLockService);

})();
