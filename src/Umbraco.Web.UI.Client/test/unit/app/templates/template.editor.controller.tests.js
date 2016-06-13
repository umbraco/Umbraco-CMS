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

            beforeEach(function() {
                angular.module('umbraco.filters', []);
                angular.module('umbraco.directives', []);
                angular.module('umbraco.resources', []);
                angular.module('umbraco.services', []);
                angular.module('umbraco.packages', []);
                angular.module('umbraco.views', []);
                angular.module('ngCookies', []);
                angular.module('ngSanitize', []);
                angular.module('ngMobile', []);
                angular.module('tmh.dynamicLocale', []);
                angular.module('ngFileUpload', []);
                angular.module('LocalStorageModule', []);
            });

            beforeEach(module("umbraco"));

            beforeEach(inject(function($controller, $rootScope, $q) {

                controllerFactory = $controller;
                scope = $rootScope.$new();
                q = $q;

                ace = {
                    on: function(){}
                }

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
                        getById: resolvedPromise({})
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
                    macroService: {}
                });
            }

            it("has ace editor",
                function () {
                    expect(controller.editor).toBe(ace);
                });

            it("sets masterpage on template",
                function () {
                    controller.setLayout = function() {};

                    controller.openOrganizeOverlay();
                    controller.organizeOverlay.submit({
                        masterPage: {
                            alias: "NewMasterPage"
                        }
                    });
                    expect(controller.template.masterPageAlias).toBe("NewMasterPage");
                });

            it("changes layout value when masterpage is selected",
                function() {
                    var newTemplate;
                    ace.clearSelection = nada;
                    ace.navigateFileStart = nada;
                    ace.getValue = function () {
                        return "@{ Layout = null; }";
                    }
                    ace.setValue = function (value) {
                        newTemplate = value;
                    }

                    controller.openOrganizeOverlay();
                    controller.organizeOverlay.submit({
                        masterPage: {
                            alias: "NewMasterPage"
                        }
                    });
                    expect(newTemplate).toBe("@{ Layout = \"NewMasterPage.cshtml\"; }");
                });
        });

}());
