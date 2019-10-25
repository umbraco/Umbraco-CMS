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
        function exposeAPI(scope) {
            if (!scope) {
                throw "scope cannot be null";
            }

            scope.$emit("ExposePropertyEditorAPI", scope);
        }

        return {
            exposeAPI: exposeAPI
        };
    }

    angular.module('umbraco.services').factory('propertyEditorService', propertyEditorService);
})();
