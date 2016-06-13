(function () {
    "use strict";

    function TemplatesEditController($scope, $routeParams, templateResource, assetsService, notificationsService, editorState, navigationService, appState, macroService) {

        var vm = this;
        vm.page = {};
        vm.page.loading = true;
        
        //menu
        vm.page.menu = {};
        vm.page.menu.currentSection = appState.getSectionState("currentSection");
        vm.page.menu.currentNode = null;

        vm.insert = function(str){
            vm.editor.insert(str);
            vm.editor.focus();
        };

        vm.save = function(){
            vm.page.saveButtonState = "busy";

            vm.template.content = vm.editor.getValue();

            templateResource.save(vm.template).then(function(saved){
                notificationsService.success("Template saved");
                vm.page.saveButtonState = "success";
                vm.template = saved;

                //sync state
                editorState.set(vm.template);
                navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });


            }, function(err){
                notificationsService.error("Template save failed");
                vm.page.saveButtonState = "error";
            });
        };

        vm.init = function(){


            //we need to load this somewhere, for now its here.
            assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css");
            templateResource.getById($routeParams.id).then(function(template){
                    
                    vm.page.loading = false;
                    vm.template = template;

                    //sync state
                    editorState.set(vm.template);
                    navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true }).then(function (syncArgs) {
                        vm.page.menu.currentNode = syncArgs.node;
                    });

                    vm.aceOption = {
                        mode: "razor",
                        theme: "chrome",

                        onLoad: function(_editor) {
                            vm.editor = _editor;
                    }
                };
            });

        };
        
        
        vm.setLayout = function(path){

            var templateCode = vm.editor.getValue();
            var newValue = path;
            var layoutDefRegex = new RegExp("(@{[\\s\\S]*?Layout\\s*?=\\s*?)(\"[^\"]*?\"|null)(;[\\s\\S]*?})", "gi");

            if (newValue !== undefined && newValue !== "") {
                if (layoutDefRegex.test(templateCode)) {
                    // Declaration exists, so just update it
                    templateCode = templateCode.replace(layoutDefRegex, "$1\"" + newValue + "\"$3");
                } else {
                    // Declaration doesn't exist, so prepend to start of doc
                    //TODO: Maybe insert at the cursor position, rather than just at the top of the doc?
                    templateCode = "@{\n\tLayout = \"" + newValue + "\";\n}\n" + templateCode;
                }
            } else {
                if (layoutDefRegex.test(templateCode)) {
                    // Declaration exists, so just update it
                    templateCode = templateCode.replace(layoutDefRegex, "$1null$3");
                }
            }

            vm.editor.setValue(templateCode);
            vm.editor.clearSelection();
            vm.editor.navigateFileStart();
        };


        vm.openPageFieldOverlay = openPageFieldOverlay;
        vm.openDictionaryItemOverlay = openDictionaryItemOverlay;
        vm.openQueryBuilderOverlay = openQueryBuilderOverlay;
        vm.openMacroOverlay = openMacroOverlay;
        vm.openInsertOverlay = openInsertOverlay;

        function openInsertOverlay() {

            vm.insertOverlay = {
                view: "insert",
                hideSubmitButton: true,
                show: true,
                submit: function(model) {

                    switch(model.insert.type) {
                        case "macro":

                            var macroObject = macroService.collectValueData(model.insert.selectedMacro, model.insert.macroParams, "Mvc");
                            vm.insert(macroObject.syntax);
                            break;

                        case "dictionary":
                            //crappy hack due to dictionary items not in umbracoNode table
                        	var code = "@Umbraco.GetDictionaryValue(\"" + model.insert.node.name + "\")";
                        	vm.insert(code);
                            break;
                    }

                    vm.insertOverlay.show = false;
                    vm.insertOverlay = null;

                },
                close: function(oldModel) {
                    vm.insertOverlay.show = false;
                    vm.insertOverlay = null;
                }
            };

        }


        function openMacroOverlay() {
           
            vm.macroPickerOverlay = {
                view: "macropicker",
                dialogData: {},
                show: true,
                submit: function(model) {

                    var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, "Mvc");
                    vm.insert(macroObject.syntax);
                    vm.macroPickerOverlay.show = false;
                    vm.macroPickerOverlay = null;
                }
            };
        }

 
        function openPageFieldOverlay() {
            vm.pageFieldOverlay = {
                view: "mediapicker",
                show: true,
                submit: function(model) {

                },
                close: function(model) {
                    vm.pageFieldOverlay.show = false;
                    vm.pageFieldOverlay = null;
                }
            };
        }


        function openDictionaryItemOverlay() {
            vm.dictionaryItemOverlay = {
                view: "treepicker",
                section: "settings", 
                treeAlias: "dictionary",
                entityType: "dictionary",
                multiPicker: false,

                show: true,

                submit: function(model) {
                    
                },

                select: function(node){
                	//crappy hack due to dictionary items not in umbracoNode table
                	var code = "@Umbraco.GetDictionaryValue(\"" + node.name + "\")";
                	vm.insert(code);
                	
                	vm.dictionaryItemOverlay.show = false;
                    vm.dictionaryItemOverlay = null;
                },

                close: function(model) {
                    vm.dictionaryItemOverlay.show = false;
                    vm.dictionaryItemOverlay = null;
                }
            };
        }


        function openQueryBuilderOverlay() {
            vm.queryBuilderOverlay = {
                view: "querybuilder",
                show: true,
                title: "Query for content",
               
                submit: function(model) {

                    var code = "\n@{\n" + "\tvar selection = " + model.result.queryExpression + ";\n}\n";
                    code += "<ul>\n" +
                                "\t@foreach(var item in selection){\n" +
                                    "\t\t<li>\n" +
                                        "\t\t\t<a href=\"@item.Url\">@item.Name</a>\n" +
                                    "\t\t</li>\n" +
                                "\t}\n" +
                            "</ul>\n\n";

                    vm.insert(code);
                },

                close: function(model) {
                    vm.queryBuilderOverlay.show = false;
                    vm.queryBuilderOverlay = null;
                }
            };
        }

        vm.init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Templates.EditController", TemplatesEditController);
})();
