(function() {
   'use strict';

   function platformService() {

        function isMac() {
            return navigator.platform.toUpperCase().indexOf('MAC')>=0;
        }

        ////////////

        var service = {
            isMac: isMac
        };

        return service;

   }

   angular.module('umbraco.services').factory('platformService', platformService);


})();
