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
                return this.build(entityType, String.CreateGuid());
            },

            build: function (entityType, guid) {
                return "umb://" + entityType + "/" + (guid.replace(/-/g, ""));
            },

            getKey: function (udi) {
                if (!Utilities.isString(udi)) {
                    throw "udi is not a string";
                }
                if (!udi.startsWith("umb://")) {
                    throw "udi does not start with umb://";
                }
                var withoutScheme = udi.substr("umb://".length);
                var withoutHost = withoutScheme.substr(withoutScheme.indexOf("/") + 1).trim();
                if (withoutHost.length !== 32) {
                    throw "udi is not 32 chars";
                }
                return `${withoutHost.substr(0, 8)}-${withoutHost.substr(8, 4)}-${withoutHost.substr(12, 4)}-${withoutHost.substr(16, 4)}-${withoutHost.substr(20)}`;
            }
        }
    }

    angular.module("umbraco.services").factory("udiService", udiService);

})();
