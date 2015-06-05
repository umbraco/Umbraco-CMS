/**
    * @ngdoc service
    * @name umbraco.resources.memberTypeResource
    * @description Loads in data for member types
    **/
function memberTypeResource($q, $http, umbRequestHelper) {

    return {

        //return all member types
        getTypes: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberTypeApiBaseUrl",
                       "GetAllTypes")),
               'Failed to retrieve data for member types id');
        },

        getPropertyTypeScaffold : function (id) {
              return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberTypeApiBaseUrl",
                       "GetPropertyTypeScaffold",
                       [{ id: id }])),
               'Failed to retrieve property type scaffold');
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

        getEmpty: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberTypeApiBaseUrl",
                       "GetEmpty")),
               'Failed to retrieve content type scaffold');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentTypeResource#save
         * @methodOf umbraco.resources.contentTypeResource
         *
         * @description
         * Saves or update a member type       
         * 
         * @param {Object} content data type object to create/update
         * @returns {Promise} resourcePromise object.
         *
         */
        save: function (contentType) {
            
            return umbRequestHelper.resourcePromise(
                 $http.post(umbRequestHelper.getApiUrl("memberTypeApiBaseUrl", "PostSave"), contentType),
                'Failed to save data for member type id ' + contentType.id);
        }

    };
}
angular.module('umbraco.resources').factory('memberTypeResource', memberTypeResource);
