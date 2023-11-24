(function () {
  "use strict";

  function contentItemResolverFilterService(contentResource, eventsService) {
    
    var contentKeysRequest = [];
    var contentItemCache = [];

    var service = {
      
      getByKey: function (key) {
        // Is it cached, then get that:
        const cachedcontentItem = contentItemCache.find(cache => key === cache.key);
        if (cachedcontentItem) {
          return cachedcontentItem;
        }

        // check its not already being loaded, and then start loading:
        if (contentKeysRequest.indexOf(key) === -1) {
          contentKeysRequest.push(key);
          contentResource.getById(key).then(contentItem => {
            if (contentItem) {
              contentItemCache.push(contentItem);
            }
          });
        }

        return null;
      }
    };

    eventsService.on("content.saved", function (name, args) {
      const index = contentItemCache.findIndex(cache => cache.key === args.content.key);
      if (index !== -1) {
        contentItemCache[index] = args.content;
      }
    });

    return service;

  }
  
  angular.module("umbraco.filters").factory("contentItemResolverFilterService", contentItemResolverFilterService);


  // Filter loads content Item Model from a content Key.
  // Usage: {{ mycontentProperty[0].key | contentItemResolver }}
  angular.module("umbraco.filters").filter("contentItemResolver", function (contentItemResolverFilterService) {

    contentItemResolverFilter.$stateful = true;
    function contentItemResolverFilter(input) {

      // Check we have a value at all
      if (typeof input === 'string' && input.length > 0) {
        return contentItemResolverFilterService.getByKey(input);
      }

      return null;
    }

    return contentItemResolverFilter;

  });

})();
