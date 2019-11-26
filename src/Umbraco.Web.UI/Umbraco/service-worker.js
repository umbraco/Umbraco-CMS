 var currentCache;
 const cachePrefix = 'umbraco-';
 
 function IsCacheable(event) {
    const cacheableURLRegexes = [
        /\/umbraco\/assets\/*/
        ,/\/umbraco\/lib\/*/
        ,/\/umbraco\/views\/*/
        ,/\/umbraco\/js\/*/
        ,/\/DependencyHandler\.axd/
        ,/\/umbraco\/Application/
        ,/\/umbraco$/
        /* ,/\/umbraco\/ServerVariables$/
        This shouldnt really change and has the same cache versioning as we use for the sw cache.
        However changes to the response can be done without the hash changing*/
    ];
    var result = cacheableURLRegexes.some(reg => { return reg.test(new URL(event.request.url).pathname); });
    return result;
}

self.addEventListener('install', event => {
    event.waitUntil(async function() {
        let version = new URL(location).searchParams.get('version');
        currentCache = cachePrefix + version;
        await caches.open(currentCache)
    }());
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
        return Promise.all(
            cacheNames.filter(cacheName => {
            if(cacheName.indexOf(cachePrefix) == 0) {
                return cacheName !== currentCache;
            }
            return false;
            }).map(cacheName => { return caches.delete(cacheName); })
        );
        })
    );
});

self.addEventListener('fetch', event => {
    if(IsCacheable(event)) 
    {
        event.respondWith(
            caches.open(currentCache).then(function(cache) {
              return cache.match(event.request).then(function (cachedResponse) {
                return cachedResponse || fetch(event.request).then(function(response) {
                  cache.put(event.request, response.clone());
                  return response;
                });
              });
            })
          );
    }
});