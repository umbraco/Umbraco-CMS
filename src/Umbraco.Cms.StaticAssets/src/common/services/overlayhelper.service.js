(function() {
   'use strict';

   function overlayHelper() {

      var numberOfOverlays = 0;

      function registerOverlay() {
         numberOfOverlays++;
         return numberOfOverlays;
      }

      function unregisterOverlay() {
         numberOfOverlays--;
         return numberOfOverlays;
      }

      function getNumberOfOverlays() {
         return numberOfOverlays;
      }

      var service = {
         numberOfOverlays: numberOfOverlays,
         registerOverlay: registerOverlay,
         unregisterOverlay: unregisterOverlay,
         getNumberOfOverlays: getNumberOfOverlays
      };

      return service;

   }


   angular.module('umbraco.services').factory('overlayHelper', overlayHelper);


})();
