/**
 @ngdoc service
 * @name umbraco.services.focusLockService
 *
 * @description
 * <b>Added in Umbraco 8.7</b>. Application-wide service for locking focus within overlays or sections that are triggered by the backdrop service.
 * 
 */

(function () {
    "use strict";

    function focusLockService($timeout, eventsService) {
        var elementToInert = document.querySelector('#mainwrapper');
        var setInertAttribute = false;

        eventsService.on("appState.overlay", (name, args) => {
            if(args){
                setInertAttribute = true;
                return;
            }
        });

        // TODO; at a later stage hook into these events as well and determine whether the inert should be set or not
        // At some point it will also be neccessary to check if the attribute has already been set since the backdrop can
        // potentially already be open in another context and then that context can trigger an infinte editor for instance, which
        // will trigger the backdrop again

        // eventsService.on("appState.editors.open", (name, args) => {
        //     console.log('editor open:', args);
        // });

        // eventsService.on("appState.editors.close", (name, args) => {
        //     console.log('editor close:', args);
        // });

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
                // If the backdrop was triggered by an overlay we add the inert attribute
                $timeout(function(){
                    if(setInertAttribute) {
                        elementToInert.setAttribute('inert', true);
                    }
                })
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
                elementToInert.removeAttribute('inert');
                setInertAttribute = false;
            }
        };

        return service;

    }
    
    angular.module("umbraco.services").factory("focusLockService", focusLockService);

})();
