/**
    * @ngdoc service
    * @name umbraco.resources.publishedContentResource
    * @description service to retrieve published content from the umbraco cache
    * 
    *
    **/
function publishedContentResource($q, $http, umbRequestHelper) {

    //TODO: Why do we have this ? If we want a URL why isn't it just on the content controller ?

    //the factory object returned
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.resources.publishedContentResource#getUrl
         * @methodOf umbraco.resources.publishedContentResource
         *
         * @description
         * Returns a url, given a node ID
         *
         * ##usage
         * <pre>
         * publishedContentResource.getUrl()
         *    .then(function(stylesheets) {
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Int} id Id of node to return the public url to
         * @returns {Promise} resourcePromise object containing the url.
         *
         */
        getUrl: function (id) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "publishedContentApiBaseUrl",
                       "GetUrl",[{id: id}])),
               'Failed to retreive url for id:' + id);
        }
    };
}

angular.module('umbraco.resources').factory('publishedContentResource', publishedContentResource);
