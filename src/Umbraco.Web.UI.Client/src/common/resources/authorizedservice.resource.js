function authorizedServiceResource($q, $http, umbRequestHelper) {

    return {

        getByAlias: function(alias) {
            return umbRequestHelper.resourcePromise(
              $http.get(umbRequestHelper.getApiUrl("authorizedServiceApiBaseUrl", "GetByAlias", { "alias": alias }), "Failed to get service details.")
            );
        }
    };
}

angular.module('umbraco.resources').factory('authorizedServiceResource', authorizedServiceResource);
