(function() {
   'use strict';

   function entityHelper() {

        function getEntityTypeFromSection(section) {
            if (section === "member") {
                return "Member";
            }
            else if (section === "media") {
                return "Media";
            } else {
                return "Document";
            }
        }

        ////////////

        var service = {
            getEntityTypeFromSection: getEntityTypeFromSection
        };

        return service;

   }

   angular.module('umbraco.services').factory('entityHelper', entityHelper);

})();
