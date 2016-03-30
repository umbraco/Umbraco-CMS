/**
    * @ngdoc service
    * @name umbraco.resources.contentTypeResource
    * @description Loads in data for content types
    **/
function contentTypeResource($q, $http, umbRequestHelper, umbDataFormatter) {

    return {

        getCount: function () {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
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
                       "contentTypeApiBaseUrl",
                       "GetAvailableCompositeContentTypes"),
                       query),
               'Failed to retrieve data for content type id ' + contentTypeId);
        },


        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#getAllowedTypes
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Returns a list of allowed content types underneath a content item with a given ID
         *
         * ##usage
         * <pre>
         * contentTypeResource.getAllowedTypes(1234)
         *    .then(function(array) {
         *        $scope.type = type;
         *    });
         * </pre>
         * @param {Int} contentTypeId id of the content item to retrive allowed child types for
         * @returns {Promise} resourcePromise object.
         *
         */
        getAllowedTypes: function (contentTypeId) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetAllowedChildren",
                       [{ contentId: contentTypeId }])),
               'Failed to retrieve data for content id ' + contentTypeId);
        },


        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#getAllPropertyTypeAliases
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Returns a list of defined property type aliases
         *
         * @returns {Promise} resourcePromise object.
         *
         */
        getAllPropertyTypeAliases: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetAllPropertyTypeAliases")),
               'Failed to retrieve property type aliases');
        },

        getPropertyTypeScaffold : function (id) {
              return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetPropertyTypeScaffold",
                       [{ id: id }])),
               'Failed to retrieve property type scaffold');
        },

        getById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retrieve content type');
        },

        deleteById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "DeleteById",
                       [{ id: id }])),
               'Failed to delete content type');
        },

        deleteContainerById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "DeleteContainer",
                       [{ id: id }])),
               'Failed to delete content type contaier');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#getAll
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Returns a list of all content types
         *
         * @returns {Promise} resourcePromise object.
         *
         */
        getAll: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetAll")),
               'Failed to retrieve all content types');
        },

        getScaffold: function (parentId) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "contentTypeApiBaseUrl",
                       "GetEmpty", { parentId: parentId })),
               'Failed to retrieve content type scaffold');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#save
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Saves or update a content type
         *
         * @param {Object} content data type object to create/update
         * @returns {Promise} resourcePromise object.
         *
         */
        save: function (contentType) {

            var saveModel = umbDataFormatter.formatContentTypePostData(contentType);

            return umbRequestHelper.resourcePromise(
                 $http.post(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "PostSave"), saveModel),
                'Failed to save data for content type id ' + contentType.id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#move
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Moves a node underneath a new parentId
         *
         * ##usage
         * <pre>
         * contentTypeResource.move({ parentId: 1244, id: 123 })
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
                $http.post(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "PostMove"),
                    {
                        parentId: args.parentId,
                        id: args.id
                    }),
                'Failed to move content');
        },

        copy: function(args) {
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
                $http.post(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "PostCopy"),
                    {
                        parentId: args.parentId,
                        id: args.id
                    }),
                'Failed to copy content');
        },

        createContainer: function(parentId, name) {

            return umbRequestHelper.resourcePromise(
                 $http.post(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "PostCreateContainer", { parentId: parentId, name: name })),
                'Failed to create a folder under parent id ' + parentId);

        }

    };
}
angular.module('umbraco.resources').factory('contentTypeResource', contentTypeResource);
