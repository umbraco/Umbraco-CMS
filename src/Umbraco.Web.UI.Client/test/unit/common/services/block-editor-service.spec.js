describe('blockEditorService tests', function () {

    var contentKey = "6A1F5BDD-67EF-4173-B061-D6348ED07094";
    var contentUdi = "umb://element/6A1F5BDD67EF4173B061D6348ED07094";
    var settingsKey = "2AF42343-C8A2-400D-BA43-4818C2B3CDC5";
    var settingsUdi = "umb://element/2AF42343C8A2400DBA434818C2B3CDC5";

    var blockEditorService, contentResource, $rootScope, $scope, $q, localizationService, $timeout;

    beforeEach(module('umbraco.services'));
    beforeEach(module('umbraco.resources'));
    beforeEach(module('umbraco.mocks'));
    beforeEach(module('umbraco'));

    beforeEach(inject(function ($injector, mocksUtils, _$rootScope_, _$q_, _$timeout_) {

        mocksUtils.disableAuth();

        $rootScope = _$rootScope_;
        $scope = $rootScope.$new();
        $q = _$q_;
        $timeout = _$timeout_;

        contentResource = $injector.get("contentResource");
        spyOn(contentResource, "getScaffoldByKeys").and.callFake(
            function () {
                var scaffold = mocksUtils.getMockVariantContent(1234, contentKey, contentUdi);
                return $q.resolve([scaffold]);
            }
        );
        // this seems to be required because of the poor promise implementation in localizationService (see TODO in that service)
        localizationService = $injector.get("localizationService");
        spyOn(localizationService, "localize").and.callFake(
            function () {
                return $q.resolve("Localized test text");
            }
        );

        blockEditorService = $injector.get('blockEditorService');

    }));


    var blockConfigurationMock = { contentElementTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB", label: "Test label", settingsElementTypeKey: null, view: "/testview.html" };

    var propertyModelMock = {
        layout: {
            "Umbraco.TestBlockEditor": [
                {
                    contentUdi: contentUdi
                }
            ]
        },
        contentData: [
            {
                udi: contentUdi,
                contentTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB",
                testproperty: "myTestValue"
            }
        ]
    };

    var blockWithSettingsConfigurationMock = { contentElementTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB", label: "Test label", settingsElementTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB", view: "/testview.html" };
    var propertyModelWithSettingsMock = {
        layout: {
            "Umbraco.TestBlockEditor": [
                {
                    contentUdi: contentUdi,
                    settingsUdi: settingsUdi
                }
            ]
        },
        contentData: [
            {
                udi: contentUdi,
                contentTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB",
                testproperty: "myTestValue"
            }
        ],
        settingsData: [
            {
                udi: settingsUdi,
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

            expect(modelObject.getBlockConfiguration(blockConfigurationMock.contentElementTypeKey).label).toBe(blockConfigurationMock.label);
        });

        it('load provides data for itemPicker', function (done) {
            var modelObject = blockEditorService.createModelObject({}, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {
                try {
                    var itemPickerOptions = modelObject.getAvailableBlocksForBlockPicker();
                    expect(itemPickerOptions.length).toBe(1);
                    expect(itemPickerOptions[0].blockConfigModel.contentElementTypeKey).toBe(blockConfigurationMock.contentElementTypeKey);
                    done();
                } catch (e) {
                    done.fail(e);
                }
            });

            $rootScope.$digest();
            $timeout.flush();
        });

        it('getLayoutEntry has values', function (done) {


            var modelObject = blockEditorService.createModelObject(propertyModelMock, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                try {
                    var layout = modelObject.getLayout();

                    expect(layout).not.toBeUndefined();
                    expect(layout.length).toBe(1);
                    expect(layout[0]).toBe(propertyModelMock.layout["Umbraco.TestBlockEditor"][0]);
                    expect(layout[0].contentUdi).toBe(propertyModelMock.layout["Umbraco.TestBlockEditor"][0].contentUdi);

                    done();
                } catch (e) {
                    done.fail(e);
                }
            });

            $rootScope.$digest();
            $timeout.flush();
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

            $rootScope.$digest();
            $timeout.flush();
        });


        it('getBlockObject syncs primitive values', function (done) {

            var propertyModel = angular.copy(propertyModelMock);

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                try {
                    var layout = modelObject.getLayout();

                    var blockObject = modelObject.getBlockObject(layout[0]);

                    blockObject.content.variants[0].tabs[0].properties[0].value = "anotherTestValue";

                    // invoke angularJS Store.
                    $timeout(function () {
                        expect(blockObject.data).toEqual(propertyModel.contentData[0]);
                        expect(blockObject.data.testproperty).toBe("anotherTestValue");
                        expect(propertyModel.contentData[0].testproperty).toBe("anotherTestValue");

                        done();
                    });

                } catch (e) {
                    done.fail(e);
                }
            });

            $rootScope.$digest();
            $timeout.flush();

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

                    // invoke angularJS Store.
                    $timeout(function () {
                        expect(propertyModel.contentData[0].testproperty.list[0]).toBe("AA");
                        expect(propertyModel.contentData[0].testproperty.list.length).toBe(4);

                        done();
                    });


                } catch (e) {
                    done.fail(e);
                }
            });

            $rootScope.$digest();
            $timeout.flush();
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

            $rootScope.$digest();
            $timeout.flush();
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

            $rootScope.$digest();
            $timeout.flush();
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

            $rootScope.$digest();
            $timeout.flush();
        });


        it('getBlockObject of block with settings syncs primative values', function (done) {

            var propertyModel = angular.copy(propertyModelWithSettingsMock);

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockWithSettingsConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                var layout = modelObject.getLayout();

                var blockObject = modelObject.getBlockObject(layout[0]);

                blockObject.content.variants[0].tabs[0].properties[0].value = "anotherTestValue";
                blockObject.settings.variants[0].tabs[0].properties[0].value = "anotherTestValueForSettings";

                // invoke angularJS Store.
                $timeout(function () {
                    expect(blockObject.data).toEqual(propertyModel.contentData[0]);
                    expect(blockObject.data.testproperty).toBe("anotherTestValue");
                    expect(propertyModel.contentData[0].testproperty).toBe("anotherTestValue");

                    expect(blockObject.settingsData).toEqual(propertyModel.settingsData[0]);
                    expect(blockObject.settingsData.testproperty).toBe("anotherTestValueForSettings");
                    expect(propertyModel.settingsData[0].testproperty).toBe("anotherTestValueForSettings");

                    done();
                });

            });

            $rootScope.$digest();
            $timeout.flush();
        });


        it('getBlockObject of block with settings syncs values of object', function (done) {

            var propertyModel = angular.copy(propertyModelWithSettingsMock);

            var complexValue = { "list": ["A", "B", "C"] };
            propertyModel.contentData[0].testproperty = complexValue;

            var complexSettingsValue = { "list": ["A", "B", "C"] };
            propertyModel.settingsData[0].testproperty = complexSettingsValue;

            var modelObject = blockEditorService.createModelObject(propertyModel, "Umbraco.TestBlockEditor", [blockWithSettingsConfigurationMock], $scope, $scope);

            modelObject.load().then(() => {

                try {
                    var layout = modelObject.getLayout();

                    var blockObject = modelObject.getBlockObject(layout[0]);

                    blockObject.content.variants[0].tabs[0].properties[0].value.list[0] = "AA";
                    blockObject.content.variants[0].tabs[0].properties[0].value.list.push("D");

                    blockObject.settings.variants[0].tabs[0].properties[0].value.list[0] = "settingsValue";
                    blockObject.settings.variants[0].tabs[0].properties[0].value.list.push("settingsNewValue");

                    // invoke angularJS Store.
                    $timeout(function () {
                        expect(propertyModel.contentData[0].testproperty.list[0]).toBe("AA");
                        expect(propertyModel.contentData[0].testproperty.list.length).toBe(4);

                        expect(propertyModel.settingsData[0].testproperty.list[0]).toBe("settingsValue");
                        expect(propertyModel.settingsData[0].testproperty.list.length).toBe(4);

                        done();
                    });


                } catch (e) {
                    done.fail(e);
                }
            });

            $rootScope.$digest();
            $timeout.flush();
        });


    });

});
