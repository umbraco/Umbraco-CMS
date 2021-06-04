/**
 * @ngdoc service
 * @name umbraco.resources.relationTypeResource
 * @description Loads in data for relation types.
 */
function relationTypeResource($q, $http, umbRequestHelper, umbDataFormatter) {
    return {

        /**
         * @ngdoc method
         * @name umbraco.resources.relationTypeResource#getById
         * @methodOf umbraco.resources.relationTypeResource
         *
         * @description
         * Gets a relation type with a given ID.
         *
         * ##usage
         * <pre>
         * relationTypeResource.getById(1234)
         *    .then(function() {
         *        alert('Found it!');
         *    });
         * </pre>
         *
         * @param {Int} id of the relation type to get.
         * @returns {Promise} resourcePromise containing relation type data.
         */
        getById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("relationTypeApiBaseUrl", "GetById", [{ id: id }])),
                "Failed to get item " + id
            );
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.relationTypeResource#getRelationObjectTypes
         * @methodOf umbraco.resources.relationTypeResource
         *
         * @description
         * Gets a list of Umbraco object types which can be associated with a relation.
         *
         * @returns {Object} A collection of Umbraco object types.
         */
        getRelationObjectTypes: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl("relationTypeApiBaseUrl", "GetRelationObjectTypes")
                ),
                "Failed to get object types"
            );
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.relationTypeResource#save
         * @methodOf umbraco.resources.relationTypeResource
         *
         * @description
         * Updates a relation type.
         *
         * @param {Object} relationType The relation type object to update.
         * @returns {Promise} A resourcePromise object.
         */
        save: function (relationType) {
            var saveModel = umbDataFormatter.formatRelationTypePostData(relationType);

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("relationTypeApiBaseUrl", "PostSave"), saveModel),
                "Failed to save data for relation type ID" + relationType.id
            );
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.relationTypeResource#create
         * @methodOf umbraco.resources.relationTypeResource
         *
         * @description
         * Creates a new relation type.
         *
         * @param {Object} relationType The relation type object to create.
         * @returns {Promise} A resourcePromise object.
         */
        create: function (relationType) {
            var createModel = umbDataFormatter.formatRelationTypePostData(relationType);

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("relationTypeApiBaseUrl", "PostCreate"), createModel),
                "Failed to create new realtion"
            );
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.relationTypeResource#deleteById
         * @methodOf umbraco.resources.relationTypeResource
         *
         * @description
         * Deletes a relation type with a given ID.
         *
         * * ## Usage
         * <pre>
         * relationTypeResource.deleteById(1234).then(function() {
         *    alert('Deleted it!');
         * });
         * </pre>
         *
         * @param {Int} id The ID of the relation type to delete.
         * @returns {Promose} resourcePromise object.
         */
        deleteById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("relationTypeApiBaseUrl", "DeleteById", [{ id: id }])),
                "Failed to delete item " + id
            );
        },

        getPagedResults: function (id, options) {

            var defaults = {
                pageSize: 25,
                pageNumber: 1
            };
            if (options === undefined) {
                options = {};
            }
            //overwrite the defaults if there are any specified
            Utilities.extend(defaults, options);
            //now copy back to the options we will use
            options = defaults;
            
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "relationTypeApiBaseUrl",
                        "GetPagedResults",
                        {
                            id: id,
                            pageNumber: options.pageNumber,
                            pageSize: options.pageSize
                        }
                    )),
                'Failed to get paged relations for id ' + id);
        }

    };
}

angular.module("umbraco.resources").factory("relationTypeResource", relationTypeResource);
