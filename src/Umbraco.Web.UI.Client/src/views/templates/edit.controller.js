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
                notificationsService.success("Template saved")
                vm.page.saveButtonState = "success";
                vm.template = saved;

                //sync state
                editorState.set(vm.template);
                navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });


            }, function(err){
                notificationsService.error("Template save failed")
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
        
        vm.openPageFieldOverlay = openPageFieldOverlay;
        vm.openDictionaryItemOverlay = openDictionaryItemOverlay;
        vm.openQueryBuilderOverlay = openQueryBuilderOverlay;
        vm.openMacroOverlay = openMacroOverlay;
        vm.openOrganizeOverlay = openOrganizeOverlay;

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
                view: "mediapicker",
                show: true,
                submit: function(model) {

                },
                close: function(model) {
                    vm.dictionaryItemOverlay.show = false;
                    vm.dictionaryItemOverlay = null;
                }
            };
        }

        function openQueryBuilderOverlay() {
            vm.queryBuilderOverlay = {
                view: "mediapicker",
                show: true,
                submit: function(model) {

                },
                close: function(model) {
                    vm.queryBuilderOverlay.show = false;
                    vm.queryBuilderOverlay = null;
                }
            };
        }

        function openOrganizeOverlay() {
            vm.organizeOverlay = {
                view: "/umbraco/views/common/dialogs/template/organize.html",
                show: true,
                template: vm.template,
                submit: function(model) {
                    vm.setLayout(model);
                },
                close: function(model) {
                    vm.organizeOverlay.show = false;
                    vm.organizeOverlay = null;
                }
            }
        }

        vm.init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Templates.EditController", TemplatesEditController);
})();
