/**
    * @ngdoc factory 
    * @name umbraco.resources.treeResource     
    * @description Loads in data for trees
    **/
function mediaResource($q, $http) {

    /** internal method to get the api url */
    function getMediaUrl(contentId) {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "GetById?id=" + contentId;
    }
    /** internal method to get the api url */
    function getRootMediaUrl() {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "GetRootMedia";
    }

    /** internal method to get the api url */
    function getChildrenMediaUrl(parentId) {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "GetChildren?parentId=" + parentId;
    }

    return {
        getById: function (id) {

            var deferred = $q.defer();

            //go and get the data
            $http.get(getMediaUrl(id)).
                success(function (data, status, headers, config) {
                    //set the first tab to active
                    _.each(data.tabs, function (item) {
                        item.active = false;
                    });
                    if (data.tabs.length > 0) {
                        data.tabs[0].active = true;
                    }

                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for media id ' + id);
                });

            return deferred.promise;
        },

        rootMedia: function () {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getRootMediaUrl()).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for application tree ' + section);
                });

            return deferred.promise;
        },

        getChildren: function (parentId) {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getChildrenMediaUrl(parentId)).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for application tree ' + section);
                });

            return deferred.promise;
        }
    };
}

angular.module('umbraco.resources').factory('mediaResource', mediaResource);
