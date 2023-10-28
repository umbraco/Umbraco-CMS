/**
* @ngdoc service
* @name umbraco.services.imageHelper
* @deprecated
**/
function imageHelper(umbRequestHelper, mediaHelper) {
    return {
        /**
         * @ngdoc function
         * @name umbraco.services.imageHelper#getImagePropertyValue
         * @methodOf umbraco.services.imageHelper
         * @function    
         *
         * @deprecated
         */
        getImagePropertyValue: function (options) {
            return mediaHelper.getImagePropertyValue(options);
        },
        /**
         * @ngdoc function
         * @name umbraco.services.imageHelper#getThumbnail
         * @methodOf umbraco.services.imageHelper
         * @function    
         *
         * @deprecated
         */
        getThumbnail: function (options) {
            return mediaHelper.getThumbnail(options);
        },

        /**
         * @ngdoc function
         * @name umbraco.services.imageHelper#scaleToMaxSize
         * @methodOf umbraco.services.imageHelper
         * @function    
         *
         * @deprecated
         */
        scaleToMaxSize: function (maxSize, width, height) {
            return mediaHelper.scaleToMaxSize(maxSize, width, height);
        },

        /**
         * @ngdoc function
         * @name umbraco.services.imageHelper#getThumbnailFromPath
         * @methodOf umbraco.services.imageHelper
         * @function    
         *
         * @deprecated
         */
        getThumbnailFromPath: function (imagePath) {
            return mediaHelper.getThumbnailFromPath(imagePath);
        },

        /**
         * @ngdoc function
         * @name umbraco.services.imageHelper#detectIfImageByExtension
         * @methodOf umbraco.services.imageHelper
         * @function    
         *
         * @deprecated
         */
        detectIfImageByExtension: function (imagePath) {
            return mediaHelper.detectIfImageByExtension(imagePath);
        }
    };
}
angular.module('umbraco.services').factory('imageHelper', imageHelper);