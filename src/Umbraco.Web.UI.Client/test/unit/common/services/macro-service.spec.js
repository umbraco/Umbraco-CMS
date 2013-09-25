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
            expect(result.alias).toBe("Map");
            expect(result.params.length).toBe(2);
            expect(result.params[0].alias).toBe("test1");
            expect(result.params[0].value).toBe("asdf");
            expect(result.params[1].alias).toBe("test2");
            expect(result.params[1].value).toBe("hello");


        });
        
        it('can parse syntax for macros with body', function () {

            var result = macroService.parseMacroSyntax("<?UMBRACO_MACRO macroAlias='Map' test1=\"asdf\" test2='hello' ><img src='blah.jpg'/></?UMBRACO_MACRO>");

            expect(result).not.toBeNull();
            expect(result.alias).toBe("Map");
            expect(result.params.length).toBe(2);
            expect(result.params[0].alias).toBe("test1");
            expect(result.params[0].value).toBe("asdf");
            expect(result.params[1].alias).toBe("test2");
            expect(result.params[1].value).toBe("hello");


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