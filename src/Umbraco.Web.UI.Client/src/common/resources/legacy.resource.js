/**
    * @ngdoc service
    * @name umbraco.resources.legacyResource
    * @description Handles legacy dialog requests
    **/
function legacyResource($q, $http, umbRequestHelper) {
   
    //the factory object returned
    return {
        /** Loads in the data to display the section list */
        deleteItem: function (args) {
            
            if (!args.nodeId || !args.nodeType || !args.alias) {
                throw "The args parameter is not formatted correct, it requires properties: nodeId, nodeType, alias";
            } 

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "legacyApiBaseUrl",
                        "DeleteLegacyItem",
                        [{ nodeId: args.nodeId }, { nodeType: args.nodeType }, { alias: args.alias }])),
                'Failed to delete item ' + args.nodeId);

        }
    };
}

angular.module('umbraco.resources').factory('legacyResource', legacyResource);