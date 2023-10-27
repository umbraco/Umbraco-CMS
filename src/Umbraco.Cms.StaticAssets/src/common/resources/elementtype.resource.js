/**
    * @ngdoc service
    * @name umbraco.resources.elementTypeResource
    * @description Loads in data for element types
    **/
function elementTypeResource($q, $http, umbRequestHelper) {

    return {

        /**
        * @ngdoc method
        * @name umbraco.resources.elementTypeResource#getAll
        * @methodOf umbraco.resources.elementTypeResource
        *
        * @description
        * Gets a list of all element types
        *
        * ##usage
        * <pre>
        * elementTypeResource.getAll()
        *    .then(function() {
        *        alert('Found it!');
        *    });
        * </pre>
        *
        * @returns {Promise} resourcePromise object.
        *
        **/
        getAll: function () {
            
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "elementTypeApiBaseUrl",
                        "GetAll")),
                "Failed to retrieve element types");
            
        }

    };
}

angular.module("umbraco.resources").factory("elementTypeResource", elementTypeResource);
