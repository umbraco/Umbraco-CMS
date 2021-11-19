describe('umbRequestHelper tests', function () {
    var umbRequestHelper;

    beforeEach(module('umbraco'));
    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        umbRequestHelper = $injector.get('umbRequestHelper');

        // set the Umbraco.Sys.ServerVariables.application.applicationPath
        if (!Umbraco) Umbraco = {};
        if (!Umbraco.Sys) Umbraco.Sys = {};
        if (!Umbraco.Sys.ServerVariables) Umbraco.Sys.ServerVariables = {};
        if (!Umbraco.Sys.ServerVariables.application) Umbraco.Sys.ServerVariables.application = {};
        Umbraco.Sys.ServerVariables.application.applicationPath = "/mysite/";
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

    describe('Virtual Paths', function () {

        it('can convert virtual path to absolute url', function () {
            var result = umbRequestHelper.convertVirtualToAbsolutePath("~/App_Plugins/hello/world.css");
            expect(result).toBe("/mysite/App_Plugins/hello/world.css");
        });

        it('can convert absolute path to absolute url', function () {
            var result = umbRequestHelper.convertVirtualToAbsolutePath("/App_Plugins/hello/world.css");
            expect(result).toBe("/App_Plugins/hello/world.css");
        });

        it('throws on invalid virtual path', function () {
            var relativePath = "App_Plugins/hello/world.css";
            expect(function () {
                umbRequestHelper.convertVirtualToAbsolutePath(relativePath);
            }).toThrow("The path " + relativePath + " is not a virtual path");
        });

    });
});
