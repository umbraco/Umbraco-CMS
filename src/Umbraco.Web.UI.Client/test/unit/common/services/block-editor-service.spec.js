describe('blockEditorService tests', function () {

    var blockEditorService, contentResource;

    beforeEach(module('umbraco.services'));
    beforeEach(module('umbraco.resources'));
    beforeEach(module('umbraco.mocks'));
    
    beforeEach(inject(function ($injector, mocksUtils) {

        mocksUtils.disableAuth();

        contentResource = $injector.get("contentResource");
        spyOn(contentResource, "getScaffold").and.callFake(
            function () {
                return Promise.resolve(mocksUtils.getMockVariantContent(1234))
            }
        );

        blockEditorService = $injector.get('blockEditorService');

    }));


    var blockConfigurationMock = {contentTypeAlias: "testAlias", label:"Test label", settingsElementTypeAlias: null, view: "testview.html"};

    var propertyModelMock = {
        layout: {
            "Umbraco.TestBlockEditor": [
                {
                    udi: 1234
                }
            ]
        },
        data: [
            {
                udi: 1234,
                contentTypeAlias: "testAlias",
                testvalue: "myTestValue"
            }
        ]
    };

    describe('init blockEditoModelObject', function () {
        
        it('fail if no model value', function () {
            function createWithNoModelValue() {
                blockEditorService.createModelObject(null, "Umbraco.TestBlockEditor", []);
            }
            expect(createWithNoModelValue).toThrow();
        });

        it('return a object, with methods', function () {
            var modelObject = blockEditorService.createModelObject({}, "Umbraco.TestBlockEditor", []);

            expect(modelObject).not.toBeUndefined();
            expect(modelObject.loadScaffolding).not.toBeUndefined();
        });

        it('getBlockConfiguration provide the requested block configurtion', function () {
            var modelObject = blockEditorService.createModelObject({}, "Umbraco.TestBlockEditor", [blockConfigurationMock]);
            
            expect(modelObject.getBlockConfiguration(blockConfigurationMock.contentTypeAlias).label).toBe(blockConfigurationMock.label);
        });
        
        it('loadScaffolding provides data for itemPicker', function (done) {
            var modelObject = blockEditorService.createModelObject({}, "Umbraco.TestBlockEditor", [blockConfigurationMock]);
            
            modelObject.loadScaffolding().then(() => {
                var itemPickerOptions = modelObject.getAvailableBlocksForItemPicker();
                expect(itemPickerOptions.length).toBe(1);
                expect(itemPickerOptions[0].alias).toBe(blockConfigurationMock.contentTypeAlias);
                done();
            });
            
        });
        
        it('getLayoutEntry has right values', function (done) {

            
            var modelObject = blockEditorService.createModelObject(propertyModelMock, "Umbraco.TestBlockEditor", [blockConfigurationMock]);
            
            modelObject.loadScaffolding().then(() => {
                
                var layout = modelObject.getLayout();

                expect(layout).not.toBeUndefined();
                expect(layout.length).toBe(1);
                expect(layout[0]).toBe(propertyModelMock.layout["Umbraco.TestBlockEditor"][0]);
                expect(layout[0].udi).toBe(propertyModelMock.layout["Umbraco.TestBlockEditor"][0].udi);

                done();
            });
            
        });
        
        it('getBlockModel provide value', function (done) {

            
            var modelObject = blockEditorService.createModelObject(propertyModelMock, "Umbraco.TestBlockEditor", [blockConfigurationMock]);
            
            modelObject.loadScaffolding().then(() => {
                
                var layout = modelObject.getLayout();
                expect(layout).not.toBeUndefined();

                var blockModel = modelObject.getBlockModel(layout[0]);

                expect(blockModel).not.toBeUndefined();
                expect(blockModel[0].udi).toBe(propertyModelMock.data[0].udi);
                expect(blockModel[0].testvalue).toBe(propertyModelMock.data[0].testvalue);

                done();
            });
            
        });


  });

});
