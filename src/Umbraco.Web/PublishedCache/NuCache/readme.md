NuCache Documentation
======================

HOW IT WORKS
-------------

NuCache uses a ContentStore to keep content - basically, a dictionary of int => content,
and some logic to maintain it up-to-date. In order to provide immutable content to
pages rendering, a ContentStore can create ContentViews. A ContentView basically is
another int => content dictionary, containing entries only for things that have changed
in the ContentStore - so the ContentStore changes, but it updates the views so that
they

Views are chained, ie each new view is the parent of previously existing views. A view
knows its parent but not the other way round, so views just disappear when they are GC.

When reading the cache, we read views up the chain until we find a value (which may be
null) for the given id, and finally we read the store itself.


The PublishedSnapshotService manages a ContentStore for content, and another for media.
When a PublishedSnapshot is created, the PublishedSnapshotService gets ContentView objects from the stores.
Views provide an immutable snapshot of the content and media.

When the PublishedSnapshotService is notified of changes, it notifies the stores.
Then it resync the current PublishedSnapshot, so that it requires new views, etc.

Whenever a content, media or member is modified or removed, the cmsContentNu table
is updated with a json dictionary of alias => property value, so that a content,
media or member can be loaded with one database row - this is what is used to populate
the in-memory cache.


A ContentStore actually stores ContentNode instances, which contain what's common to
both the published and draft version of content + the actual published and/or draft
content.


LOCKS
------

Each ContentStore is protected by a reader/writer lock 'Locker' that is used both by
the store and its views to ensure that the store remains consistent.

Each ContentStore has a _freezeLock object used to protect the 'Frozen'
state of the store. It's a disposable object that releases the lock when disposed,
so usage would be: using (store.Frozen) { ... }.

The PublishedSnapshotService has a _storesLock object used to guarantee atomic access to the
set of content, media stores.


CACHE
------

For each set of views, the PublishedSnapshotService creates a SnapshotCache. So a SnapshotCache
is valid until anything changes in the content or media trees. In other words, things
that go in the SnapshotCache stay until a content or media is modified.

For each PublishedSnapshot, the PublishedSnapshotService creates a PublishedSnapshotCache. So a PublishedSnapshotCache is valid
for the duration of the PublishedSnapshot (usually, the request). In other words, things that go
in the PublishedSnapshotCache stay (and are visible to) for the duration of the request only.

The PublishedSnapshotService defines a static constant FullCacheWhenPreviewing, that defines
how caches operate when previewing:
- when true, the caches in preview mode work normally.
- when false, everything that would go to the SnapshotCache goes to the PublishedSnapshotCache.
At the moment it is true in the code, which means that eg converted values for
previewed content will go in the SnapshotCache. Makes for faster preview, but uses
more memory on the long term... would need some benchmarking to figure out what is
best.

Members only live for the duration of the PublishedSnapshot. So, for members SnapshotCache is
never used, and everything goes to the PublishedSnapshotCache.

All cache keys are computed in the CacheKeys static class.


TESTS
-----

For testing purposes the following mechanisms exist:

The PublishedSnapshot type has a static Current property that is used to obtain the 'current'
PublishedSnapshot in many places, going through the PublishedCachesServiceResolver to get the
current service, and asking the current service for the current PublishedSnapshot, which by
default relies on UmbracoContext. For test purposes, it is possible to override the
entire mechanism by defining PublishedSnapshot.GetCurrentPublishedSnapshotFunc which should return a PublishedSnapshot.

A PublishedContent keeps only id-references to its parent and children, and needs a
way to retrieve the actual objects from the cache - which depends on the current
PublishedSnapshot. It is possible to override the entire mechanism by defining PublishedContent.
GetContentByIdFunc or .GetMediaByIdFunc.

Setting these functions must be done before Resolution is frozen.


STATUS
------

"Detached" contents & properties, which need to be refactored anyway, are not supported
by NuCache - throwing NotImplemented in ContentCache.

All the cached elements rely on guids for the cache key, and not ints, so it could be
possible to support detached contents & properties, even those that do not have an actual
int id, but again this should be refactored entirely anyway.

Not doing any row-version checks (see XmlStore) when reloading from database, though it
is maintained in the database. Two FIXME in PublishedSnapshotService. Should we do it?

There is no on-disk cache at all so everything is reloaded from the cmsContentNu table
when the site restarts. This is pretty fast, but we should experiment with solutions to
store things locally (and deal with the sync issues, see XmlStore...).

Doing our best with PublishedMember but the whole thing should be refactored, because
PublishedMember exposes properties that IPublishedContent does not, and that are going
to be lost soon as the member is wrapped (content set, model...) - so we probably need
some sort of IPublishedMember.

/
