(function () {
    describe("Utilities", function () {
        describe("fromJson", function () {
            it("should deserialize json as object", function () {
                expect(Utilities.fromJson('{"a":1,"b":2}')).toEqual({ a: 1, b: 2 });
            });
            it("should return object as object", function () {
                expect(Utilities.fromJson({ a: 1, b: 2 })).toEqual({ a: 1, b: 2 });
            });
        }),
        describe("toJson", function () {
            it("should delegate to JSON.stringify", function () {
                var spy = spyOn(JSON, "stringify").and.callThrough();

                expect(Utilities.toJson({})).toEqual("{}");
                expect(spy).toHaveBeenCalled();
            });

            it("should format objects pretty", function () {
                expect(Utilities.toJson({ a: 1, b: 2 }, true)).toBe(
                    '{\n  "a": 1,\n  "b": 2\n}'
                );
                expect(Utilities.toJson({ a: { b: 2 } }, true)).toBe(
                    '{\n  "a": {\n    "b": 2\n  }\n}'
                );
                expect(Utilities.toJson({ a: 1, b: 2 }, false)).toBe('{"a":1,"b":2}');
                expect(Utilities.toJson({ a: 1, b: 2 }, 0)).toBe('{"a":1,"b":2}');
                expect(Utilities.toJson({ a: 1, b: 2 }, 1)).toBe(
                    '{\n "a": 1,\n "b": 2\n}'
                );
                expect(Utilities.toJson({ a: 1, b: 2 }, {})).toBe(
                    '{\n  "a": 1,\n  "b": 2\n}'
                );
            });

            it("should not serialize properties starting with $$", function () {
                expect(Utilities.toJson({ $$some: "value" }, false)).toEqual("{}");
            });

            it("should serialize properties starting with $", function () {
                expect(Utilities.toJson({ $few: "v" }, false)).toEqual('{"$few":"v"}');
            });

            it("should not serialize $window object", function () {
                expect(Utilities.toJson(window)).toEqual('"$WINDOW"');
            });

            it("should not serialize $document object", function () {
                expect(Utilities.toJson(document)).toEqual('"$DOCUMENT"');
            });

            it("should not serialize scope instances", inject(function (
                $rootScope
            ) {
                expect(Utilities.toJson({ key: $rootScope })).toEqual('{"key":"$SCOPE"}');
            }));

            it("should serialize undefined as undefined", function () {
                expect(Utilities.toJson(undefined)).toEqual(undefined);
            });
        });
    });
})();
