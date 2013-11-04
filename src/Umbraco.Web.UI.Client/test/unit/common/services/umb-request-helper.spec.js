describe('umbRequestHelper tests', function () {
    var umbRequestHelper;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        umbRequestHelper = $injector.get('umbRequestHelper');
    }));

    describe('formatting Urls', function () {

        it('can create a query string from name value pairs', function () {

            expect(umbRequestHelper.dictionaryToQueryString([
                { key1: "value1" },
                { key2: "value2" },
                { key3: "value3" },
                { trying: "?to&hack=you" },
                { "?trying=to&hack": "you" }
            ])).toBe(
            "key1=value1" +
            "&key2=value2" +
            "&key3=value3" +
            "&trying=%3Fto%26hack%3Dyou" +
            "&%3Ftrying%3Dto%26hack=you"
            );
        });

        it('can create a url based on server vars', function () {

            expect(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "GetAllowedChildren", [{ contentId: 123 }])).toBe(
                    "/umbraco/Api/ContentType/GetAllowedChildren?contentId=123");
        });

    });
});