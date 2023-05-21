/**
 * @ngdoc service
 * @name umbraco.resources.imageUrlGeneratorResource
 * @function
 *
 * @description
 * Used by the various controllers to get an image URL formatted correctly for the current image URL generator
 */
(function () {
    'use strict';

    function imageUrlGeneratorResource($http, umbRequestHelper) {

        function getCropUrl(mediaPath, width, height, imageCropMode) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "imageUrlGeneratorApiBaseUrl",
                        "GetCropUrl",
                        { mediaPath, width, height, imageCropMode })),
                'Failed to get crop URL');
        }


        var resource = {
            getCropUrl: getCropUrl
        };

        return resource;

    }

    angular.module('umbraco.resources').factory('imageUrlGeneratorResource', imageUrlGeneratorResource);

})();
