describe('macro service tests', function () {
    var macroService;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        macroService = $injector.get('macroService');
    }));

    describe('generates macro syntax', function () {

        it('can generate syntax for webforms', function () {

            var syntax = macroService.generateWebFormsSyntax({
                macroAlias: "myMacro",
                macroParams: [
                    { alias: "param1", value: "value1" },
                    { alias: "param2", value: "value2" },
                    { alias: "param3", value: "value3" }
                ]
            });
            
            expect(syntax).
                toBe("<umbraco:Macro param1=\"value1\" param2=\"value2\" param3=\"value3\" Alias=\"myMacro\" runat=\"server\"></umbraco:Macro>");
            
        });
        
        it('can generate syntax for webforms with no params', function () {

            var syntax = macroService.generateWebFormsSyntax({
                macroAlias: "myMacro",
                macroParams: []
            });

            expect(syntax).
                toBe("<umbraco:Macro Alias=\"myMacro\" runat=\"server\"></umbraco:Macro>");

        });
        
        it('can generate syntax for MVC', function () {

            var syntax = macroService.generateMvcSyntax({
                macroAlias: "myMacro",
                macroParams: [
                    { alias: "param1", value: "value1" },
                    { alias: "param2", value: "value2" },
                    { alias: "param3", value: "value3" }
                ]
            });

            expect(syntax).
                toBe("@Umbraco.RenderMacro(\"myMacro\", new {param1=\"value1\", param2=\"value2\", param3=\"value3\"})");

           
        });
        
        it('can generate syntax for webforms with no params', function () {

            var syntax = macroService.generateMvcSyntax({
                macroAlias: "myMacro",
                macroParams: []
            });

            expect(syntax).
                toBe("@Umbraco.RenderMacro(\"myMacro\")");

        });

    });
});