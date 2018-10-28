/**
 * @ngdoc service
 * @name umbraco.resources.relationTypeResource
 * @description Loads in data for relation types.
 */
function relationTypeResource($q, $http, umbRequestHelper) {
    return {

        /**
         * @ngdoc method
         * @name umbraco.resources.relationTypeResource#getById
         * @methodOf umbraco.resources.relationTypeResource
         * @param {Int} id of the dictionary item to get.
         * @returns {Promise} resourcePromise containing relation type data.
         */
        getById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "relationTypeApiBaseUrl",
                        "GetById",
                        [{ id: id }]
                    )
                ),
                "Failed to get item " + id
            );
        },

        save: function () {

        },

        deleteById: function (id) {

        }

    };
}

angular.module("umbraco.resources").factory("relationTypeResource", relationTypeResource);
