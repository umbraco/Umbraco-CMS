describe('umbRequestHelper tests', function () {
    var umbRequestHelper;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        umbRequestHelper = $injector.get('umbRequestHelper');
    }));

    describe('formatting Urls', function () {

        it('can create a query string from name value pairs', function () {
            
            expect(umbRequestHelper.dictionaryToQueryString(
                [{ key1: "value1" }, { key2: "value2" }, { key3: "value3" }])).toBe(
                    "key1=value1&key2=value2&key3=value3");            
        });
        
        it('can create a url based on server vars', function () {

            expect(umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "GetAllowedChildren", [{contentId: 123}])).toBe(
                    "/umbraco/Api/ContentType/GetAllowedChildren?contentId=123");
        });
        
    });
});