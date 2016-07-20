/**
    * @ngdoc service
    * @name umbraco.resources.ourPackageRepositoryResource
    * @description handles data for package installations
    **/
function ourPackageRepositoryResource($q, $http, umbDataFormatter, umbRequestHelper) {

    var baseurl = "https://our.umbraco.org/webapi/packages/v1";

    return {
        
        getDetails: function (packageId) {

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl + "/" + packageId),
               'Failed to get package details');
        },

        getCategories: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl),
               'Failed to query packages');
        },

        getPopular: function (maxResults, category) {

            if (maxResults === undefined) {
                maxResults = 10;
            }
            if (category === undefined) {
                category = "";
            }

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl + "?pageIndex=0&pageSize=" + maxResults + "&category=" + category + "&order=Popular"),
               'Failed to query packages');
        },
       
        search: function (pageIndex, pageSize, category, query, canceler) {

            var httpConfig = {};
            if (canceler) {
                httpConfig["timeout"] = canceler;
            }

            if (category === undefined) {
                category = "";
            }
            if (query === undefined) {
                query = "";
            }

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl + "?pageIndex=" + pageIndex + "&pageSize=" + pageSize + "&category=" + category + "&query=" + query),
               httpConfig,
               'Failed to query packages');
        }
        
       
    };
}

angular.module('umbraco.resources').factory('ourPackageRepositoryResource', ourPackageRepositoryResource);
