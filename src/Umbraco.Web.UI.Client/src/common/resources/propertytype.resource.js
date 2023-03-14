/**
    * @ngdoc service
    * @name umbraco.resources.propertyTypeResource
    * @description Loads in data for property types
    **/
 function propertyTypeResource($http, umbRequestHelper) {

    return {
        /**
         * @ngdoc method
         * @name umbraco.resources.propertyTypeResource#hasValues
         * @methodOf umbraco.resources.propertyTypeResource
         *
         * @description
         * Checks for values stored for the property type
         *
         * ##usage
         * <pre>
         * propertyTypeResource.hasValues(alias)
         *    .then(function(data) {
         *        console.log(data.hasValues);
         *    }, function(err) {
         *      console.log("failed to check if property type has values", err);
         *    });
         * </pre> 
         * 
         * @param {Int} alias alias of data type
         * @returns {Promise} resourcePromise object.
         */
        hasValues: function (alias) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "propertyTypeApiBaseUrl",
                        "hasvalues",
                        { alias }
                    )),
                "Failed to check if the data type with " + alias + " has values");
        }
    };
}

angular.module("umbraco.resources").factory("propertyTypeResource", propertyTypeResource);
