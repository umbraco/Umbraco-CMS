describe('umbDataFormatter service tests', function () {
    var umbDataFormatter;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        umbDataFormatter = $injector.get('umbDataFormatter');
    }));

    describe('formatting GET content data', function () {

        it('will set the same invariant property instance reference between all variants', function () {

            var model = {
                variants: [
                    {
                        language: { culture: "en-US" },
                        tabs: [
                            {
                                properties: [
                                    { alias: "test1", culture: null, value: "test1" },
                                    { alias: "test2", culture: "en-US", value: "test2" }
                                ]
                            },
                            {
                                properties: [
                                    { alias: "test3", culture: "en-US", value: "test3" },
                                    { alias: "test4", culture: null, value: "test4" }
                                ]
                            }
                        ]

                    },
                    {
                        language: { culture: "es-ES" },
                        tabs: [
                            {
                                properties: [
                                    { alias: "test1", culture: null, value: "test5" },
                                    { alias: "test2", culture: "en-US", value: "test6" }
                                ]
                            },
                            {
                                properties: [
                                    { alias: "test3", culture: "en-US", value: "test7" },
                                    { alias: "test4", culture: null, value: "test8" }
                                ]
                            }
                        ]
                    },
                    {
                        language: { culture: "fr-FR" },
                        tabs: [
                            {
                                properties: [
                                    { alias: "test1", culture: null, value: "test9" },
                                    { alias: "test2", culture: "en-US", value: "test10" }
                                ]
                            },
                            {
                                properties: [
                                    { alias: "test3", culture: "en-US", value: "test11" },
                                    { alias: "test4", culture: null, value: "test12" }
                                ]
                            }
                        ]
                    }
                ]
            };
            var result = umbDataFormatter.formatContentGetData(model);

            //make sure the same property reference exists for property 0 and 3 for each variant
            for (var i = 1; i < result.variants.length; i++) {
                expect(result.variants[0].tabs[0].properties[0]).toBe(result.variants[i].tabs[0].properties[0]);
                expect(result.variants[0].tabs[1].properties[3]).toBe(result.variants[i].tabs[1].properties[3]);
            }

            //test that changing a property value in one variant is definitely updating the same object reference and therefor
            //is done on all variants.
            result.variants[0].tabs[0].properties[0].value = "hello";
            for (var i = 1; i < result.variants.length; i++) {
                expect(result.variants[0].tabs[0].properties[0].value).toBe("hello");
            }

        });

    });
});
