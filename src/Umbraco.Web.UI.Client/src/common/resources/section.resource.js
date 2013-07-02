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
        getSections: function () {
           
            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getSectionsUrl()).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for sections');
                });

            return deferred.promise;
        }
    };
}

angular.module('umbraco.resources').factory('sectionResource', sectionResource);