angular.module('umbraco.mocks.resources')
.factory('mediaResource', function () {
    var mediaArray = [];
    return {
        rootMedia: function(){
          return [
          {id: 1234, src: "/Media/boston.jpg", thumbnail: "/Media/boston.jpg" },
          {src: "/Media/bird.jpg", thumbnail: "/Media/bird.jpg" },
          {src: "/Media/frog.jpg", thumbnail: "/Media/frog.jpg" }
          ];
      }
  };
});