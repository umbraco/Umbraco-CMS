/**
    * @ngdoc service
    * @name umbraco.resources.sectionResource
    * @description Loads in data for section
    **/
function sectionResource($q, $http, umbRequestHelper) {

    /** internal method to get the tree app url */
    function getSectionsUrl(section) {
        return Umbraco.Sys.ServerVariables.sectionApiBaseUrl + "GetSections";
    }
   
    //the factory object returned
    return {
        /** Loads in the data to display the section list */
        getSections: function () {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "sectionApiBaseUrl",
                       "GetSections")),
               'Failed to retrieve data for sections');
		},
        
        /** Loads in all available sections */
        getAllSections: function () {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "sectionApiBaseUrl",
                       "GetAllSections")),
               'Failed to retrieve data for sections');
		}
    };
}

angular.module('umbraco.resources').factory('sectionResource', sectionResource);
