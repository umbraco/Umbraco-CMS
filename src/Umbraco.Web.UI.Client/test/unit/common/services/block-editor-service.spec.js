describe('blockEditorService tests', function () {

    var blockEditorService, $rootScope, $httpBackend, varaintMocks, contentResource;

    beforeEach(module('umbraco.services'));
    beforeEach(module('umbraco.mocks'));
    
    beforeEach(inject(function ($injector, mocksUtils) {

        mocksUtils.disableAuth();

        blockEditorService = $injector.get('blockEditorService'); 
        $rootScope = $injector.get('$rootScope');
        $httpBackend = $injector.get('$httpBackend');
        varaintMocks = $injector.get("variantContentMocks");
        varaintMocks.register();
        contentResource = $injector.get('contentResource');

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
        
        it('loadScaffolding provides data for itemPicker', function () {
            var modelObject = blockEditorService.createModelObject({}, "test", [simpleBlockConfigurationMock]);
            
            var itemPickerOptions;

            var pendingPromise = modelObject.loadScaffolding(contentResource).then(() => {
                itemPickerOptions = modelObject.getAvailableBlocksForItemPicker();
            });

            $rootScope.$digest();
            $httpBackend.flush();
            
            expect(itemPickerOptions.length).toBe(1);

            
        });

  });

});
