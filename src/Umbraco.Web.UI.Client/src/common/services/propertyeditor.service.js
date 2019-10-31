/**
 @ngdoc service
 * @name umbraco.services.propertyEditorService
 */

(function() {
    'use strict';

    function propertyEditorService() {
        /**
         * @ngdoc function
         * @name umbraco.services.propertyEditorService#expose
         * @methodOf umbraco.services.propertyEditorService
         * @function
         *
         * @param {object} scope An object containing API for the PropertyEditor
         */
        function exposeAPI(scope, api) {
            if (!scope) {
                throw "scope cannot be null";
            }
            if (!api) {
                throw "api cannot be null";
            }
            scope.$emit("ExposePropertyEditorAPI", api);
        }

        return {
            exposeAPI: exposeAPI
        };
    }

    angular.module('umbraco.services').factory('propertyEditorService', propertyEditorService);
})();
