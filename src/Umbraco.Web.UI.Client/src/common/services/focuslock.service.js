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

    function focusLockService() {
        var elementToInert = document.querySelector('#mainwrapper');

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
                elementToInert.setAttribute('inert', true);
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
            }
        };

        return service;

    }
    
    angular.module("umbraco.services").factory("focusLockService", focusLockService);

})();
