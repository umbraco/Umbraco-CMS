/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.EditController
 * @function
 *
 * @description
 * The controller for the content type editor
 */
(function () {
    "use strict";

    function DocumentTypesEditController($scope, $routeParams, $q,
        contentTypeResource, editorState, contentEditingHelper,
        navigationService, iconHelper, contentTypeHelper, notificationsService,
        localizationService, overlayHelper, eventsService, angularHelper, editorService) {

        var vm = this;
        var evts = [];

        var disableTemplates = Umbraco.Sys.ServerVariables.features.disabledFeatures.disableTemplates;
        var documentTypeId = $routeParams.id;
        var create = $routeParams.create;
        var noTemplate = $routeParams.notemplate;
        var isElement = $routeParams.iselement;
        var icon = $routeParams.icon;
        var allowVaryByCulture = $routeParams.culturevary;
        var infiniteMode = $scope.model && $scope.model.infiniteMode;
        var documentTypeIcon = "";

        vm.save = save;
        vm.close = close;

        vm.currentNode = null;
        vm.contentType = {};
        vm.header = {};
        vm.header.editorfor = "content_documentType";
        vm.header.setPageTitle = true;
        vm.labels = {};
        vm.submitButtonKey = "buttons_save";
        vm.generateModelsKey = "buttons_saveAndGenerateModels";

        vm.page = {};
        vm.page.loading = false;
        vm.page.saveButtonState = "init";
        vm.page.navigation = [];

        var labelKeys = [
            "general_design",
            "general_listView",
            "general_rights",
            "treeHeaders_templates",
            "main_sections",
            "shortcuts_navigateSections",
            "shortcuts_addTab",
            "shortcuts_addGroup",
            "shortcuts_addProperty",
            "defaultdialogs_selectEditor",
            "shortcuts_editDataType",
            "shortcuts_toggleListView",
            "shortcuts_toggleAllowAsRoot",
            "shortcuts_addChildNode",
            "shortcuts_addTemplate",
            "shortcuts_toggleAllowCultureVariants"
        ];

        onInit();

        function onInit() {
            // get init values from model when in infinite mode
            if (infiniteMode) {
                documentTypeId = $scope.model.id;
                create = $scope.model.create;
                if (create && !documentTypeId) documentTypeId = -1;
                noTemplate = $scope.model.notemplate || $scope.model.noTemplate;
                isElement = $scope.model.isElement;
                icon = $scope.model.icon;
                allowVaryByCulture = $scope.model.allowVaryByCulture;
                vm.submitButtonKey = "buttons_saveAndClose";
                vm.generateModelsKey = "buttons_generateModelsAndClose";
            }
        }

        localizationService.localizeMany(labelKeys).then(function (values) {
            // navigation
            vm.labels.design = values[0];
            vm.labels.listview = values[1];
            vm.labels.permissions = values[2];
            vm.labels.templates = values[3];
            // keyboard shortcuts
            vm.labels.sections = values[4];
            vm.labels.navigateSections = values[5];
            vm.labels.addTab = values[6]
            vm.labels.addGroup = values[7];
            vm.labels.addProperty = values[8];
            vm.labels.addEditor = values[9];
            vm.labels.editDataType = values[10];
            vm.labels.toggleListView = values[11];
            vm.labels.allowAsRoot = values[12];
            vm.labels.addChildNode = values[13];
            vm.labels.addTemplate = values[14];
            vm.labels.allowCultureVariants = values[15];

            vm.page.keyboardShortcutsOverview = [
                {
                    "name": vm.labels.sections,
                    "shortcuts": [
                        {
                            "description": vm.labels.navigateSections,
                            "keys": [{ "key": "1" }, { "key": "4" }],
                            "keyRange": true
                        }
                    ]
                },
                {
                    "name": vm.labels.design,
                    "shortcuts": [
                        {
                            "description": vm.labels.addTab,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "a" }]
                        },
                        {
                            "description": vm.labels.addGroup,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "g" }]
                        },
                        {
                            "description": vm.labels.addProperty,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "p" }]
                        },
                        {
                            "description": vm.labels.addEditor,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "e" }]
                        },
                        {
                            "description": vm.labels.editDataType,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "d" }]
                        }
                    ]
                },
                {
                    "name": vm.labels.listview,
                    "shortcuts": [
                        {
                            "description": vm.labels.toggleListView,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "l" }]
                        }
                    ]
                },
                {
                    "name": vm.labels.permissions,
                    "shortcuts": [
                        {
                            "description": vm.labels.allowAsRoot,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "r" }]
                        },
                        {
                            "description": vm.labels.addChildNode,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "c" }]
                        },
                        {
                            "description": vm.labels.allowCultureVariants,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "v" }]
                        }
                    ]
                },
                {
                    "name": vm.labels.templates,
                    "shortcuts": [
                        {
                            "description": vm.labels.addTemplate,
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "t" }]
                        }
                    ]
                }
            ];
        });

        contentTypeHelper.checkModelsBuilderStatus().then(function (result) {
            vm.page.modelsBuilder = result;
            if (result) {
                //Models builder mode:
                vm.page.defaultButton = {
                    alias: "save",
                    hotKey: "ctrl+s",
                    hotKeyWhenHidden: true,
                    labelKey: vm.submitButtonKey,
                    letter: "S",
                    handler: function () { vm.save(); }
                };
                vm.page.subButtons = [{
                    alias: "saveAndGenerateModels",
                    hotKey: "ctrl+g",
                    hotKeyWhenHidden: true,
                    labelKey: vm.generateModelsKey,
                    letter: "G",
                    handler: function () {

                        vm.page.saveButtonState = "busy";

                        saveInternal().then(function (result) {

                            vm.page.saveButtonState = "busy";

                            localizationService.localize("modelsBuilder_buildingModels").then(function (headerValue) {
                                localizationService.localize("modelsBuilder_waitingMessage").then(function (msgValue) {
                                    notificationsService.info(headerValue, msgValue);
                                });
                            });

                            contentTypeHelper.generateModels().then(function (result) {

                                // generateModels() returns the dashboard content
                                if (!result.lastError) {

                                    //re-check model status
                                    contentTypeHelper.checkModelsBuilderStatus().then(function (statusResult) {
                                        vm.page.modelsBuilder = statusResult;
                                    });

                                    //clear and add success
                                    vm.page.saveButtonState = "init";
                                    localizationService.localize("modelsBuilder_modelsGenerated").then(function (value) {
                                        notificationsService.success(value);
                                    });

                                } else {
                                    vm.page.saveButtonState = "error";
                                    localizationService.localize("modelsBuilder_modelsExceptionInUlog").then(function (value) {
                                        notificationsService.error(value);
                                    });
                                }

                            }, function () {
                                vm.page.saveButtonState = "error";
                                localizationService.localize("modelsBuilder_modelsGeneratedError").then(function (value) {
                                    notificationsService.error(value);
                                });
                            });
                        });
                    }
                }];
            }
        });

        if (create) {
            vm.page.loading = true;

            //we are creating so get an empty data type item
            contentTypeResource.getScaffold(documentTypeId)
                .then(function (dt) {
                    init(dt);
                    vm.page.loading = false;
                });
        }
        else {
            loadDocumentType();
        }

        function loadDocumentType() {
            vm.page.loading = true;
            contentTypeResource.getById(documentTypeId).then(function (dt) {
                init(dt);
                // we don't need to sync the tree in infinite mode
                if (!infiniteMode) {
                    syncTreeNode(vm.contentType, dt.path, true);
                }
                vm.page.loading = false;
            });
        }

        function loadButtons() {
            vm.page.navigation = vm.contentType.apps;

            if (disableTemplates === true) {
                Utilities.forEach(vm.contentType.apps,
                    (app, index) => {
                        if (app.alias === "templates") {
                            vm.page.navigation.splice(index, 1);
                        }
                    });
            }

            initializeActiveNavigationPanel();
        }

        function initializeActiveNavigationPanel() {
            // Initialise first loaded panel based on page route paramater
            // i.e. ?view=design|listview|permissions
            var initialViewSetFromRouteParams = false;
            var view = $routeParams.view;
            if (view) {
                for (var i = 0; i < vm.page.navigation.length; i++) {
                    if (vm.page.navigation[i].alias.localeCompare(view, undefined, { sensitivity: 'accent' }) === 0) {
                        vm.page.navigation[i].active = true;
                        initialViewSetFromRouteParams = true;
                        break;
                    }
                }
            }
            if (initialViewSetFromRouteParams === false) {
                vm.page.navigation[0].active = true;
            }
        }

        /* ---------- SAVE ---------- */

        function save() {
            saveInternal().then(Utilities.noop, Utilities.noop);
        }

        /** This internal save method performs the actual saving and returns a promise, not to be bound to any buttons but used by other bound methods */
        function saveInternal() {

            // only save if there are no dialogs open
            if (overlayHelper.getNumberOfOverlays() === 0 && (editorService.getNumberOfEditors() === 0 || infiniteMode)) {

                vm.page.saveButtonState = "busy";

                // reformat allowed content types to array if id's
                vm.contentType.allowedContentTypes = contentTypeHelper.createIdArray(vm.contentType.allowedContentTypes);

                //if this is a new item and it's creating a template, ensure that the template alias is synced correctly
                syncTemplateAlias(vm.contentType);

                return contentEditingHelper.contentEditorPerformSave({
                    saveMethod: contentTypeResource.save,
                    scope: $scope,
                    content: vm.contentType,
                    infiniteMode: infiniteMode,
                    rebindCallback: function (_, savedContentType) {
                        // we need to rebind... the IDs that have been created!
                        contentTypeHelper.rebindSavedContentType(vm.contentType, savedContentType);
                    }
                }).then(function (data) {
                    // allow UI to access server validation state
                    vm.contentType.ModelState = data.ModelState;

                    //success
                    // we don't need to sync the tree in infinite mode
                    if (!infiniteMode) {
                        syncTreeNode(vm.contentType, data.path);
                    }

                    // emit event
                    var args = { documentType: vm.contentType };
                    eventsService.emit("editors.documentType.saved", args);

                    if (documentTypeIcon !== vm.contentType.icon) {
                        eventsService.emit("editors.tree.icon.changed", args);
                    }

                    vm.page.saveButtonState = "success";

                    if (infiniteMode && $scope.model.submit) {
                        $scope.model.documentTypeAlias = vm.contentType.alias;
                        $scope.model.documentTypeKey = vm.contentType.key;
                        $scope.model.submit($scope.model);
                    }

                    return $q.resolve(data);
                }, function (err) {
                    //error
                    if (err) {
                        // allow UI to access server validation state
                        vm.contentType.ModelState = err.data.ModelState;
                        editorState.set($scope.content);
                    }
                    else {
                        localizationService.localize("speechBubbles_validationFailedHeader").then(function (headerValue) {
                            localizationService.localize("speechBubbles_validationFailedMessage").then(function (msgValue) {
                                notificationsService.error(headerValue, msgValue);
                            });
                        });
                    }
                    vm.page.saveButtonState = "error";
                    return $q.reject(err);
                });
            }
            else {
                return $q.reject();
            }
        }

        function init(contentType) {

            // insert template on new doc types
            if (!noTemplate && contentType.id === 0) {
                contentType.defaultTemplate = contentTypeHelper.insertDefaultTemplatePlaceholder(contentType.defaultTemplate);
                contentType.allowedTemplates = contentTypeHelper.insertTemplatePlaceholder(contentType.allowedTemplates);
            }

            // set isElement checkbox by default
            if (isElement) {
                contentType.isElement = true;
            }

            // set icon if one is provided
            if (icon !== null && icon !== undefined) {
                contentType.icon = icon;
            }

            // set vary by culture checkbox by default
            if (allowVaryByCulture) {
                contentType.allowCultureVariant = true;
            }

            // convert icons for content type
            convertLegacyIcons(contentType);

            //set a shared state
            editorState.set(contentType);

            vm.contentType = contentType;

            documentTypeIcon = contentType.icon;

            loadButtons();
        }

        /** Syncs the template alias for new doc types before saving if a template is to be created */
        function syncTemplateAlias(contentType) {
            if (!noTemplate && contentType.id === 0) {
                //sync default template that had the placeholder flag
                if (contentType.defaultTemplate !== null && contentType.defaultTemplate.placeholder) {
                    contentType.defaultTemplate.name = contentType.name;
                    contentType.defaultTemplate.alias = contentType.alias;
                }
                //sync allowed templates that had the placeholder flag
                contentType.allowedTemplates.forEach(function (allowedTemplate) {
                    if (allowedTemplate.placeholder) {
                        allowedTemplate.name = contentType.name;
                        allowedTemplate.alias = contentType.alias;
                    }
                });
            }
        }

        function convertLegacyIcons(contentType) {
            // make array to store contentType icon
            var contentTypeArray = [];

            // push icon to array
            contentTypeArray.push({ "icon": contentType.icon });

            // run through icon method
            iconHelper.formatContentTypeIcons(contentTypeArray);

            // set icon back on contentType
            contentType.icon = contentTypeArray[0].icon;
        }

        /** Syncs the content type  to it's tree node - this occurs on first load and after saving */
        function syncTreeNode(dt, path, initialLoad) {
            const args = { tree: "documentTypes", path: path.split(","), forceReload: initialLoad !== true };
            navigationService.syncTree(args)
                .then(function (syncArgs) {
                    vm.currentNode = syncArgs.node;
                });
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close($scope.model);
            }
        }

        evts.push(eventsService.on("app.refreshEditor", function (name, error) {
            loadDocumentType();
        }));

        evts.push(eventsService.on("editors.documentType.reload", function (name, args) {
            if (args && args.node && vm.contentType.id === args.node.id) {
                loadDocumentType();
            }
        }));

        evts.push(eventsService.on("editors.documentType.saved", function(name, args) {
            if(args.documentType.allowedTemplates.length > 0) {
                navigationService.hasTree("templates").then(function (treeExists) {
                    if (treeExists) {
                        navigationService.syncTree({ tree: "templates", path: [], forceReload: true })
                            .then(function (syncArgs) {
                                navigationService.reloadNode(syncArgs.node);
                            });
                    }
                });
            }
        }));

        evts.push(eventsService.on("editors.groupsBuilder.changed", function(name, args) {
            angularHelper.getCurrentForm($scope).$setDirty();
        }));

        //ensure to unregister from all events!
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }
        });

        // #3368 - changes on the other "buttons" do not register on the current form, so we manually have to flag the form as dirty
        $scope.$watch(
            "vm.contentType.allowedContentTypes.length + vm.contentType.allowAsRoot + vm.contentType.allowCultureVariant + vm.contentType.isElement + " +
            "vm.contentType.allowedTemplates.length + vm.contentType.isContainer + vm.contentType.compositeContentTypes.length",
            function(newVal, oldVal) {
                if (oldVal === undefined) {
                    // still initializing, ignore
                    return;
                }
                angularHelper.getCurrentForm($scope).$setDirty();
            }
        );
    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentTypes.EditController", DocumentTypesEditController);
})();
