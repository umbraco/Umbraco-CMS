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
             * Generates a Udi string with a new ID
             *
             * @param {string} entityType The entityType as a string.
             * @returns {string} The generated UDI
             */
            create: function(entityType) {
                return this.create(entityType, String.CreateGuid());
            },

            build: function (entityType, guid) {
                return "umb://" + entityType + "/" + (guid.replace(/-/g, ""));
            }
        }
    }

    angular.module("umbraco.services").factory("udiService", udiService);

})();
