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
                    setReadOnly: function () { },
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

                        getGeneralShortcuts: resolvedPromise({}),
                        getEditorShortcuts: resolvedPromise({}),
                        getTemplateEditorShortcuts: resolvedPromise({})
                    }
                });
            }

            it("has ace editor", function () {
                expect(controller.editor).toBe(ace);
            });
            
        });

}());
