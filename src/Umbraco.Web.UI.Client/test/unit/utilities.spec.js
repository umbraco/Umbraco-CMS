(function () {
    describe('Utilities', function () {
        var dateA = new Date('December 25, 1995 03:24:00');
        var dateB = new Date('1995-12-25T03:24:00');
        var dateC = new Date('2020-11-26T15:21:10');
        
        describe('equals', function () {
            it('should be true for identical objects', function () {
                expect(Utilities.equals('john', 'john')).toEqual(true);
                expect(Utilities.equals(['john', 'doe'], ['john', 'doe'])).toEqual(true);
                expect(Utilities.equals(dateA, dateA)).toEqual(true);
                expect(Utilities.equals(['john', 'doe', [0, 1]], ['john', 'doe', [0, 1]])).toEqual(true);
            });
            it('should be false for none-identical objects of the same type', function () {
                expect(Utilities.equals('john', 'doe')).toEqual(false);
                expect(Utilities.equals(['john', 'doe'], ['jane', 'doe'])).toEqual(false);
                expect(Utilities.equals(dateB, dateC)).toEqual(false);
            });

            it('should be false for different types of objects', function () {
                expect(Utilities.equals(dateA, ['john', 'doe'])).toEqual(false);
                expect(Utilities.equals(1, ['john', 'doe'])).toEqual(false);
                expect(Utilities.equals(false, ['john', 'doe'])).toEqual(false);
            });
        });

        describe('isDate', function () {

            it('should be true if the object is a Date', function () {
                expect(angular.isDate(dateA)).toEqual(true);
                expect(Utilities.isDate(dateA)).toEqual(true);
            });

            it('should be false if the object is not a Date', function () {
                expect(Utilities.isDate('Not a date')).toEqual(false);
            });
        });

        describe('isRegExp', function () {
            var regex = new RegExp('');
            it('should be true if the object is a RegExp', function () {
                expect(Utilities.isRegExp(regex)).toEqual(true);
            });

            it('should be false if the object is not a RegExp', function () {
                expect(Utilities.isRegExp('Not a RegExp')).toEqual(false);
            });
        });


        describe('toJson', function () {
            it('should delegate to JSON.stringify', function () {
                var spy = spyOn(JSON, 'stringify').and.callThrough();

                expect(Utilities.toJson({})).toEqual('{}');
                expect(spy).toHaveBeenCalled();
            });

            it('should format objects pretty', function () {
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

            it('should not serialize properties starting with $$', function () {
                expect(Utilities.toJson({ $$some: 'value' }, false)).toEqual('{}');
            });

            it('should serialize properties starting with $', function () {
                expect(Utilities.toJson({ $few: 'v' }, false)).toEqual('{"$few":"v"}');
            });

            it('should not serialize $window object', function () {
                expect(Utilities.toJson(window)).toEqual('"$WINDOW"');
            });

            it('should not serialize $document object', function () {
                expect(Utilities.toJson(document)).toEqual('"$DOCUMENT"');
            });

            it('should not serialize scope instances', inject(function (
                $rootScope
            ) {
                expect(Utilities.toJson({ key: $rootScope })).toEqual('{"key":"$SCOPE"}');
            }));

            it('should serialize undefined as undefined', function () {
                expect(Utilities.toJson(undefined)).toEqual(undefined);
            });
        });
    });
})();
