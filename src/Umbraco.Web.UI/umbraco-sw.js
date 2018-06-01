self.addEventListener('install', function (event) {
    'use strict';
    var serviceCaches = new URL(location).searchParams.get('serviceCaches').split(',');
    self.serviceCaches = serviceCaches;
});

self.addEventListener('fetch', function (event) {
    'use strict';
    event.respondWith(caches.match(event.request)
        .then(function (response) {

            // Cache hit - return response
            if (response) {
                return response;
            }
            return fetch(event.request);
        })
    );
});


self.addEventListener('activate', function (event) {
    event.waitUntil(
        caches.keys().then(function (cacheNames) {
            return Promise.all(
                cacheNames.filter(function (cacheName) {
                    // Return true if you want to remove this cache,
                    // but remember that caches are shared across
                    // the whole origin
                    return !self.serviceCaches.includes(cacheName) && cacheName.startsWith('umb-');
                }).map(function (cacheName) {
                    return caches.delete(cacheName);
                })
            );
        })
    );
});
