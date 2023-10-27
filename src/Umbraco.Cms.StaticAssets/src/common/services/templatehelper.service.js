(function() {
   'use strict';

   function templateHelperService(localizationService) {

        //crappy hack due to dictionary items not in umbracoNode table
        function getInsertDictionarySnippet(nodeName) {
            return "@Umbraco.GetDictionaryValue(\"" + nodeName + "\")";
        }

        function getInsertPartialSnippet(parentId, nodeName) {

            var partialViewName = nodeName.replace(".cshtml", "");

            if(parentId) {
                partialViewName = parentId + "/" + partialViewName;
            }

            return "@await Html.PartialAsync(\"" + partialViewName + "\")";
        }

        function getQuerySnippet(queryExpression) {
            var code = "\n@{\n" + "\tvar selection = " + queryExpression + ";\n}\n";
                code += "<ul>\n" +
                            "\t@foreach (var item in selection)\n" +
                            "\t{\n" +
                                "\t\t<li>\n" +
                                    "\t\t\t<a href=\"@item.Url()\">@item.Name()</a>\n" +
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

        function getGeneralShortcuts(){
            var keys = [
                "shortcuts_generalHeader",
                "buttons_undo",
                "buttons_redo",
                "buttons_save"
            ];

            return localizationService.localizeMany(keys).then(function(data){

                var labels = {};
                labels.header = data[0];
                labels.undo = data[1];
                labels.redo = data[2];
                labels.save = data[3];

                return {
			        "name": labels.header,
			        "shortcuts": [
                        {
                            "description": labels.undo,
                            "keys": [{ "key": "ctrl" }, { "key": "z" }]
                        },
                        {
                            "description": labels.redo,
                            "keys": [{ "key": "ctrl" }, { "key": "y" }]
                        },
                        {
                            "description": labels.save,
                            "keys": [{ "key": "ctrl" }, { "key": "s" }]
                        }
			        ]
			    };
            });
        }

        function getEditorShortcuts(){

            var keys = [
                "shortcuts_editorHeader",
                "shortcuts_commentLine",
                "shortcuts_removeLine",
                "shortcuts_copyLineUp",
                "shortcuts_copyLineDown",
                "shortcuts_moveLineUp",
                "shortcuts_moveLineDown"
            ];

            return localizationService.localizeMany(keys).then(function(data){

                var labels = {};
                labels.header = data[0];
                labels.commentline = data[1];
                labels.removeline = data[2];
                labels.copylineup = data[3];
                labels.copylinedown = data[4];
                labels.movelineup = data[5];
                labels.movelinedown = data[6];

                return {
			        "name": labels.header,
			        "shortcuts": [
                        {
                            "description":labels.commentline,
                            "keys": [{ "key": "ctrl" }, { "key": "/" }]
                        },
                        {
                            "description": labels.removeline,
                            "keys": [{ "key": "ctrl" }, { "key": "d" }]
                        },
                        {
                            "description": labels.copylineup,
                            "keys": {
                                "win": [{ "key": "alt" }, { "key": "shift" }, { "key": "up" }],
                                "mac": [{ "key": "cmd" }, { "key": "alt" }, { "key": "up" }]
                            }
                        },
                        {
                            "description": labels.copylinedown,
                            "keys": {
                                "win": [{ "key": "alt" }, { "key": "shift" }, { "key": "down" }],
                                "mac": [{ "key": "cmd" }, { "key": "alt" }, { "key": "down" }]
                            }
                        },
                        {
                            "description": labels.movelineup,
                            "keys": [{ "key": "alt" }, { "key": "up" }]
                        },
                        {
                            "description": labels.movelinedown,
                            "keys": [{ "key": "alt" }, { "key": "down" }]
                        }
                    ]
			    };
            });
        }

        function getTemplateEditorShortcuts(){
            var keys = [
                "template_insert",
                "template_insertPageField",
                "template_insertPartialView",
                "template_insertDictionaryItem",
                "template_insertMacro",
                "template_queryBuilder",
                "template_insertSections",
                "template_mastertemplate"
            ];

            return localizationService.localizeMany(keys).then(function(data){

                var labels = {};
                labels.insert = data[0];
                labels.pagefield = data[1];
                labels.partialview = data[2];
                labels.dictionary = data[3];
                labels.macro = data[4];
                labels.querybuilder = data[5];
                labels.sections = data[6];
                labels.mastertemplate = data[7];

                return {
			        "name": "Umbraco", //No need to localise Umbraco is the same in all languages :)
			        "shortcuts": [
                        {
                            "description": labels.insert.concat(' ', labels.pagefield),
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "v" }]
                        },
                        {
                            "description": labels.insert.concat(' ', labels.partialview),
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "p" }]
                        },
                        {
                            "description": labels.insert.concat(' ', labels.dictionary),
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "d" }]
                        },
                        {
                            "description": labels.insert.concat(' ', labels.macro),
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "m" }]
                        },
                        {
                            "description": labels.querybuilder,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "q" }]
                        },
                        {
                            "description":  labels.insert.concat(' ', labels.sections),
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "s" }]
                        },
                        {
                            "description": labels.mastertemplate,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "t" }]
                        }
                    ]
			    };
            });
        }

        function getPartialViewEditorShortcuts(){
            var keys = [
                "template_insert",
                "template_insertPageField",
                "template_insertDictionaryItem",
                "template_insertMacro",
                "template_queryBuilder"
            ];

            return localizationService.localizeMany(keys).then(function(data){

                var labels = {};
                labels.insert = data[0];
                labels.pagefield = data[1];
                labels.dictionary = data[2];
                labels.macro = data[3];
                labels.querybuilder = data[4];

                return {
			        "name": "Umbraco", //No need to localise Umbraco is the same in all languages :)
			        "shortcuts": [
                        {
                            "description": labels.insert.concat(' ', labels.pagefield),
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "v" }]
                        },
                        {
                            "description": labels.insert.concat(' ', labels.dictionary),
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "d" }]
                        },
                        {
                            "description": labels.insert.concat(' ', labels.macro),
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "m" }]
                        },
                        {
                            "description": labels.querybuilder,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "q" }]
                        }
                    ]
			    };
            });


        }

        ////////////

        var service = {
            getInsertDictionarySnippet: getInsertDictionarySnippet,
            getInsertPartialSnippet: getInsertPartialSnippet,
            getQuerySnippet: getQuerySnippet,
            getRenderBodySnippet: getRenderBodySnippet,
            getRenderSectionSnippet: getRenderSectionSnippet,
            getAddSectionSnippet: getAddSectionSnippet,
            getGeneralShortcuts: getGeneralShortcuts,
            getEditorShortcuts: getEditorShortcuts,
            getTemplateEditorShortcuts: getTemplateEditorShortcuts,
            getPartialViewEditorShortcuts: getPartialViewEditorShortcuts
        };

        return service;

   }

   angular.module('umbraco.services').factory('templateHelper', templateHelperService);


})();
