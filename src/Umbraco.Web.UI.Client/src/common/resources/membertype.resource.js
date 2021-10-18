/**
    * @ngdoc service
    * @name umbraco.resources.memberTypeResource
    * @description Loads in data for member types
    **/
function memberTypeResource($q, $http, umbRequestHelper, umbDataFormatter, localizationService) {

    return {

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

        getScaffold: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberTypeApiBaseUrl",
                       "GetEmpty")),
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
        }
    };
}
angular.module('umbraco.resources').factory('memberTypeResource', memberTypeResource);
