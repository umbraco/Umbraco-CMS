describe('tree service tests', function () {
    var treeService;

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

    beforeEach(inject(function ($injector) {
        treeService = $injector.get('treeService');
    })); 

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