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
        
        
    };
}

angular.module('umbraco.resources').factory('publishedContentResource', publishedContentResource);
