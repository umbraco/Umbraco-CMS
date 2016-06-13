/**
    * @ngdoc service
    * @name umbraco.resources.schemaTypeResource
    * @description Loads in data for schema types
    **/
function schemaTypeResource($q, $http, umbRequestHelper, umbDataFormatter) {

    return {

        getCount: function () {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "schemaTypeApiBaseUrl",
                       "GetCount")),
               'Failed to retrieve count');
        },

        getAvailableCompositeContentTypes: function (contentTypeId, filterContentTypes, filterPropertyTypes) {
            if (!filterContentTypes) {
                filterContentTypes = [];
            }
            if (!filterPropertyTypes) {
                filterPropertyTypes = [];
            }

            var query = {
                contentTypeId: contentTypeId,
                filterContentTypes: filterContentTypes,
                filterPropertyTypes: filterPropertyTypes
            };

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "schemaTypeApiBaseUrl",
                       "GetAvailableCompositeMediaTypes"),
                       query),
               'Failed to retrieve data for content type id ' + contentTypeId);
        },

        getById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "schemaTypeApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retrieve content type');
        },

        getAll: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "schemaTypeApiBaseUrl",
                       "GetAll")),
               'Failed to retrieve all content types');
        },

        getScaffold: function (parentId) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "schemaTypeApiBaseUrl",
                       "GetEmpty", { parentId: parentId })),
               'Failed to retrieve content type scaffold');
        },

        deleteById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "schemaTypeApiBaseUrl",
                       "DeleteById",
                       [{ id: id }])),
               'Failed to retrieve content type');
        },

        deleteContainerById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "schemaTypeApiBaseUrl",
                       "DeleteContainer",
                       [{ id: id }])),
               'Failed to delete content type contaier');
        },

        save: function (contentType) {

            var saveModel = umbDataFormatter.formatContentTypePostData(contentType);

            return umbRequestHelper.resourcePromise(
                 $http.post(umbRequestHelper.getApiUrl("schemaTypeApiBaseUrl", "PostSave"), saveModel),
                'Failed to save data for content type id ' + contentType.id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaTypeResource#move
         * @methodOf umbraco.resources.mediaTypeResource
         *
         * @description
         * Moves a node underneath a new parentId
         *
         * ##usage
         * <pre>
         * mediaTypeResource.move({ parentId: 1244, id: 123 })
         *    .then(function() {
         *        alert("node was moved");
         *    }, function(err){
         *      alert("node didnt move:" + err.data.Message);
         *    });
         * </pre>
         * @param {Object} args arguments object
         * @param {Int} args.idd the ID of the node to move
         * @param {Int} args.parentId the ID of the parent node to move to
         * @returns {Promise} resourcePromise object.
         *
         */
        move: function (args) {
            if (!args) {
                throw "args cannot be null";
            }
            if (!args.parentId) {
                throw "args.parentId cannot be null";
            }
            if (!args.id) {
                throw "args.id cannot be null";
            }

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("schemaTypeApiBaseUrl", "PostMove"),
                    {
                        parentId: args.parentId,
                        id: args.id
                    }),
                'Failed to move content');
        },

        copy: function (args) {
            if (!args) {
                throw "args cannot be null";
            }
            if (!args.parentId) {
                throw "args.parentId cannot be null";
            }
            if (!args.id) {
                throw "args.id cannot be null";
            }

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("schemaTypeApiBaseUrl", "PostCopy"),
                    {
                        parentId: args.parentId,
                        id: args.id
                    }),
                'Failed to copy content');
        },

        createContainer: function(parentId, name) {

            return umbRequestHelper.resourcePromise(
                 $http.post(
                    umbRequestHelper.getApiUrl(
                       "schemaTypeApiBaseUrl",
                       "PostCreateContainer",
                       { parentId: parentId, name: name })),
                'Failed to create a folder under parent id ' + parentId);
        }

    };
}
angular.module('umbraco.resources').factory('schemaTypeResource', schemaTypeResource);
