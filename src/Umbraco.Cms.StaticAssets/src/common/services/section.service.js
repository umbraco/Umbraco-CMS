/**
 * @ngdoc service
 * @name umbraco.services.sectionService
 *
 *  
 * @description
 * A service to return the sections (applications) to be listed in the navigation which are contextual to the current user
 */
(function () {
    'use strict';

    function sectionService(userService, $q, sectionResource) {

        function getSectionsForUser() {
            var deferred = $q.defer();
            userService.getCurrentUser().then(function (u) {
                //if they've already loaded, return them
                if (u.sections) {
                    deferred.resolve(u.sections);
                }
                else {
                    sectionResource.getSections().then(function (sections) {
                        //set these to the user (cached), then the user changes, these will be wiped
                        u.sections = sections;
                        deferred.resolve(u.sections);
                    });   
                }                
            });
            return deferred.promise;
        }
        
        var service = {
            getSectionsForUser: getSectionsForUser
        };

        return service;

    }

    angular.module('umbraco.services').factory('sectionService', sectionService);


})();
