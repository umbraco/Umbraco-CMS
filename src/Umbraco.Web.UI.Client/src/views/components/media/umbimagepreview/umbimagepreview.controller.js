angular.module("umbraco")
  .controller("umbImagePreviewController",
    function (mediaHelper) {

      var vm = this;

      vm.getThumbnail = function (source) {
        return mediaHelper.getThumbnailFromPath(source) || source;
      }

      vm.getClientSideUrl = function (sourceData) {
        return URL.createObjectURL(sourceData);
      }

      vm.openSVG = (source) => mediaHelper.openSVG(source);
    });
