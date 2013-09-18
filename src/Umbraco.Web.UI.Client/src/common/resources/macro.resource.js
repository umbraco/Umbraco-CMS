/**
    * @ngdoc service
    * @name umbraco.resources.macroResource
    * @description Deals with data for macros
    * 
    **/
function macroResource($q, $http, umbRequestHelper) {

    //the factory object returned
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getMacroParameters
         * @methodOf umbraco.resources.macroResource
         *
         * @description
         * Gets the editable macro parameters for the specified macro alias
         *
         * @param {int} macroId The macro id to get parameters for
         *
         */
        getMacroParameters: function (macroId) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "macroApiBaseUrl",
                       "GetMacroParameters",
                       [{ macroId: macroId }])),
               'Failed to retreive macro parameters for macro with id  ' + macroId);
        }
            
    };
}

angular.module('umbraco.resources').factory('macroResource', macroResource);
