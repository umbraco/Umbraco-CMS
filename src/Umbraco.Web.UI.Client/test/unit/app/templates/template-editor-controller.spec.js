/// <reference path="../../../../lib/angular/1.1.5/angular.js" />
/// <reference path="../../../lib/angular/angular-mocks.js" />
/// <reference path="../../../../src/app.js" />
/// <reference path="../../../../src/views/templates/edit.controller.js" />

(function() {
    "use strict";

    describe("templates editor controller",
        function() {


            var scope,
                controllerFactory,
                q,
                ace,
                controller,
                nada = function() {};

            // UNCOMMENT TO RUN WITH RESHARPERS TESTRUNNER FOR JS
            //beforeEach(function() {
            //    angular.module('umbraco.filters', []);
            //    angular.module('umbraco.directives', []);
            //    angular.module('umbraco.resources', []);
            //    angular.module('umbraco.services', []);
            //    angular.module('umbraco.packages', []);
            //    angular.module('umbraco.views', []);
            //    angular.module('ngCookies', []);
            //    angular.module('ngSanitize', []);
            //    angular.module('ngMobile', []);
            //    angular.module('tmh.dynamicLocale', []);
            //    angular.module('ngFileUpload', []);
            //    angular.module('LocalStorageModule', []);
            //});

            beforeEach(module("umbraco"));

            beforeEach(inject(function($controller, $rootScope, $q) {

                controllerFactory = $controller;
                scope = $rootScope.$new();
                q = $q;

                ace = {
                    on: function(){},
                    navigateFileEnd: function() {},
                    getCursorPosition: function() {},
                    getValue: function() {},
                    setValue: function() {},
                    focus: function() {},
                    clearSelection: function() {},
                    navigateFileStart: function() {},
                    commands: {
                        bindKey: function() {},
                        addCommands: function() {}
                    }
                };

                controller = createController();
                scope.$digest();
                controller.aceOption.onLoad(ace);

            }));

            function resolvedPromise(obj) {
                return function() {
                    var def = q.defer();
                    def.resolve(obj);
                    return def.promise;
                }
            }

            function createController() {
                return controllerFactory("Umbraco.Editors.Templates.EditController",
                {
                    $scope: scope,
                    $routeParams: {},
                    templateResource: {
                        getById: resolvedPromise({}),
                        getAll: resolvedPromise({})
                    },
                    assetsService: {
                        loadCss: function() {}
                    },
                    notificationsService: {
                    },
                    editorState: {
                        set: function(){}
                    },
                    navigationService: {
                        syncTree: resolvedPromise({})
                    },
                    appState: {
                        getSectionState : function() { return {}; }
                    },
                    macroService: {},
                    contentEditingHelper: {},
                    localizationService: {
                        localize: resolvedPromise({})
                    },
                    angularHelper: {
                        getCurrentForm: function() { 
                            return {
                                $setDirty: function() {},
                                $setPristine: function() {}
                            }
                        }
                    },
                    templateHelper: {
                        getInsertDictionary: function() { return ""; },
                        getInsertPartialSnippet: function() { return ""; },
                        getQuerySnippet: function() { return ""; },
                        getRenderBodySnippet: function() { return ""; },
                        getRenderSectionSnippet: function() { return ""; },
                        getGeneralShortcuts: function() { return ""; },
                        getEditorShortcuts: function() { return ""; },
                        getTemplateEditorShortcuts: function() { return ""; }
                    }
                });
            }

            it("has ace editor", function () {
                expect(controller.editor).toBe(ace);
            });

            it("sets masterpage on template", function () {
                controller.setLayout = function() {};

                controller.openMasterTemplateOverlay();
                controller.masterTemplateOverlay.submit({
                    selectedItem: {
                        alias: "NewMasterPage"
                    }
                });
                expect(controller.template.masterTemplateAlias).toBe("NewMasterPage");
            });

            it("changes layout value when masterpage is selected", function() {
                var newTemplate;
                ace.clearSelection = nada;
                ace.navigateFileStart = nada;
                ace.getValue = function () {
                    return "@{ Layout = null; }";
                }
                ace.setValue = function (value) {
                    newTemplate = value;
                }

                controller.openMasterTemplateOverlay();
                controller.masterTemplateOverlay.submit({
                    selectedItem: {
                        alias: "NewMasterPage"
                    }
                });
                expect(newTemplate).toBe("@{ Layout = \"NewMasterPage.cshtml\"; }");
            });
            
        });

}());
