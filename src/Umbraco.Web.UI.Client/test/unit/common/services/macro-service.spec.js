describe('macro service tests', function () {
    var macroService;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        macroService = $injector.get('macroService');
    }));

    describe('generates macro syntax', function() {

        it('can parse syntax for macros', function () {

            var result = macroService.parseMacroSyntax("<?UMBRACO_MACRO macroAlias='Map' test=\"asdf\" test2='hello' />");


            expect(result).not.toBeNull();
            expect(result.macroAlias).toBe("Map");
            expect(result.macroParamsDictionary).not.toBeUndefined();

            expect(result.macroParamsDictionary.test).not.toBeUndefined();
            expect(result.macroParamsDictionary.test).toBe("asdf");

            expect(result.macroParamsDictionary.test2).not.toBeUndefined();
            expect(result.macroParamsDictionary.test2).toBe("hello");


        });

        it('can parse syntax for macros with aliases containing dots', function () {

            var result = macroService.parseMacroSyntax("<?UMBRACO_MACRO macroAlias='Map.Test' test=\"asdf\" test2='hello' />");

            expect(result).not.toBeNull();
            expect(result.macroAlias).toBe("Map.Test");
            expect(result.macroParamsDictionary.test).not.toBeUndefined();
            expect(result.macroParamsDictionary.test).toBe("asdf");
            expect(result.macroParamsDictionary.test2).not.toBeUndefined();
            expect(result.macroParamsDictionary.test2).toBe("hello");
        });

        it('can parse syntax for macros when macroAlias is not the first parameter', function () {

            var result = macroService.parseMacroSyntax("<?UMBRACO_MACRO test=\"asdf\" test2='hello' macroAlias='Map.Test' />");

            expect(result).not.toBeNull();
            expect(result.macroAlias).toBe("Map.Test");
            expect(result.macroParamsDictionary.test).not.toBeUndefined();
            expect(result.macroParamsDictionary.test).toBe("asdf");
            expect(result.macroParamsDictionary.test2).not.toBeUndefined();
            expect(result.macroParamsDictionary.test2).toBe("hello");
        });

        it('can parse syntax for macros with aliases containing whitespace and other chars', function () {

            var result = macroService.parseMacroSyntax("<?UMBRACO_MACRO macroAlias='Map Test [Hello\\World]' test=\"asdf\" test2='hello' />");

            expect(result).not.toBeNull();
            expect(result.macroAlias).toBe("Map Test [Hello\\World]");
            expect(result.macroParamsDictionary.test).not.toBeUndefined();
            expect(result.macroParamsDictionary.test).toBe("asdf");
            expect(result.macroParamsDictionary.test2).not.toBeUndefined();
            expect(result.macroParamsDictionary.test2).toBe("hello");
        });

        it('can parse syntax for macros with body', function () {

            var result = macroService.parseMacroSyntax("<?UMBRACO_MACRO macroAlias='Map' test1=\"asdf\" test2='hello' ><img src='blah.jpg'/></?UMBRACO_MACRO>");

            expect(result).not.toBeNull();
            expect(result.macroAlias).toBe("Map");
            expect(result.macroParamsDictionary.test1).not.toBeUndefined();
            expect(result.macroParamsDictionary.test1).toBe("asdf");
            expect(result.macroParamsDictionary.test2).not.toBeUndefined();
            expect(result.macroParamsDictionary.test2).toBe("hello");


        });

    });

    describe('generates macro syntax', function () {

        it('can generate syntax for macros', function () {

            var syntax = macroService.generateMacroSyntax({
                macroAlias: "myMacro",
                macroParamsDictionary: {
                    param1: "value1",
                    param2: "value2",
                    param3: "value3"
                }
            });

            expect(syntax).
                toBe("<?UMBRACO_MACRO macroAlias=\"myMacro\" param1=\"value1\" param2=\"value2\" param3=\"value3\" />");

        });

        it('can generate syntax for macros with no params', function () {

            var syntax = macroService.generateMacroSyntax({
                macroAlias: "myMacro",
                macroParamsDictionary: {}
            });

            expect(syntax).
                toBe("<?UMBRACO_MACRO macroAlias=\"myMacro\" />");

        });

        it('can generate syntax for MVC', function () {

            var syntax = macroService.generateMvcSyntax({
                macroAlias: "myMacro",
                macroParamsDictionary: {
                    param1: "value1",
                    param2: "value2",
                    param3: "value3"
                }
            });

            expect(syntax).
                toBe("@await Umbraco.RenderMacroAsync(\"myMacro\", new {param1=\"value1\", param2=\"value2\", param3=\"value3\"})");


        });

        it('can generate syntax for MVC with no params', function () {

            var syntax = macroService.generateMvcSyntax({
                macroAlias: "myMacro",
                macroParamsDictionary: {}
            });

            expect(syntax).
                toBe("@await Umbraco.RenderMacroAsync(\"myMacro\")");

        });

    });
});
