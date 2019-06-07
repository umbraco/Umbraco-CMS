(function () {
    "use strict";

    /**
    * @ngdoc service
    * @name umbraco.services.udiParser
    * @description A object used to parse UDIs
    **/
    function udiParser() {
        return {

            /**
             * @ngdoc method
             * @name umbraco.services.udiParser#parse
             * @methodOf umbraco.services.udiParser
             * @function
             *
             * @description
             * Converts the string representation of an entity identifier into the equivalent Udi instance.
             *
             * @param {string} input The string to parse
             * @returns {Object} The parsed UDI or null if input isn't a valid UDI
             */
            parse: function(input) {
                if (!input || typeof input !== "string" || !input.startsWith("umb://"))
                    return null;

                var lastIndexOfSlash = input.substring("umb://".length).lastIndexOf("/");

                var entityType = lastIndexOfSlash === -1 ? input.substring("umb://".length) : input.substr("umb://".length, lastIndexOfSlash);
                var value = lastIndexOfSlash === -1 ? null : input.substring("umb://".length + lastIndexOfSlash + 1);

                return {
                    entityType,
                    value,

                    toString: function() {
                        return "umb://" + entityType + (value === null ? "" :  "/" + value);
                    }
                }
            }
        }
    }

    angular.module("umbraco.services").factory("udiParser", udiParser);

})();
