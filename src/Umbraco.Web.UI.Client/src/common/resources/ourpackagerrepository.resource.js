/**
    * @ngdoc service
    * @name umbraco.resources.ourPackageRepositoryResource
    * @description handles data for package installations
    **/
function ourPackageRepositoryResource($q, $http, umbDataFormatter, umbRequestHelper) {

    var baseurl = "http://localhost:24292/webapi/packages/v1";

    return {
        
        getDetails: function (packageId) {

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl + "/getdetails/" + packageId),
               'Failed to get package details');
        },

        getCategories: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl + "/getcategories"),
               'Failed to query packages');
        },

        getPopular: function (maxResults) {

            if (maxResults === undefined) {
                maxResults = 10;
            }

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl + "/GetPopular?maxResults=" + maxResults),
               'Failed to query packages');
        },

        getLatest: function (pageIndex, pageSize, category) {

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl + "/GetLatest?pageIndex=" + pageIndex + "&pageSize=" + pageSize + "&category=" + category),
               'Failed to query packages');
        }
        
       
    };
}

angular.module('umbraco.resources').factory('ourPackageRepositoryResource', ourPackageRepositoryResource);
