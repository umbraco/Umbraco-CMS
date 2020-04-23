describe('blockEditorService tests', function () {

    var blockEditorService, contentResource, $rootScope, $scope;

    beforeEach(module('umbraco.services'));
    beforeEach(module('umbraco.resources'));
    beforeEach(module('umbraco.mocks'));
    beforeEach(module('umbraco'));
    
    beforeEach(inject(function ($injector, mocksUtils, _$rootScope_) {

        mocksUtils.disableAuth();

        $rootScope = _$rootScope_;
        $scope = $rootScope.$new();

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
                testproperty: "myTestValue"
            }
        ]
    };

    describe('init blockEditoModelObject', function () {
        
        it('fail if no model value', function () {
            function createWithNoModelValue() {
                blockEditorService.createModelObject(null, "Umbraco.TestBlockEditor", [], $scope);
            }
            expect(createWithNoModelValue).toThrow();
        });

        it('return a object, with methods', function () {
            var modelObject = blockEditorService.createModelObject({}, "Umbraco.TestBlockEditor", [], $scope);

            expect(modelObject).not.toBeUndefined();
            expect(modelObject.loadScaffolding).not.toBeUndefined();
        });

        it('getBlockConfiguration provide the requested block configurtion', function () {
            var modelObject = blockEditorService.createModelObject({}, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope);
            
            expect(modelObject.getBlockConfiguration(blockConfigurationMock.contentTypeAlias).label).toBe(blockConfigurationMock.label);
        });
        
        it('loadScaffolding provides data for itemPicker', function (done) {
            var modelObject = blockEditorService.createModelObject({}, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope);
            
            modelObject.loadScaffolding().then(() => {
                var itemPickerOptions = modelObject.getAvailableBlocksForItemPicker();
                expect(itemPickerOptions.length).toBe(1);
                expect(itemPickerOptions[0].alias).toBe(blockConfigurationMock.contentTypeAlias);
                done();
            });
            
        });
        
        it('getLayoutEntry has values', function (done) {

            
            var modelObject = blockEditorService.createModelObject(propertyModelMock, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope);
            
            modelObject.loadScaffolding().then(() => {
                
                var layout = modelObject.getLayout();

                expect(layout).not.toBeUndefined();
                expect(layout.length).toBe(1);
                expect(layout[0]).toBe(propertyModelMock.layout["Umbraco.TestBlockEditor"][0]);
                expect(layout[0].udi).toBe(propertyModelMock.layout["Umbraco.TestBlockEditor"][0].udi);

                done();
            });
            
        });
        
        it('getBlockModel has values', function (done) {

            
            var modelObject = blockEditorService.createModelObject(propertyModelMock, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope);
            
            modelObject.loadScaffolding().then(() => {
                
                var layout = modelObject.getLayout();

                var blockModel = modelObject.getBlockModel(layout[0]);

                expect(blockModel).not.toBeUndefined();
                expect(blockModel.data.udi).toBe(propertyModelMock.data[0].udi);
                expect(blockModel.content.variants[0].tabs[0].properties[0].value).toBe(propertyModelMock.data[0].testproperty);

                done();
            });
            
        });

        
        it('getBlockModel syncs primative values', function (done) {

            var propertyModel = angular.copy(propertyModelMock);

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope);
            
            modelObject.loadScaffolding().then(() => {
                
                var layout = modelObject.getLayout();

                var blockModel = modelObject.getBlockModel(layout[0]);

                blockModel.content.variants[0].tabs[0].properties[0].value = "anotherTestValue";

                $rootScope.$digest();// invoke angularJS Store.

                expect(blockModel.data).toBe(propertyModel.data[0]);
                expect(blockModel.data.testproperty).toBe("anotherTestValue");
                expect(propertyModel.data[0].testproperty).toBe("anotherTestValue");

                //

                done();
            });
            
        });

        
        it('getBlockModel syncs values of object', function (done) {

            var propertyModel = angular.copy(propertyModelMock);

            var complexValue = {"list": ["A", "B", "C"]};
            propertyModel.data[0].testproperty = complexValue;


            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope);
            
            modelObject.loadScaffolding().then(() => {
                
                var layout = modelObject.getLayout();

                var blockModel = modelObject.getBlockModel(layout[0]);

                blockModel.content.variants[0].tabs[0].properties[0].value.list[0] = "AA";
                blockModel.content.variants[0].tabs[0].properties[0].value.list.push("D");

                $rootScope.$digest();// invoke angularJS Store.

                expect(propertyModel.data[0].testproperty.list[0]).toBe("AA");
                expect(propertyModel.data[0].testproperty.list.length).toBe(4);

                done();
            });
            
        });

        it('layout is referencing layout of propertyModel', function (done) {

            var propertyModel = angular.copy(propertyModelMock);

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope);
            
            modelObject.loadScaffolding().then(() => {
                
                var layout = modelObject.getLayout();

                // remove from layout;
                layout.splice(0, 1);

                expect(propertyModel.layout["Umbraco.TestBlockEditor"].length).toBe(0);
                expect(propertyModel.layout["Umbraco.TestBlockEditor"][0]).toBeUndefined();

                done();
            });
            
        });

        it('removeDataAndDestroyModel removes data', function (done) {

            var propertyModel = angular.copy(propertyModelMock);

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope);
            
            modelObject.loadScaffolding().then(() => {
                
                var layout = modelObject.getLayout();

                var blockModel = modelObject.getBlockModel(layout[0]);

                // remove from layout;
                layout.splice(0, 1);

                // remove from data;
                modelObject.removeDataAndDestroyModel(blockModel);

                expect(propertyModel.data.length).toBe(0);
                expect(propertyModel.data[0]).toBeUndefined();
                expect(propertyModel.layout["Umbraco.TestBlockEditor"].length).toBe(0);
                expect(propertyModel.layout["Umbraco.TestBlockEditor"][0]).toBeUndefined();

                done();
            });
            
        });


  });

});
