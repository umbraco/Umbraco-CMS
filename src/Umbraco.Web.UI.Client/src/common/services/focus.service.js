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
        
        
        var currentFocus = null;
        var lastKnownFocus = null;
        
        
        function focusInApp(e) {
            currentFocus = e.target;
        }
        document.addEventListener('focusin', focusInApp);
        
        
        var service = {
            
            /**
            * @ngdoc function
            * @name umbraco.services.focusService#getLastKnownFocus
            * @methodOf umbraco.services.focusService
            * @function
             *
             * @description
             * Gives the element that was set to be remembered, the directive using this should store the value of this to make sure that its not changed white using that directive.
             * This variable is avaiable for directives that are not able to figure out the focused element on init, and there this service will help remembering it untill the directive is initialized.
             * 
             */
            getLastKnownFocus: function() {
                return lastKnownFocus;
            },
            
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
                lastKnownFocus = currentFocus;
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
                lastKnownFocus = element;
            }
        };

        return service;

    }
    
    angular.module("umbraco.services").factory("focusService", focusService);

})();
