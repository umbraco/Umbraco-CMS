(function () {
    "use strict";

    /**
    * @ngdoc service
    * @name umbraco.services.udiService
    * @description A service for UDIs
    **/
    function udiService() {
        return {

            /**
             * @ngdoc method
             * @name umbraco.services.udiService#create
             * @methodOf umbraco.services.udiService
             * @function
             *
             * @description
             * Generates a Udi string.
             *
             * @param {string} entityType The entityType as a string.
             * @returns {string} The generated UDI
             */
            create: function(entityType) {
                return "umb://" + entityType + "/" + (String.CreateGuid().replace(/-/g, ""));
            }
        }
    }

    angular.module("umbraco.services").factory("udiService", udiService);

})();
