/**
    * @ngdoc service
    * @name umbraco.resources.mediaTypeResource
    * @description Loads in data for media types
    **/
function mediaTypeResource($q, $http, umbRequestHelper, umbDataFormatter) {

    return {

        getCount: function () {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaTypeApiBaseUrl",
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
                       "mediaTypeApiBaseUrl",
                       "GetAvailableCompositeMediaTypes"),
                       query),
               'Failed to retrieve data for content type id ' + contentTypeId);
        },
               /**
         * @ngdoc method
         * @name umbraco.resources.mediaTypeResource#getWhereCompositionIsUsedInContentTypes
         * @methodOf umbraco.resources.mediaTypeResource
         *
         * @description
         * Returns a list of media types which use a specific composition with a given id
         *
         * ##usage
         * <pre>
         * mediaTypeResource.getWhereCompositionIsUsedInContentTypes(1234)
         *    .then(function(mediaTypeList) {
         *        console.log(mediaTypeList);
         *    });
         * </pre>
         * @param {Int} contentTypeId id of the composition content type to retrieve the list of the media types where it has been used
         * @returns {Promise} resourcePromise object.
         *
         */
        getWhereCompositionIsUsedInContentTypes: function (contentTypeId) {
            var query = {
                contentTypeId: contentTypeId
            };

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "mediaTypeApiBaseUrl",
                        "GetWhereCompositionIsUsedInContentTypes"),
                    query),
                'Failed to retrieve data for content type id ' + contentTypeId);
        },
        /**
         * @ngdoc method
         * @name umbraco.resources.mediaTypeResource#getAllowedTypes
         * @methodOf umbraco.resources.mediaTypeResource
         *
         * @description
         * Returns a list of allowed media types underneath a media item with a given ID
         *
         * ##usage
         * <pre>
         * mediaTypeResource.getAllowedTypes(1234)
         *    .then(function(array) {
         *        $scope.type = type;
         *    });
         * </pre>
         * @param {Int} mediaId id of the media item to retrive allowed child types for
         * @returns {Promise} resourcePromise object.
         *
         */
        getAllowedTypes: function (mediaId) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaTypeApiBaseUrl",
                       "GetAllowedChildren",
                       [{ contentId: mediaId }])),
               'Failed to retrieve allowed types for media id ' + mediaId);
        },

        getById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaTypeApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retrieve content type');
        },

        getAll: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaTypeApiBaseUrl",
                       "GetAll")),
               'Failed to retrieve all content types');
        },

        getScaffold: function (parentId) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaTypeApiBaseUrl",
                       "GetEmpty", { parentId: parentId })),
               'Failed to retrieve content type scaffold');
        },

        deleteById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "mediaTypeApiBaseUrl",
                       "DeleteById",
                       [{ id: id }])),
               'Failed to retrieve content type');
        },

        deleteContainerById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "mediaTypeApiBaseUrl",
                       "DeleteContainer",
                       [{ id: id }])),
               'Failed to delete content type contaier');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaTypeResource#save
         * @methodOf umbraco.resources.mediaTypeResource
         *
         * @description
         * Saves or update a media type
         *
         * @param {Object} content data type object to create/update
         * @returns {Promise} resourcePromise object.
         *
         */
        save: function (contentType) {

            var saveModel = umbDataFormatter.formatContentTypePostData(contentType);

            return umbRequestHelper.resourcePromise(
                 $http.post(umbRequestHelper.getApiUrl("mediaTypeApiBaseUrl", "PostSave"), saveModel),
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
                $http.post(umbRequestHelper.getApiUrl("mediaTypeApiBaseUrl", "PostMove"),
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
                $http.post(umbRequestHelper.getApiUrl("mediaTypeApiBaseUrl", "PostCopy"),
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
                       "mediaTypeApiBaseUrl",
                       "PostCreateContainer",
                       { parentId: parentId, name: name })),
                'Failed to create a folder under parent id ' + parentId);
        },

        renameContainer: function (id, name) {

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("mediaTypeApiBaseUrl",
                    "PostRenameContainer",
                    { id: id, name: name })),
                "Failed to rename the folder with id " + id
            );

        }

    };
}
angular.module('umbraco.resources').factory('mediaTypeResource', mediaTypeResource);
