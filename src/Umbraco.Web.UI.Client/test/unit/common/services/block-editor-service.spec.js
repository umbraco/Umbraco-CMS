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

    var simpleBlockConfigurationMock = {contentTypeAlias: "testAlias", label:"Test", settingsElementTypeAlias: null, view: "testview.html"};

    describe('init blockEditoModelObject', function () {
        
        it('fail if no model value', function () {
            function createWithNoModelValue() {
                blockEditorService.createModelObject(null, "test", []);
            }
            expect(createWithNoModelValue).toThrow();
        });

        it('return a object, with methods', function () {
            var modelObject = blockEditorService.createModelObject({}, "test", []);

            expect(modelObject).not.toBeUndefined();
            expect(modelObject.loadScaffolding).not.toBeUndefined();
        });

        it('getBlockConfiguration provide the requested block configurtion', function () {
            var modelObject = blockEditorService.createModelObject({}, "test", [simpleBlockConfigurationMock]);
            
            expect(modelObject.getBlockConfiguration(simpleBlockConfigurationMock.contentTypeAlias).label).toBe(simpleBlockConfigurationMock.label);
        });
        
        it('loadScaffolding provides data for itemPicker', function (done) {
            var modelObject = blockEditorService.createModelObject({}, "test", [simpleBlockConfigurationMock]);
            
            var pendingPromise = modelObject.loadScaffolding().then(() => {
                var itemPickerOptions = modelObject.getAvailableBlocksForItemPicker();
                expect(itemPickerOptions.length).toBe(1);
                expect(itemPickerOptions[0].alias).toBe(simpleBlockConfigurationMock.contentTypeAlias);
                done();
            });
            
        });


  });

});
