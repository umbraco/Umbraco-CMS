describe('model mapper tests', function () {
    var umbModelMapper;
    
    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        umbModelMapper = $injector.get('umbModelMapper');        
    }));

    describe('maps basic entity', function () {

        it('can map content object', function () {
            var content = {
                id: "1",
                name: "test",
                icon: "icon",
                key: "some key",
                parentId: "-1",
                alias: "test alias",
                path: "-1,1",
                metaData: {hello:"world"},
                publishDate: null,
                releaseDate: null,
                removeDate: false,
                template: "test",
                urls: [],
                allowedActions: [],
                contentTypeName: null,
                notifications: [],
                ModelState: {},
                tabs: [],
                properties: [],                
            };

            var mapped = umbModelMapper.convertToEntityBasic(content);
            var keys = _.keys(mapped);

            expect(keys.length).toBe(8);
            expect(mapped.id).toBe("1");
            expect(mapped.name).toBe("test");
            expect(mapped.icon).toBe("icon");
            expect(mapped.key).toBe("some key");
            expect(mapped.parentId).toBe("-1");
            expect(mapped.alias).toBe("test alias");
            expect(mapped.metaData.hello).toBe("world");
        });
        
        it('throws when info is missing', function () {
            var content = {
                id: "1",
                //name: "test", //removed
                icon: "icon",
                key: "some key",
                parentId: "-1",
                alias: "test alias",
                path: "-1,1",
                metaData: { hello: "world" },
                publishDate: null,
                releaseDate: null,
                removeDate: false,
                template: "test",
                urls: [],
                allowedActions: [],
                contentTypeName: null,
                notifications: [],
                ModelState: {},
                tabs: [],
                properties: [],
            };

            function doMap() {
                umbModelMapper.convertToEntityBasic(content);
            }
           
            expect(doMap).toThrow();
        });
        
        it('can map the minimum props', function () {
            var content = {
                id: "1",
                name: "test",
                icon: "icon",
                parentId: "-1",
                path: "-1,1"          
            };

            var mapped = umbModelMapper.convertToEntityBasic(content);
            var keys = _.keys(mapped);

            expect(keys.length).toBe(5);
            expect(mapped.id).toBe("1");
            expect(mapped.name).toBe("test");
            expect(mapped.icon).toBe("icon");
            expect(mapped.parentId).toBe("-1");
        });
        
    });
    
});