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

        vm.insert = function (str) {
            vm.editor.moveCursorToPosition(vm.currentPosition);
            vm.editor.insert(str);
            vm.editor.focus();
        };


        vm.save = function () {
            vm.page.saveButtonState = "busy";

            vm.template.content = vm.editor.getValue();

            templateResource.save(vm.template).then(function (saved) {

                notificationsService.success("Template saved");
                vm.page.saveButtonState = "success";
                vm.template = saved;

                //sync state
                editorState.set(vm.template);
                navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });

            }, function (err) {
                notificationsService.error("Template save failed");
                vm.page.saveButtonState = "error";
            });
        };

        function persistCurrentLocation() {
            vm.currentPosition = vm.editor.getCursorPosition();
        }

        vm.init = function () {

            //we need to load this somewhere, for now its here.
            assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css");

            if($routeParams.create){

            	templateResource.getScaffold().then(function(template){
            		vm.ready(template);
            	});

            }else{

            	templateResource.getById($routeParams.id).then(function(template){
                    vm.ready(template);
                });

            }

        };


        vm.ready = function(template){
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
                    
                    //initial cursor placement
                    vm.editor.navigateFileEnd();
                    persistCurrentLocation();

                    //change on blur, focus
                    vm.editor.on("blur", persistCurrentLocation);
                    vm.editor.on("focus", persistCurrentLocation);
            	}
            }
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
        vm.openOrganizeOverlay = openOrganizeOverlay;
        vm.openPartialOverlay = openPartialOverlay;

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

                        case "partial":
                            //crappy hack due to dictionary items not in umbracoNode table
                            var code = "@Html.Partial(\"" + model.insert.node.name + "\")";
                            vm.insert(code);
                            break;
                            
                        case "umbracoField":
                          vm.insert(model.insert.umbracoField);
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
                title: "Insert macro",

                submit: function (model) {

                    var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, "Mvc");
                    vm.insert(macroObject.syntax);
                    vm.macroPickerOverlay.show = false;
                    vm.macroPickerOverlay = null;
                }
            };
        }


        function openPageFieldOverlay() {
            vm.pageFieldOverlay = {
                title: "Insert page field",
                description: "Insert data in template",
                submitButtonLabel: "Insert",
                closeButtonlabel: "Cancel",
                view: "insertfield",
                show: true,

                submit: function (model) {
                  vm.insert(model.umbracoField);
                  vm.pageFieldOverlay.show = false;
                  vm.pageFieldOverlay = null;
                },

                close: function (model) {
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
                title: "Insert dictionary item",

                select: function(node){
                	//crappy hack due to dictionary items not in umbracoNode table
                	var code = "@Umbraco.GetDictionaryValue(\"" + node.name + "\")";
                	vm.insert(code);

                	vm.dictionaryItemOverlay.show = false;
                    vm.dictionaryItemOverlay = null;
                },

                submit: function (model) {
                    console.log(model);
                },

                close: function (model) {
                    vm.dictionaryItemOverlay.show = false;
                    vm.dictionaryItemOverlay = null;
                }
            };
        }

        function openPartialOverlay() {
            vm.partialItemOverlay = {
                view: "treepicker",
                section: "settings", 
                treeAlias: "partialViews",
                entityType: "partialView",
                multiPicker: false,
                show: true,
                title: "Insert Partial view",

                select: function(node){
                    //crappy hack due to dictionary items not in umbracoNode table
                    var code = "@Html.Partial(\"" + node.name + "\")";
                    vm.insert(code);

                    vm.partialItemOverlay.show = false;
                    vm.partialItemOverlay = null;
                },

                submit: function (model) {
                    console.log(model);
                    vm.partialItemOverlay.show = false;
                    vm.partialItemOverlay = null;
                },

                close: function (model) {
                    vm.partialItemOverlay.show = false;
                    vm.partialItemOverlay = null;
                }
            };
        }

        function openQueryBuilderOverlay() {
            vm.queryBuilderOverlay = {
                view: "querybuilder",
                show: true,
                title: "Query for content",

                submit: function (model) {

                    var code = "\n@{\n" + "\tvar selection = " + model.result.queryExpression + ";\n}\n";
                    code += "<ul>\n" +
                                "\t@foreach(var item in selection){\n" +
                                    "\t\t<li>\n" +
                                        "\t\t\t<a href=\"@item.Url\">@item.Name</a>\n" +
                                    "\t\t</li>\n" +
                                "\t}\n" +
                            "</ul>\n\n";

                    vm.insert(code);
                    
                    vm.queryBuilderOverlay.show = false;
                    vm.queryBuilderOverlay = null;
                },

                close: function (model) {
                    vm.queryBuilderOverlay.show = false;
                    vm.queryBuilderOverlay = null;
                }
            };
        }


        function openOrganizeOverlay() {
            vm.organizeOverlay = {

                view: "organize",
                show: true,
                template: vm.template,
                title: "Organise template",

                submit: function (model) {
                    if (model.masterPage && model.masterPage.alias) {
                        vm.template.masterPageAlias = model.masterPage.alias;
                        vm.setLayout(model.masterPage.alias + ".cshtml");
                    } else {
                        vm.template.masterPageAlias = null;
                        vm.setLayout(null);
                    }

                    if (model.addRenderBody) {
                        vm.insert("@RenderBody()");
                    }

                    if (model.addRenderSection) {
                        vm.insert("@RenderSection(\"" +
                            model.renderSectionName +
                            "\", " +
                            model.mandatoryRenderSection +
                            ")");
                    }

                    if (model.addSection) {
                        vm.insert("@section " + model.sectionName + "\r\n{\r\n\r\n}\r\n");
                    }

                    vm.organizeOverlay.show = false;
                    vm.organizeOverlay = null;
                },

                close: function (model) {
                    vm.organizeOverlay.show = false;
                    vm.organizeOverlay = null;
                }

            }
        }

        vm.init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Templates.EditController", TemplatesEditController);
})();
