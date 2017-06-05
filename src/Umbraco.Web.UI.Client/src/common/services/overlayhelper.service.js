(function() {
   'use strict';

   function overlayHelper() {

      var numberOfOverlays = 0;
      var numberOfOverLaysWithBackdrop = 0;

      function registerOverlay(hideBackdrop) {
          numberOfOverlays++;

          if (hideBackdrop !== true) {
              numberOfOverLaysWithBackdrop++;
          }

         return numberOfOverlays;
      }

      function unregisterOverlay(hideBackdrop) {
          numberOfOverlays--;

          if (hideBackdrop !== true) {
              numberOfOverLaysWithBackdrop--;
          }
          
         return numberOfOverlays;
      }

      function getNumberOfOverlays() {
         return numberOfOverlays;
      }

      function showBackdrop() {
          return numberOfOverLaysWithBackdrop > 0;
      }

      var service = {
         numberOfOverlays: numberOfOverlays,
         registerOverlay: registerOverlay,
         unregisterOverlay: unregisterOverlay,
         getNumberOfOverlays: getNumberOfOverlays,
         showBackdrop: showBackdrop
      };

      return service;

   }


   angular.module('umbraco.services').factory('overlayHelper', overlayHelper);


})();
