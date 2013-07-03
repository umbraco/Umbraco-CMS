/**
    * @ngdoc factory 
    * @name umbraco.resources.section     
    * @description Loads in data for section
    **/
function sectionResource($q, $http) {

    /** internal method to get the tree app url */
    function getSectionsUrl(section) {
        return Umbraco.Sys.ServerVariables.sectionApiBaseUrl + "GetSections";
    }
   
    //the factory object returned
    return {
        /** Loads in the data to display the section list */
        getSections: function (options) {
            return $http.get(getSectionsUrl())
                .then(function (response) {
                    return response.data;
                }, function (response) {
                    throw new Error('Failed to retreive data for content id ' + id);
                });
		}
    };
}

angular.module('umbraco.resources').factory('sectionResource', sectionResource);
