/**
    * @ngdoc service
    * @name umbraco.resources.contentTypeResource
    * @description Loads in data for content types
    **/
function contentTypeResource($q, $http, umbRequestHelper, umbDataFormatter, localizationService, notificationsService) {

    return {

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#getCount
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Gets the count of content types
        *
        * ##usage
        * <pre>
        * contentTypeResource.getCount()
        *    .then(function(data) {
        *        console.log(data);
        *    });
        * </pre>
        * 
        * @returns {Promise} resourcePromise object.
        *
        */
        getCount: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentTypeApiBaseUrl",
                        "GetCount")),
                'Failed to retrieve count');
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#getAvailableCompositeContentTypes
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Gets the compositions for a content type
        *
        * ##usage
        * <pre>
        * contentTypeResource.getAvailableCompositeContentTypes()
        *    .then(function(data) {
        *        console.log(data);
        *    });
        * </pre>
        *
        * @param {Int} contentTypeId id of the content type to retrieve the list of the compositions
        * @param {Array} filterContentTypes array of content types to filter out 
        * @param {Array} filterPropertyTypes array of property aliases to filter out. If specified any content types with the property aliases will be filtered out
        * @param {Boolean} isElement whether the composite content types should be applicable for an element type
        * @returns {Promise} resourcePromise object.
        *
        */
        getAvailableCompositeContentTypes: function (contentTypeId, filterContentTypes, filterPropertyTypes, isElement) {
            if (!filterContentTypes) {
                filterContentTypes = [];
            }
            if (!filterPropertyTypes) {
                filterPropertyTypes = [];
            }

            var query = {
                contentTypeId: contentTypeId,
                filterContentTypes: filterContentTypes,
                filterPropertyTypes: filterPropertyTypes,
                isElement: isElement
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
        * @name umbraco.resources.contentTypeResource#getWhereCompositionIsUsedInContentTypes
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Returns a list of content types which use a specific composition with a given id
        *
        * ##usage
        * <pre>
        * contentTypeResource.getWhereCompositionIsUsedInContentTypes(1234)
        *    .then(function(contentTypeList) {
        *        console.log(contentTypeList);
        *    });
        * </pre>
        * @param {Int} contentTypeId id of the composition content type to retrieve the list of the content types where it has been used
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
                        "contentTypeApiBaseUrl",
                        "GetWhereCompositionIsUsedInContentTypes"),
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
         * 
         * @param {Int} contentTypeId id of the content item to retrieve allowed child types for
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
         * ##usage
         * <pre>
         * contentTypeResource.getAllPropertyTypeAliases()
         *    .then(function(array) {
         *       Do stuff...
         *    });
         * </pre>
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

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#getAllStandardFields
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Returns a list of standard property type aliases
        *
        * ##usage
        * <pre>
        * contentTypeResource.getAllStandardFields()
        *    .then(function(array) {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @returns {Promise} resourcePromise object.
        *
        */
        getAllStandardFields: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentTypeApiBaseUrl",
                        "GetAllStandardFields")),
                'Failed to retrieve standard fields');
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#getPropertyTypeScaffold
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Returns the property display for a given datatype id
        *
        * ##usage
        * <pre>
        * contentTypeResource.getPropertyTypeScaffold(1234)
        *    .then(function(array) {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @param {Int} id the id of the datatype
        * @returns {Promise} resourcePromise object.
        *
        */
        getPropertyTypeScaffold: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentTypeApiBaseUrl",
                        "GetPropertyTypeScaffold",
                        [{ id: id }])),
                'Failed to retrieve property type scaffold');
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#getById
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Get the content type with a given id
        *
        * ##usage
        * <pre>
        * contentTypeResource.getById("64058D0F-4911-4AB7-B3BA-000D89F00A26")
        *    .then(function(array) {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @param {String} id the guid id of the content type
        * @returns {Promise} resourcePromise object.
        *
        */
        getById: function (id) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentTypeApiBaseUrl",
                        "GetById",
                        [{ id: id }])),
                'Failed to retrieve content type');
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#deleteById
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Delete the content type of a given id
        *
        * ##usage
        * <pre>
        * contentTypeResource.deleteById(1234)
        *    .then(function(array) {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @param {Int} id the id of the content type
        * @returns {Promise} resourcePromise object.
        *
        */
        deleteById: function (id) {

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "contentTypeApiBaseUrl",
                        "DeleteById",
                        [{ id: id }])),
                'Failed to delete content type');
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#deleteContainerById
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Delete the content type container of a given id
        *
        * ##usage
        * <pre>
        * contentTypeResource.deleteContainerById(1234)
        *    .then(function(array) {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @param {Int} id the id of the content type container
        * @returns {Promise} resourcePromise object.
        *
        */
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

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#getScaffold
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Returns an empty content type for use as a scaffold when creating a new content type
        *
        * ##usage
        * <pre>
        * contentTypeResource.getScaffold(1234)
        *    .then(function(array) {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @param {Int} id the parent id
        * @returns {Promise} resourcePromise object.
        *
        */
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
         *        alert("content type was moved");
         *    }, function(err){
         *      alert("content type didnt move:" + err.data.Message);
         *    });
         * </pre>
         * @param {Object} args arguments object
         * @param {Int} args.id the ID of the content type to move
         * @param {Int} args.parentId the ID of the parent content type to move to
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
                    }, { responseType: 'text' }),
              'Failed to move content type');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#copy
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Copied a content type underneath a new parentId
         *
         * ##usage
         * <pre>
         * contentTypeResource.copy({ parentId: 1244, id: 123 })
         *    .then(function() {
         *        alert("content type was copied");
         *    }, function(err){
         *      alert("content type didnt copy:" + err.data.Message);
         *    });
         * </pre>
         * @param {Object} args arguments object
         * @param {Int} args.id the ID of the content type to copy
         * @param {Int} args.parentId the ID of the parent content type to copy to
         * @returns {Promise} resourcePromise object.
         *
         */
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

            var promise = localizationService.localize("contentType_copyFailed");

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "PostCopy"),
                    {
                        parentId: args.parentId,
                        id: args.id
                    }, { responseType: 'text' }),
                promise);
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#createContainer
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Create a new content type container of a given name underneath a given parent item
        *
        * ##usage
        * <pre>
        * contentTypeResource.createContainer(1244,"testcontainer")
        *    .then(function() {
        *       Do stuff..
        *    });
        * </pre>
        * 
        * @param {Int} parentId the ID of the parent content type underneath which to create the container
        * @param {String} name the name of the container
        * @returns {Promise} resourcePromise object.
        *
        */
        createContainer: function (parentId, name) {

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "PostCreateContainer", { parentId: parentId, name: encodeURIComponent(name) })),
                'Failed to create a folder under parent id ' + parentId);


        }, 

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#renameContainer
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Rename a container of a given id
        *
        * ##usage
        * <pre>
        * contentTypeResource.renameContainer( 1244,"testcontainer")
        *    .then(function() {
        *       Do stuff..
        *    });
        * </pre>
        *
        * @param {Int} id the ID of the container to rename
        * @param {String} name the new name of the container
        * @returns {Promise} resourcePromise object.
        *
        */
        renameContainer: function (id, name) {

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl",
                    "PostRenameContainer",
                    { id: id, name: name })),
                "Failed to rename the folder with id " + id
            );

        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#export
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Export a content type of a given id.
        *
        * ##usage
        * <pre>
        * contentTypeResource.export(1234){
        *    .then(function() {
        *       Do stuff..
        *    });
        * </pre>
        *
        * @param {Int} id the ID of the container to rename
        * @param {String} name the new name of the container
        * @returns {Promise} resourcePromise object.
        *
        */
        export: function (id) {
            if (!id) {
                throw "id cannot be null";
            }

            var url = umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "Export", { id: id });

            return umbRequestHelper.downloadFile(url).then(function () {
                localizationService.localize("speechBubbles_documentTypeExportedSuccess").then(function(value) {
                    notificationsService.success(value);
                });
            }, function (data) {
                localizationService.localize("speechBubbles_documentTypeExportedError").then(function(value) {
                    notificationsService.error(value);
                });
            });
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#import
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Import a content type from a file
        *
        * ##usage
        * <pre>
        * contentTypeResource.import("path to file"){
        *    .then(function() {
        *       Do stuff..
        *    });
        * </pre>
        *
        * @param {String} file path of the file to import
        * @returns {Promise} resourcePromise object.
        *
        */
        import: function (file) {
            if (!file) {
                throw "file cannot be null";
            }

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "Import", { file: file })),
                "Failed to import document type " + file
            );
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#createDefaultTemplate
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Create a default template for a content type with a given id
        *
        * ##usage
        * <pre>
        * contentTypeResource.createDefaultTemplate(1234){
        *    .then(function() {
        *       Do stuff..
        *    });
        * </pre>
        *
        * @param {Int} id the id of the content type for which to create the default template
        * @returns {Promise} resourcePromise object.
        *
        */
        createDefaultTemplate: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "PostCreateDefaultTemplate", { id: id })),
                'Failed to create default template for content type with id ' + id);
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentTypeResource#hasContentNodes
        * @methodOf umbraco.resources.contentTypeResource
        *
        * @description
        * Returns whether a content type has content nodes
        *
        * ##usage
        * <pre>
        * contentTypeResource.hasContentNodes(1234){
        *    .then(function() {
        *       Do stuff..
        *    });
        * </pre>
        *
        * @param {Int} id the id of the content type
        * @returns {Promise} resourcePromise object.
        *
        */
        hasContentNodes: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentTypeApiBaseUrl",
                        "HasContentNodes",
                        [{ id: id }])),
                'Failed to retrieve indication for whether content type with id ' + id + ' has associated content nodes');
        },

        allowsCultureVariation: function () {
          return umbRequestHelper.resourcePromise(
            $http.get(
              umbRequestHelper.getApiUrl(
                "contentTypeApiBaseUrl",
                "AllowsCultureVariation")),
            'Failed to retrieve variant content types');
        }
    };
}
angular.module('umbraco.resources').factory('contentTypeResource', contentTypeResource);
