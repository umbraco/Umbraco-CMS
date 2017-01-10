describe('service: templateHelper', function () {

    var templateHelper;

    beforeEach(module("umbraco.services"));

    beforeEach(inject(function ($injector) {
        templateHelper = $injector.get('templateHelper');
    }));

    afterEach(inject(function ($rootScope) {
        $rootScope.$apply();
    }));

    describe('getInsertDictionarySnippet', function () {

        it('should return the snippet for inserting a dictionary item', function () {
            var snippet = '@Umbraco.GetDictionaryValue("nodeName")';
            expect(templateHelper.getInsertDictionarySnippet('nodeName')).toBe(snippet);
        });

    });

    describe('getInsertPartialSnippet', function () {

        it('should return the snippet for inserting a partial', function () {
            var snippet = '@Html.Partial("nodeName")';
            expect(templateHelper.getInsertPartialSnippet("nodeName")).toBe(snippet);
        });
        
    });

    describe('getQuerySnippet', function () {

        it('should return the snippet for a query', function () {
            var queryExpression = "queryExpression";
            var snippet = "\n@{\n" + "\tvar selection = " + queryExpression + ";\n}\n";
                snippet += "<ul>\n" +
                            "\t@foreach(var item in selection){\n" +
                                "\t\t<li>\n" +
                                    "\t\t\t<a href=\"@item.Url\">@item.Name</a>\n" +
                                "\t\t</li>\n" +
                            "\t}\n" +
                        "</ul>\n\n";

            expect(templateHelper.getQuerySnippet(queryExpression)).toBe(snippet);
        });
        
    });

    describe('getRenderBodySnippet', function () {

        it('should return the snippet for render body', function () {
            var snippet = '@RenderBody()';
            expect(templateHelper.getRenderBodySnippet()).toBe(snippet);
        });
        
    });

    describe('getRenderSectionSnippet', function () {

        it('should return the snippet for defining a section', function () {
            var snippet = '@RenderSection("sectionName", false)';
            expect(templateHelper.getRenderSectionSnippet("sectionName", false)).toBe(snippet);
        });

        it('should return the snippet for defining a mandatory section', function () {
            var snippet = '@RenderSection("sectionName", true)';
            expect(templateHelper.getRenderSectionSnippet("sectionName", true)).toBe(snippet);
        });
        
    });

    describe('getAddSectionSnippet', function () {

        it('should return the snippet for implementing a section', function () {
            var sectionName = "sectionName";
            var snippet = "@section " + sectionName + "\r\n{\r\n\r\n\t{0}\r\n\r\n}\r\n";
            expect(templateHelper.getAddSectionSnippet(sectionName)).toEqual(snippet);
        });
        
    });

});