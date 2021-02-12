/**
    * @ngdoc service
    * @name umbraco.resources.memberGroupResource
    * @description Loads in data for member groups
    **/
function memberGroupResource($q, $http, umbRequestHelper) {

    return {

        //return all member types
        getGroups: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberGroupApiBaseUrl",
                       "GetAllGroups")),
               "Failed to retrieve data for member groups");
        },       

        getById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberGroupApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               "Failed to retrieve member group");
        },

        getByIds: function (ids) {

            var idQuery = "";
            ids.forEach(id => idQuery += `ids=${id}&`);

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "memberGroupApiBaseUrl",
                        "GetByIds",
                        idQuery)),
                "Failed to retrieve member group");
        },

        deleteById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "memberGroupApiBaseUrl",
                       "DeleteById",
                       [{ id: id }])),
               "Failed to delete member group");
        },

        getScaffold: function() {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "memberGroupApiBaseUrl",
                        "GetEmpty")),
                "Failed to retrieve data for member group");
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.memberGroupResource#save
         * @methodOf umbraco.resources.memberGroupResource
         *
         * @description
         * Saves or update a member group
         *
         * @param {Object} member group object to create/update
         * @returns {Promise} resourcePromise object.
         *
         */
        save: function (memberGroup) {
            return umbRequestHelper.resourcePromise(
                 $http.post(umbRequestHelper.getApiUrl("memberGroupApiBaseUrl", "PostSave"), memberGroup),
                "Failed to save data for member group, id: " + memberGroup.id);
        }

    };
}
angular.module('umbraco.resources').factory('memberGroupResource', memberGroupResource);
