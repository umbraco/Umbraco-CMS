/**
    * @ngdoc service 
    * @name umbraco.resources.section     
    * @description Loads in data for section
    **/
function sectionResource($q, $http, angularHelper) {

    /** internal method to get the tree app url */
    function getSectionsUrl(section) {
        return Umbraco.Sys.ServerVariables.sectionApiBaseUrl + "GetSections";
    }
   
    //the factory object returned
    return {
        /** Loads in the data to display the section list */
        getSections: function () {
            
            return angularHelper.resourcePromise(
                $http.get(getSectionsUrl()),
                'Failed to retreive data for sections');
		}
    };
}

angular.module('umbraco.resources').factory('sectionResource', sectionResource);
