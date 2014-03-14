describe('macro service tests', function () {
    var macroService;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        macroService = $injector.get('macroService');
    }));

    describe('generates macro syntax', function() {

        it('can parse syntax for macros', function () {

            var result = macroService.parseMacroSyntax("<?UMBRACO_MACRO macroAlias='Map' test1=\"asdf\" test2='hello' />");

            expect(result).not.toBeNull();
            expect(result.macroAlias).toBe("Map");
            expect(result.marcoParamsDictionary.test1).not.toBeUndefined();
            expect(result.marcoParamsDictionary.test1).toBe("asdf");
            expect(result.marcoParamsDictionary.test2).not.toBeUndefined();
            expect(result.marcoParamsDictionary.test2).toBe("hello");


        });

        it('can parse syntax for macros with aliases containing dots', function () {

            var result = macroService.parseMacroSyntax("<?UMBRACO_MACRO macroAlias='Map.Test' test1=\"asdf\" test2='hello' />");

            expect(result).not.toBeNull();
            expect(result.macroAlias).toBe("Map.Test");
            expect(result.marcoParamsDictionary.test1).not.toBeUndefined();
            expect(result.marcoParamsDictionary.test1).toBe("asdf");
            expect(result.marcoParamsDictionary.test2).not.toBeUndefined();
            expect(result.marcoParamsDictionary.test2).toBe("hello");


        });
        
        it('can parse syntax for macros with body', function () {

            var result = macroService.parseMacroSyntax("<?UMBRACO_MACRO macroAlias='Map' test1=\"asdf\" test2='hello' ><img src='blah.jpg'/></?UMBRACO_MACRO>");

            expect(result).not.toBeNull();
            expect(result.macroAlias).toBe("Map");
            expect(result.marcoParamsDictionary.test1).not.toBeUndefined();
            expect(result.marcoParamsDictionary.test1).toBe("asdf");
            expect(result.marcoParamsDictionary.test2).not.toBeUndefined();
            expect(result.marcoParamsDictionary.test2).toBe("hello");


        });

    });

    describe('generates macro syntax', function () {

        it('can generate syntax for macros', function () {

            var syntax = macroService.generateMacroSyntax({
                macroAlias: "myMacro",
                marcoParamsDictionary: {
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
                marcoParamsDictionary: {}
            });

            expect(syntax).
                toBe("<?UMBRACO_MACRO macroAlias=\"myMacro\" />");

        });

        it('can generate syntax for webforms', function () {

            var syntax = macroService.generateWebFormsSyntax({
                macroAlias: "myMacro",
                marcoParamsDictionary: {
                    param1: "value1",
                    param2: "value2",
                    param3: "value3"
                }
            });
            
            expect(syntax).
                toBe("<umbraco:Macro param1=\"value1\" param2=\"value2\" param3=\"value3\" Alias=\"myMacro\" runat=\"server\"></umbraco:Macro>");
            
        });
        
        it('can generate syntax for webforms with no params', function () {

            var syntax = macroService.generateWebFormsSyntax({
                macroAlias: "myMacro",
                marcoParamsDictionary: {}
            });

            expect(syntax).
                toBe("<umbraco:Macro Alias=\"myMacro\" runat=\"server\"></umbraco:Macro>");

        });
        
        it('can generate syntax for MVC', function () {

            var syntax = macroService.generateMvcSyntax({
                macroAlias: "myMacro",
                marcoParamsDictionary: {
                    param1: "value1",
                    param2: "value2",
                    param3: "value3"
                }
            });

            expect(syntax).
                toBe("@Umbraco.RenderMacro(\"myMacro\", new {param1=\"value1\", param2=\"value2\", param3=\"value3\"})");

           
        });
        
        it('can generate syntax for MVC with no params', function () {

            var syntax = macroService.generateMvcSyntax({
                macroAlias: "myMacro",
                marcoParamsDictionary: {}
            });

            expect(syntax).
                toBe("@Umbraco.RenderMacro(\"myMacro\")");

        });

    });
});