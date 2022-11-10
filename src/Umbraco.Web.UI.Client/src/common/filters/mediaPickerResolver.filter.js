(function () {
  "use strict";

  function mediaPickerResolverFilterService(eventsService) {
    
    var service = {
      mediaKeysRequest: [],
      mediaEntryCache: []
    };

    eventsService.on("editors.media.saved", function (name, args) {
      const index = service.mediaEntryCache.findIndex(cache => cache.key === args.media.key);
      if(index !== -1) {
        service.mediaEntryCache[index] = args.media;
      }
    });

    return service;

  }
  
  angular.module("umbraco.filters").factory("mediaPickerResolverFilterService", mediaPickerResolverFilterService);


  // Filter to take a node id and grab it's name instead
  // Usage: {{ pickerAlias | ncNodeName }}
  angular.module("umbraco.filters").filter("mediaPickerResolverFilter", function (mediaResource, mediaPickerResolverFilterService) {

    mediaPickerResolverFilter.$stateful = true;
    function mediaPickerResolverFilter(input) {



      // Check we have a value at all
      if (typeof input === 'object') {
        if(input.length > 0) {
          var keys = input.map(x => x.mediaKey);

          const notLoadedKeys = keys.filter(key => mediaPickerResolverFilterService.mediaEntryCache.find(cache => key === cache.key) === undefined);
          const notRequestedKeys = notLoadedKeys.filter(key => mediaPickerResolverFilterService.mediaKeysRequest.indexOf(key) === -1);
          
          mediaPickerResolverFilterService.mediaKeysRequest = [...mediaPickerResolverFilterService.mediaKeysRequest, ...notRequestedKeys];

          if(notRequestedKeys.length > 0) {
            mediaResource.getById(notRequestedKeys[0]).then(function (mediaEntities) {
              mediaPickerResolverFilterService.mediaEntryCache.push(mediaEntities);
            });
          } else {
            return mediaPickerResolverFilterService.mediaEntryCache.filter(cache => keys.indexOf(cache.key) !== -1);
          }
        }
      }

      return [];
    }

    return mediaPickerResolverFilter;

  });

})();