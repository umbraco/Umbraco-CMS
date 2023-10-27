describe('tree service tests', function () {
    var treeService, $rootScope, $httpBackend, mocks;

    function getContentTree() {

        var url = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetChildren?treeType=content&id=1234&level=1";
        var menuUrl = "/umbraco/UmbracoTrees/ApplicationTreeApi/GetMenu?treeType=content&id=1234&parentId=456";

        var t = {
            name: "content",
            id: -1,
            children: [
                {
                    name: "My website", id: 1234, childNodesUrl: url, icon: "icon-home", expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl,
                    children: [
                        { name: "random-name-1", childNodesUrl: url, id: 11, icon: "icon-home",  children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                        { name: "random-name-2", childNodesUrl: url, id: 12, icon: "icon-folder-close",  children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                        { name: "random-name-3", childNodesUrl: url, id: 13, icon: "icon-folder-close",  children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl },
                        { name: "random-name-4", childNodesUrl: url, id: 14, icon: "icon-folder-close",  children: [], expanded: false, hasChildren: true, level: 1, menuUrl: menuUrl }
                    ]
                },
                { name: "Components", id: 1235, childNodesUrl: url, icon: "icon-cogs", children: [], expanded: false, hasChildren: true, level: 1,  menuUrl: menuUrl },
                { name: "Archieve", id: 1236, childNodesUrl: url, icon: "icon-folder-close", children: [], expanded: false, hasChildren: true, level: 1,  menuUrl: menuUrl },
                { name: "Recycle Bin", id: -20, childNodesUrl: url, icon: "icon-trash", routePath: "content/recyclebin", children: [], expanded: false, hasChildren: true, level: 1,  menuUrl: menuUrl }
            ],
            expanded: true,
            hasChildren: true,
            level: 0,
            menuUrl: menuUrl,
            metaData: { treeAlias: "content" }
        };

        treeService._formatNodeDataForUseInUI(t, t.children, "content", 0);
        treeService._formatNodeDataForUseInUI(t.children[0], t.children[0].children, "content", 1);

        return t;
    }

    beforeEach(module('umbraco.services'));
    beforeEach(module('umbraco.resources'));
    beforeEach(module('umbraco.mocks'));
    beforeEach(module('ngRoute'));

    beforeEach(inject(function ($injector, mocksUtils) {

        //for these tests we don't want any authorization to occur
        mocksUtils.disableAuth();

        $rootScope = $injector.get('$rootScope');
        $httpBackend = $injector.get('$httpBackend');
        mocks = $injector.get("treeMocks");
        mocks.register();
        treeService = $injector.get('treeService');
    }));

    describe('tree cache', function () {

        it('does not cache with no args', function () {

            var cache;
            treeService.getTree().then(function (data) {
                cache = treeService._getTreeCache();
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(_.keys(cache).length).toBe(0);
        });

        it('does not cache with no cacheKey', function () {
            var cache;
            treeService.getTree({section: "content"}).then(function (data) {
                cache = treeService._getTreeCache();

            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(_.keys(cache).length).toBe(0);
        });

        it('caches by section with cache key', function () {
            var cache;
            treeService.getTree({ section: "media", cacheKey: "_" }).then(function (data) {
                cache = treeService._getTreeCache();
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(cache["__media"]).toBeDefined();
        });

        it('caches by default content section with cache key', function () {
            var cache;
            treeService.getTree({ cacheKey: "_" }).then(function (data) {
                cache = treeService._getTreeCache();
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(cache).toBeDefined();
            expect(cache["__content"]).toBeDefined();
        });

        it('removes by section with cache key', function () {
            var cache;
            treeService.getTree({ section: "media", cacheKey: "_" }).then(function (data) {
                treeService.getTree({ section: "content", cacheKey: "_" }).then(function (d) {
                    cache = treeService._getTreeCache();
                });
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(cache["__media"]).toBeDefined();
            expect(_.keys(cache).length).toBe(2);

            treeService.clearCache({ section: "media", cacheKey: "_" });
            cache = treeService._getTreeCache();

            expect(cache["__media"]).not.toBeDefined();
            expect(_.keys(cache).length).toBe(1);

        });

        it('removes cache by key regardless of section', function () {
            var cache;

            treeService.getTree({ section: "media", cacheKey: "_" }).then(function (data) {
                treeService.getTree({ section: "content", cacheKey: "_" }).then(function (d) {
                    treeService.getTree({ section: "content", cacheKey: "anotherkey" }).then(function (dd) {
                        cache = treeService._getTreeCache();
                    });
                });
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(cache["__media"]).toBeDefined();
            expect(cache["__content"]).toBeDefined();
            expect(_.keys(cache).length).toBe(3);

            treeService.clearCache({ cacheKey: "_" });

            cache = treeService._getTreeCache();
            expect(cache["__media"]).not.toBeDefined();
            expect(cache["__content"]).not.toBeDefined();
            expect(_.keys(cache).length).toBe(1);
        });

        it('removes all section cache regardless of key', function () {

            var cache;

            treeService.getTree({ section: "media", cacheKey: "_" }).then(function (data) {
                treeService.getTree({ section: "media", cacheKey: "anotherkey" }).then(function (d) {
                    treeService.getTree({ section: "content", cacheKey: "anotherkey" }).then(function (dd) {
                        cache = treeService._getTreeCache();
                    });
                });
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(cache["anotherkey_media"]).toBeDefined();
            expect(cache["__media"]).toBeDefined();
            expect(_.keys(cache).length).toBe(3);

            treeService.clearCache({ section: "media" });

            cache = treeService._getTreeCache();
            expect(cache["anotherkey_media"]).not.toBeDefined();
            expect(cache["__media"]).not.toBeDefined();
            expect(_.keys(cache).length).toBe(1);
        });

        it('removes all cache', function () {

            var cache;

            treeService.getTree({ section: "media", cacheKey: "_" }).then(function (data) {
                treeService.getTree({ section: "media", cacheKey: "anotherkey" }).then(function (d) {
                    treeService.getTree({ section: "content", cacheKey: "anotherkey" }).then(function(dd) {
                        cache = treeService._getTreeCache();
                    });
                });
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(cache["anotherkey_media"]).toBeDefined();
            expect(cache["__media"]).toBeDefined();
            expect(cache["anotherkey_content"]).toBeDefined();
            expect(_.keys(cache).length).toBe(3);

            treeService.clearCache();

            cache = treeService._getTreeCache();
            expect(_.keys(cache).length).toBe(0);
        });

        it('clears cache by key using a filter that returns null', function () {

            var cache;

            treeService.getTree({ section: "media", cacheKey: "_" }).then(function (d) {
                treeService.getTree({ section: "content", cacheKey: "_" }).then(function (dd) {
                    cache = treeService._getTreeCache();
                });
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(_.keys(cache).length).toBe(2);

            treeService.clearCache({
                cacheKey: "__content",
                filter: function(currentCache) {
                    return null;
                }
            });

            cache = treeService._getTreeCache();

            expect(_.keys(cache).length).toBe(1);
        });

        it('removes cache by key using a filter', function () {

            var cache;

            treeService.getTree({ section: "media", cacheKey: "_" }).then(function (d) {
                treeService.getTree({ section: "content", cacheKey: "_" }).then(function (dd) {
                    cache = treeService._getTreeCache();
                });
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(_.keys(cache).length).toBe(2);
            expect(cache.__content.root.children.length).toBe(4);

            treeService.clearCache({
                cacheKey: "__content",
                filter: function (currentCache) {
                    var toRemove = treeService.getDescendantNode(currentCache.root, 1235);
                    toRemove.parent().children = _.without(toRemove.parent().children, toRemove);
                    return currentCache;
                }
            });

            cache = treeService._getTreeCache();

            expect(cache.__content.root.children.length).toBe(3);
        });

        it('removes cache children for a parent id specified', function () {

            var cache;

            treeService.getTree({ section: "content", cacheKey: "_" }).then(function (dd) {
                treeService.loadNodeChildren({ node: dd.root.children[0] }).then(function () {
                    cache = treeService._getTreeCache();
                });
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(cache.__content.root.children.length).toBe(4);
            expect(cache.__content.root.children[0].children.length).toBe(4);

            treeService.clearCache({
                cacheKey: "__content",
                childrenOf: "1234"
            });

            cache = treeService._getTreeCache();

            expect(cache.__content.root.children.length).toBe(4);
            expect(cache.__content.root.children[0].children).toBeNull();
            expect(cache.__content.root.children[0].expanded).toBe(false);
        });

    });

    describe('lookup plugin based trees', function() {

        it('can find a plugin based tree', function () {
            //we know this exists in the mock umbraco server vars
            var found = treeService.getTreePackageFolder("myTree");
            expect(found).toBe("MyPackage");
        });

        it('returns undefined for a not found tree', function () {
            //we know this does not exist in the mock umbraco server vars
            var found = treeService.getTreePackageFolder("asdfasdf");
            expect(found).not.toBeDefined();
        });

    });

    describe('Remove existing nodes', function() {

        it('hasChildren has to be updated on parent', function () {
            var tree = getContentTree();

            while (tree.children.length > 0) {
                treeService.removeNode(tree.children[0]);
            }

            expect(tree.hasChildren).toBe(false);
        });

    });

    describe('query existing node structure of the tree', function () {

        it('can get a descendant node with string id', function () {

            var tree = getContentTree();
            var found = treeService.getDescendantNode(tree, "13");

            expect(found).toBeDefined();
            expect(found).not.toBeNull();
            expect(found.id).toBe(13);
            expect(found.name).toBe("random-name-3");
        });

        it('can get a descendant node', function() {

            var tree = getContentTree();
            var found = treeService.getDescendantNode(tree, 13);

            expect(found).toBeDefined();
            expect(found).not.toBeNull();
            expect(found.id).toBe(13);
            expect(found.name).toBe("random-name-3");
        });

        it('returns null for a descendant node that doesnt exist', function () {

            var tree = getContentTree();
            var found = treeService.getDescendantNode(tree, 123456);

            expect(found).toBeNull();
        });

        it('can get a child node', function () {

            var tree = getContentTree();
            var found = treeService.getChildNode(tree, 1235);

            expect(found).toBeDefined();
            expect(found).not.toBeNull();
            expect(found.id).toBe(1235);
            expect(found.name).toBe("Components");
        });

        it('returns null for a child node that doesnt exist', function () {

            var tree = getContentTree();
            var found = treeService.getChildNode(tree, 123456);

            expect(found).toBeNull();
        });

        it('returns null for a descendant node that doesnt exist', function () {

            var tree = getContentTree();
            var found = treeService.getDescendantNode(tree, 123456);

            expect(found).toBeNull();
        });

        it('can get the root node from a child node', function () {

            var tree = getContentTree();
            var testNode = tree.children[0].children[3];
            var root = treeService.getTreeRoot(testNode);

            expect(root).toBeDefined();
            expect(root).not.toBeNull();
            expect(root.id).toBe(-1);
            expect(root.name).toBe("content");
        });

        it('can get the root node from the root node', function () {

            var tree = getContentTree();
            var root = treeService.getTreeRoot(tree);

            expect(root).toBeDefined();
            expect(root).not.toBeNull();
            expect(root.id).toBe(-1);
            expect(root.name).toBe("content");
        });

    });
});
