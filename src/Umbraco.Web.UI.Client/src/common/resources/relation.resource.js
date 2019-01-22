/**
  * @ngdoc service
  * @name umbraco.resources.relationResource
  * @description Handles loading of relation data
  **/
function relationResource($q, $http, umbRequestHelper) {
    return {

        /**
         * @ngdoc method
         * @name umbraco.resources.relationResource#getByChildId
         * @methodOf umbraco.resources.relationResource
         *
         * @description
         * Retrieves the relation data for a given child ID
         * 
         * @param {int} id of the child item
         * @param {string} alias of the relation type
         * @returns {Promise} resourcePromise object containing the relations array.
         *
         */
        getByChildId: function (id, alias) {
          
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "relationApiBaseUrl",
                        "GetByChildId",
                        { childId: id, relationTypeAlias: alias })),
                "Failed to get relation by child ID " + id + " and type of " + alias);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.relationResource#deleteById
         * @methodOf umbraco.resources.relationResource
         *
         * @description
         * Deletes a relation item with a given id
         *
         * ##usage
         * <pre>
         * relationResource.deleteById(1234)
         *    .then(function() {
         *        alert('its gone!');
         *    });
         * </pre> 
         * 
         * @param {Int} id id of relation item to delete
         * @returns {Promise} resourcePromise object.
         *
         */
        deleteById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "relationApiBaseUrl",
                        "DeleteById",
                        [{ id: id }])),
                'Failed to delete item ' + id);
        }
    };
}

angular.module('umbraco.resources').factory('relationResource', relationResource);
