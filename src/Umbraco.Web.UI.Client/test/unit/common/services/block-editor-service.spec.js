describe('blockEditorService tests', function () {

    var key = "6A1F5BDD-67EF-4173-B061-D6348ED07094";
    var udi = "umb://element/6A1F5BDD67EF4173B061D6348ED07094";

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
        spyOn(contentResource, "getScaffoldByKey").and.callFake(
            function () {
                return Promise.resolve(mocksUtils.getMockVariantContent(1234, key, udi))
            }
        );

        blockEditorService = $injector.get('blockEditorService');

    }));


    var blockConfigurationMock = { contentTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB", label: "Test label", settingsElementTypeKey: null, view: "testview.html" };

    var propertyModelMock = {
        layout: {
            "Umbraco.TestBlockEditor": [
                {
                    udi: udi
                }
            ]
        },
        contentData: [
            {
                udi: 1234,
                contentTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB",
                testproperty: "myTestValue"
            }
        ]
    };

    var blockWithSettingsConfigurationMock = { contentTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB", label: "Test label", settingsElementTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB", view: "testview.html" };
    var propertyModelWithSettingsMock = {
        layout: {
            "Umbraco.TestBlockEditor": [
                {
                    udi: 1234,
                    settingsUdi: 4567
                }
            ]
        },
        contentData: [
            {
                udi: udi,
                contentTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB",
                testproperty: "myTestValue"
            }
        ],
        settingsData: [
            {
                udi: 4567,
                contentTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB",
                testproperty: "myTestValueForSettings"
            }
        ]
    };

    describe('init blockEditorModelObject', function () {

        it('fail if no model value', function () {
            function createWithNoModelValue() {
                blockEditorService.createModelObject(null, "Umbraco.TestBlockEditor", [], $scope, $scope);
            }
            expect(createWithNoModelValue).toThrow();
        });

        it('return a object, with methods', function () {
            var modelObject = blockEditorService.createModelObject({}, "Umbraco.TestBlockEditor", [], $scope, $scope);

            expect(modelObject).not.toBeUndefined();
            expect(modelObject.load).not.toBeUndefined();
        });

        it('getBlockConfiguration provide the requested block configurtion', function () {
            var modelObject = blockEditorService.createModelObject({}, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            expect(modelObject.getBlockConfiguration(blockConfigurationMock.contentTypeKey).label).toBe(blockConfigurationMock.label);
        });

        it('load provides data for itemPicker', function (done) {
            var modelObject = blockEditorService.createModelObject({}, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {
                var itemPickerOptions = modelObject.getAvailableBlocksForBlockPicker();
                expect(itemPickerOptions.length).toBe(1);
                expect(itemPickerOptions[0].blockConfigModel.contentTypeKey).toBe(blockConfigurationMock.contentTypeKey);
                done();
            });

        });

        it('getLayoutEntry has values', function (done) {


            var modelObject = blockEditorService.createModelObject(propertyModelMock, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                var layout = modelObject.getLayout();

                expect(layout).not.toBeUndefined();
                expect(layout.length).toBe(1);
                expect(layout[0]).toBe(propertyModelMock.layout["Umbraco.TestBlockEditor"][0]);
                expect(layout[0].udi).toBe(propertyModelMock.layout["Umbraco.TestBlockEditor"][0].udi);

                done();
            });

        });

        it('getBlockObject has values', function (done) {


            var modelObject = blockEditorService.createModelObject(propertyModelMock, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                try {
                    var layout = modelObject.getLayout();

                    var blockObject = modelObject.getBlockObject(layout[0]);

                    expect(blockObject).not.toBeUndefined();
                    expect(blockObject.data.udi).toBe(propertyModelMock.contentData[0].udi);
                    expect(blockObject.content.variants[0].tabs[0].properties[0].value).toBe(propertyModelMock.contentData[0].testproperty);

                    done();
                } catch (e) {
                    done.fail(e);
                }
            });

        });


        it('getBlockObject syncs primitive values', function (done) {

            var propertyModel = angular.copy(propertyModelMock);

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                try {
                    var layout = modelObject.getLayout();

                    var blockObject = modelObject.getBlockObject(layout[0]);

                    blockObject.content.variants[0].tabs[0].properties[0].value = "anotherTestValue";

                    $rootScope.$digest();// invoke angularJS Store.

                    expect(blockObject.data).toEqual(propertyModel.contentData[0]);
                    expect(blockObject.data.testproperty).toBe("anotherTestValue");
                    expect(propertyModel.contentData[0].testproperty).toBe("anotherTestValue");

                    done();
                } catch (e) {
                    done.fail(e);
                }
            });

        });


        it('getBlockObject syncs values of object', function (done) {

            var propertyModel = angular.copy(propertyModelMock);

            var complexValue = { "list": ["A", "B", "C"] };
            propertyModel.contentData[0].testproperty = complexValue;


            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                try {
                    var layout = modelObject.getLayout();

                    var blockObject = modelObject.getBlockObject(layout[0]);

                    blockObject.content.variants[0].tabs[0].properties[0].value.list[0] = "AA";
                    blockObject.content.variants[0].tabs[0].properties[0].value.list.push("D");

                    $rootScope.$digest();// invoke angularJS Store.

                    expect(propertyModel.contentData[0].testproperty.list[0]).toBe("AA");
                    expect(propertyModel.contentData[0].testproperty.list.length).toBe(4);

                    done();
                } catch (e) {
                    done.fail(e);
                }
            });

        });

        it('layout is referencing layout of propertyModel', function (done) {

            var propertyModel = angular.copy(propertyModelMock);

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

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

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                try {
                    var layout = modelObject.getLayout();

                    var blockObject = modelObject.getBlockObject(layout[0]);

                    expect(blockObject).not.toBeUndefined();
                    expect(blockObject).not.toBe(null);

                    // remove from layout;
                    layout.splice(0, 1);

                    // remove from data;
                    modelObject.removeDataAndDestroyModel(blockObject);

                    expect(propertyModel.contentData.length).toBe(0);
                    expect(propertyModel.contentData[0]).toBeUndefined();
                    expect(propertyModel.layout["Umbraco.TestBlockEditor"].length).toBe(0);
                    expect(propertyModel.layout["Umbraco.TestBlockEditor"][0]).toBeUndefined();

                    done();
                } catch (e) {
                    done.fail(e);
                }
            });
        });

        it('getBlockObject of block with settings has values', function (done) {

            var propertyModel = angular.copy(propertyModelWithSettingsMock);

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockWithSettingsConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                var layout = modelObject.getLayout();

                var blockObject = modelObject.getBlockObject(layout[0]);

                expect(blockObject).not.toBeUndefined();
                expect(blockObject.data.udi).toBe(propertyModel.contentData[0].udi);
                expect(blockObject.content.variants[0].tabs[0].properties[0].value).toBe(propertyModel.contentData[0].testproperty);

                done();
            });

        });


        it('getBlockObject of block with settings syncs primative values', function (done) {

            var propertyModel = angular.copy(propertyModelWithSettingsMock);

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockWithSettingsConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                var layout = modelObject.getLayout();

                var blockObject = modelObject.getBlockObject(layout[0]);

                blockObject.content.variants[0].tabs[0].properties[0].value = "anotherTestValue";
                blockObject.settings.variants[0].tabs[0].properties[0].value = "anotherTestValueForSettings";

                $rootScope.$digest();// invoke angularJS Store.

                expect(blockObject.data).toEqual(propertyModel.contentData[0]);
                expect(blockObject.data.testproperty).toBe("anotherTestValue");
                expect(propertyModel.contentData[0].testproperty).toBe("anotherTestValue");

                expect(blockObject.settingsData).toEqual(propertyModel.settingsData[0]);
                expect(blockObject.settingsData.testproperty).toBe("anotherTestValueForSettings");
                expect(propertyModel.settingsData[0].testproperty).toBe("anotherTestValueForSettings");

                //

                done();
            });

        });


        it('getBlockObject of block with settings syncs values of object', function (done) {

            var propertyModel = angular.copy(propertyModelWithSettingsMock);

            var complexValue = { "list": ["A", "B", "C"] };
            propertyModel.contentData[0].testproperty = complexValue;

            var complexSettingsValue = { "list": ["A", "B", "C"] };
            propertyModel.settingsData[0].testproperty = complexSettingsValue;

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockWithSettingsConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                var layout = modelObject.getLayout();

                var blockObject = modelObject.getBlockObject(layout[0]);

                blockObject.content.variants[0].tabs[0].properties[0].value.list[0] = "AA";
                blockObject.content.variants[0].tabs[0].properties[0].value.list.push("D");

                blockObject.settings.variants[0].tabs[0].properties[0].value.list[0] = "settingsValue";
                blockObject.settings.variants[0].tabs[0].properties[0].value.list.push("settingsNewValue");

                $rootScope.$digest();// invoke angularJS Store.

                expect(propertyModel.contentData[0].testproperty.list[0]).toBe("AA");
                expect(propertyModel.contentData[0].testproperty.list.length).toBe(4);

                expect(propertyModel.settingsData[0].testproperty.list[0]).toBe("settingsValue");
                expect(propertyModel.settingsData[0].testproperty.list.length).toBe(4);

                done();
            } catch (e) {
                done.fail(e);
            }
        });

    });


});

});
