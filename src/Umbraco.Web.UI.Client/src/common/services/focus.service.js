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

    function focusService() {
        
        
        function focusInApp(e) {
            service.currentFocus = e.target;
        }
        document.addEventListener('focusin', focusInApp);
        
        
        var service = {
            currentFocus: null,
            
            /**
    		 * @ngdoc property
             * @name umbraco.services.focusService#lastKnownFocus
		     * @propertyOf umbraco.services.focusService
             *
             * @description
             * A element set to be remembered, the directive using this should store the value of this to make sure that its not changed white using that directive.
             * This variable is avaiable for directives that are not able to figure out the focused element on init, and there this service will help remembering it untill the directive is initialized.
             * 
             */
            lastKnownFocus: null,
            
            /**
             * @ngdoc function
             * @name umbraco.services.focusService#rememberFocus
             * @methodOf umbraco.services.focusService
             * @function
             *
             * @description
             * Call this before a new focus is begin set, to be able to return to the focus before a given scenario.
             * 
             */
            rememberFocus: function() {
                service.lastKnownFocus = service.currentFocus;
            },
            
            /**
             * @ngdoc function
             * @name umbraco.services.focusService#setLastKnownFocus
             * @methodOf umbraco.services.focusService
             * @function
             *
             * @description
             * Overwrite the element remembered as the last known element in focus.
             * 
             */
            setLastKnownFocus: function(element) {
                service.lastKnownFocus = element;
            }
        };

        return service;

    }
    
    angular.module("umbraco.services").factory("focusService", focusService);

})();
