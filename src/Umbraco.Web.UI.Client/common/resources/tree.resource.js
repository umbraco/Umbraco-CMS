'use strict';

define(['angular'], function(angular) {

    /**
     * @ngdoc factory 
     * @name umbraco.resources.trees.umbTreeResource     
    **/
    function umbTreeResource($q, $http) {        

        //internal method to get the tree app url
        function getTreeAppUrl(section) {
            return Umbraco.Sys.ServerVariables.treeApplicationApiBaseUrl + "GetApplicationTrees?application=" + section;
        }

        //the factory object returned
        return {
            loadApplication: function (section) {

                var deferred = $q.defer();

                //go and get the tree data
                $http.get(getTreeAppUrl(section)).
                    success(function (data, status, headers, config) {
                        deferred.resolve(data);
                    }).
                    error(function (data, status, headers, config) {
                        deferred.reject('Failed to retreive data for application tree ' + section);
                    });

                return deferred.promise;
            }
        };
    }

    angular.module('umbraco.resources.trees', []).factory('umbTreeResource', umbTreeResource);
});