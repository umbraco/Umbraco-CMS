/**
    * @ngdoc service
    * @name umbraco.resources.elementTypeResource
    * @description Loads in data for element types
    **/
function elementTypeResource($q, $http, umbRequestHelper) {

    return {

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
