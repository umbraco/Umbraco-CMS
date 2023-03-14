(function () {
  "use strict";

  function mediaItemResolverFilterService(mediaResource, eventsService) {
    
    var mediaKeysRequest = [];
    var mediaItemCache = [];

    var service = {
      
      getByKey: function (key) {
        // Is it cached, then get that:
        const cachedMediaItem = mediaItemCache.find(cache => key === cache.key);
        if(cachedMediaItem) {
          return cachedMediaItem;
        }

        // check its not already being loaded, and then start loading:
        if(mediaKeysRequest.indexOf(key) === -1) {
          mediaKeysRequest.push(key);
          mediaResource.getById(key).then(function (mediaItem) {
            if(mediaItem) {
              mediaItemCache.push(mediaItem);
            }
          });
        }

        return null;
      }
    };

    eventsService.on("editors.media.saved", function (name, args) {
      const index = mediaItemCache.findIndex(cache => cache.key === args.media.key);
      if(index !== -1) {
        mediaItemCache[index] = args.media;
      }
    });

    return service;

  }
  
  angular.module("umbraco.filters").factory("mediaItemResolverFilterService", mediaItemResolverFilterService);


  // Filter loads Media Item Model from a Media Key.
  // Usage: {{ myMediaProperty[0].mediaKey | mediaItemResolver }}
  angular.module("umbraco.filters").filter("mediaItemResolver", function (mediaItemResolverFilterService) {

    mediaItemResolverFilter.$stateful = true;
    function mediaItemResolverFilter(input) {

      // Check we have a value at all
      if (typeof input === 'string' && input.length > 0) {
        return mediaItemResolverFilterService.getByKey(input);
      }

      return null;
    }

    return mediaItemResolverFilter;

  });

})();