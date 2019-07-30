describe("udiParser tests", function () {

    var udiParser;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        udiParser = $injector.get('udiParser');
    }));

    describe("Parse UDI", function () {
        it("can parse a valid document UDI", function() {
            var entityType = "document";
            var key = "c0a62ced-6402-4025-8d46-a234a34f6a56";
            var value = "umb://" + entityType + "/" + key;

            var udi = udiParser.parse(value);

            expect(udi.entityType).toBe(entityType);
            expect(udi.value).toBe(key);
        });

        it("can parse a valid media UDI", function() {
            var entityType = "media";
            var key = "f82f3313-f8e9-42b0-a67f-80a7f452dd21";
            var value = "umb://" + entityType + "/" + key;

            var udi = udiParser.parse(value);

            expect(udi.entityType).toBe(entityType);
            expect(udi.value).toBe(key);
        });

        it("returns the full UDI when calling toString() on the returned value", function() {
            var value = "umb://document/c0a62ced-6402-4025-8d46-a234a34f6a56";

            var udi = udiParser.parse(value);

            expect(udi.toString()).toBe(value);
        });

        it("can parse an open UDI", function() {
            var entityType = "media";
            var value = "umb://" + entityType;

            var udi = udiParser.parse(value);

            expect(udi.entityType).toBe(entityType);
            expect(udi.value).toBeNull();
            expect(udi.toString()).toBe(value);
        });

        it("returns null if the input is invalid", function() {
            var value = "not an UDI";

            var udi = udiParser.parse(value);

            expect(udi).toBeNull();
        });
    });
});
