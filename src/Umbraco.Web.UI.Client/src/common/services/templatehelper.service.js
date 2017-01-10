(function() {
   'use strict';

   function templateHelperService() {

        //crappy hack due to dictionary items not in umbracoNode table
        function getInsertDictionarySnippet(nodeName) {
            return "@Umbraco.GetDictionaryValue(\"" + nodeName + "\")";
        }

        function getInsertPartialSnippet(nodeName) {
            return "@Html.Partial(\"" + nodeName + "\")";
        }

        function getQuerySnippet(queryExpression) {
            var code = "\n@{\n" + "\tvar selection = " + queryExpression + ";\n}\n";
                code += "<ul>\n" +
                            "\t@foreach(var item in selection){\n" +
                                "\t\t<li>\n" +
                                    "\t\t\t<a href=\"@item.Url\">@item.Name</a>\n" +
                                "\t\t</li>\n" +
                            "\t}\n" +
                        "</ul>\n\n";
            return code;
        }

        function getRenderBodySnippet() {
            return "@RenderBody()";
        }

        function getRenderSectionSnippet(sectionName, mandatory) {
            return "@RenderSection(\"" + sectionName + "\", " + mandatory + ")";
        }

        function getAddSectionSnippet(sectionName) {
            return "@section " + sectionName + "\r\n{\r\n\r\n\t{0}\r\n\r\n}\r\n";
        }
        
        ////////////

        var service = {
            getInsertDictionarySnippet: getInsertDictionarySnippet,
            getInsertPartialSnippet: getInsertPartialSnippet,
            getQuerySnippet: getQuerySnippet,
            getRenderBodySnippet: getRenderBodySnippet,
            getRenderSectionSnippet: getRenderSectionSnippet,
            getAddSectionSnippet: getAddSectionSnippet
        };

        return service;

   }

   angular.module('umbraco.services').factory('templateHelper', templateHelperService);


})();
