



angular.module("umbraco")
    .controller("umbImagePreviewController",
        function (mediaHelper) {

            var vm = this;

            vm.getThumbnail = function(source) {
                return mediaHelper.getThumbnailFromPath(source) || source;
            }

        });
