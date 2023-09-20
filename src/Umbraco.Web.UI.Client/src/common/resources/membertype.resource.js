/**
    * @ngdoc service
    * @name umbraco.resources.memberTypeResource
    * @description Loads in data for member types
    **/
function memberTypeResource($q, $http, umbRequestHelper, umbDataFormatter, localizationService) {

    return {
        getCount: function () {
          return umbRequestHelper.resourcePromise(
            $http.get(
              umbRequestHelper.getApiUrl(
                "memberTypeApiBaseUrl",
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

            var query = "";
            filterContentTypes.forEach(fct => query += `filterContentTypes=${fct}&`);

            // if filterContentTypes array is empty we need a empty variable in the querystring otherwise the service returns a error
            if (filterContentTypes.length === 0) {
                query += "filterContentTypes=&";
            }

            filterPropertyTypes.forEach(fpt => query += `filterPropertyTypes=${fpt}&`);

            // if filterPropertyTypes array is empty we need a empty variable in the querystring otherwise the service returns a error
            if (filterPropertyTypes.length === 0) {
                query += "filterPropertyTypes=&";
            }
            query += "contentTypeId=" + contentTypeId;

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberTypeApiBaseUrl",
                       "GetAvailableCompositeMemberTypes",
                       query)),
               'Failed to retrieve data for content type id ' + contentTypeId);
        },
        getWhereCompositionIsUsedInContentTypes: function (contentTypeId) {
            var query = "contentTypeId=" + contentTypeId;

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "memberTypeApiBaseUrl",
                        "GetWhereCompositionIsUsedInMemberTypes",
                        query)),
                "Failed to retrieve data for content type id " + contentTypeId);
        },
        //return all member types
        getTypes: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberTypeQueryApiBaseUrl",
                       "GetAllTypes")),
               'Failed to retrieve data for member types id');
        },

        getById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberTypeApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retrieve content type');
        },

        deleteById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "memberTypeApiBaseUrl",
                       "DeleteById",
                       [{ id: id }])),
               'Failed to delete member type');
        },

        deleteContainerById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "memberTypeApiBaseUrl",
                       "DeleteContainer",
                       [{ id: id }])),
               'Failed to delete member type container');
        },

        getScaffold: function (parentId) {

            // For backwards compatibility since parentId parameter has been added.
            parentId = parentId ?? -1;

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberTypeApiBaseUrl",
                       "GetEmpty", { parentId: parentId })),
               'Failed to retrieve content type scaffold');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.memberTypeResource#save
         * @methodOf umbraco.resources.memberTypeResource
         *
         * @description
         * Saves or update a member type
         *
         * @param {Object} content data type object to create/update
         * @returns {Promise} resourcePromise object.
         *
         */
        save: function (contentType) {

            var saveModel = umbDataFormatter.formatContentTypePostData(contentType);

            return umbRequestHelper.resourcePromise(
                 $http.post(umbRequestHelper.getApiUrl("memberTypeApiBaseUrl", "PostSave"), saveModel),
                'Failed to save data for member type id ' + contentType.id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.memberTypeResource#move
         * @methodOf umbraco.resources.memberTypeResource
         *
         * @description
         * Moves a node underneath a new parentId
         *
         * ##usage
         * <pre>
         * memberTypeResource.move({ parentId: 1244, id: 123 })
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
                $http.post(umbRequestHelper.getApiUrl("memberTypeApiBaseUrl", "PostMove"),
                    {
                        parentId: args.parentId,
                        id: args.id
                    }, { responseType: 'text' }),
                'Failed to move member type');
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

            var promise = localizationService.localize("memberType_copyFailed");

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("memberTypeApiBaseUrl", "PostCopy"),
                    {
                        parentId: args.parentId,
                        id: args.id
                    }, { responseType: 'text' }),
                promise);
        },

        createContainer: function (parentId, name) {

          var promise = localizationService.localize("member_createFolderFailed", [parentId]);

          return umbRequestHelper.resourcePromise(
            $http.post(
              umbRequestHelper.getApiUrl(
                "memberTypeApiBaseUrl",
                "PostCreateContainer",
                { parentId: parentId, name: encodeURIComponent(name) })),
            promise);
        },

        renameContainer: function (id, name) {

          var promise = localizationService.localize("member_renameFolderFailed", [id]);

          return umbRequestHelper.resourcePromise(
            $http.post(umbRequestHelper.getApiUrl("memberTypeApiBaseUrl",
              "PostRenameContainer",
              { id: id, name: name })),
            promise
          );

        }
    };
}
angular.module('umbraco.resources').factory('memberTypeResource', memberTypeResource);
