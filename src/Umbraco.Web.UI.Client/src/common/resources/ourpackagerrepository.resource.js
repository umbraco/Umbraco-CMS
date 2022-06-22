/**
    * @ngdoc service
    * @name umbraco.resources.ourPackageRepositoryResource
    * @description handles data for package installations
    **/
function ourPackageRepositoryResource($q, $http, umbDataFormatter, umbRequestHelper) {

    var baseurl = Umbraco.Sys.ServerVariables.umbracoUrls.packagesRestApiBaseUrl;

    return {
        
        getDetails: function (packageId) {

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl + "/" + packageId + "?version=" + Umbraco.Sys.ServerVariables.application.version),
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
               $http.get(baseurl + "?pageIndex=0&pageSize=" + maxResults + "&category=" + category + "&order=Popular&version=" + Umbraco.Sys.ServerVariables.application.version),
               'Failed to query packages');
        },

        getPromoted: function (maxResults, category) {

            if (maxResults === undefined) {
                maxResults = 20;
            }
            if (category === undefined) {
                category = "";
            }

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl + "?pageIndex=0&pageSize=" + maxResults + "&category=" + category + "&order=Popular&version=" + Umbraco.Sys.ServerVariables.application.version + "&onlyPromoted=true"),
               'Failed to query packages');
        },

        search: function (pageIndex, pageSize, orderBy, category, query, canceler) {

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

            //order by score if there is nothing set
            var order = !orderBy ? "&order=Default" : ("&order=" + orderBy);

            return umbRequestHelper.resourcePromise(
               $http.get(baseurl + "?pageIndex=" + pageIndex + "&pageSize=" + pageSize + "&category=" + category + "&query=" + query + order + "&version=" + Umbraco.Sys.ServerVariables.application.version),
               httpConfig,
               'Failed to query packages');
        }
        
       
    };
}

angular.module('umbraco.resources').factory('ourPackageRepositoryResource', ourPackageRepositoryResource);
