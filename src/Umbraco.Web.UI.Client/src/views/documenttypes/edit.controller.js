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

    function DocumentTypesEditController($scope, $routeParams, contentTypeResource, dataTypeResource, editorState, contentEditingHelper, formHelper, navigationService, iconHelper, contentTypeHelper, notificationsService, $q, localizationService, overlayHelper, eventsService, angularHelper, editorService) {

        var vm = this;
        var evts = [];

        var disableTemplates = Umbraco.Sys.ServerVariables.features.disabledFeatures.disableTemplates;
        var documentTypeId = $routeParams.id;
        var create = $routeParams.create;
        var noTemplate = $routeParams.notemplate;
        var infiniteMode = $scope.model && $scope.model.infiniteMode;

        vm.save = save;
        vm.close = close;

        vm.currentNode = null;
        vm.contentType = {};
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
            "shortcuts_addProperty",
            "shortcuts_addEditor",
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
            if(infiniteMode) {
                documentTypeId = $scope.model.id;
                create = $scope.model.create;
                noTemplate = $scope.model.notemplate;
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
            vm.labels.addTab = values[6];
            vm.labels.addProperty = values[7];
            vm.labels.addEditor = values[8];
            vm.labels.editDataType = values[9];
            vm.labels.toggleListView = values[10];
            vm.labels.allowAsRoot = values[11];
            vm.labels.addChildNode = values[12];
            vm.labels.addTemplate = values[13];
            vm.labels.allowCultureVariants = values[14];

            var buttons = [
                {
                    "name": vm.labels.design,
                    "alias": "design",
                    "icon": "icon-document-dashed-line",
                    "view": "views/documenttypes/views/design/design.html",
                    "active": true
                },
                {
                    "name": vm.labels.listview,
                    "alias": "listView",
                    "icon": "icon-list",
                    "view": "views/documenttypes/views/listview/listview.html"
                },
                {
                    "name": vm.labels.permissions,
                    "alias": "permissions",
                    "icon": "icon-keychain",
                    "view": "views/documenttypes/views/permissions/permissions.html"
                },
                {
                    "name": vm.labels.templates,
                    "alias": "templates",
                    "icon": "icon-layout",
                    "view": "views/documenttypes/views/templates/templates.html"
                }
            ];

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
                            "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "t" }]
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

            loadButtons(buttons);

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
                    type: "submit",
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
                if(!infiniteMode) { 
                    syncTreeNode(vm.contentType, dt.path, true);
                }
                vm.page.loading = false;
            });
        }

        function loadButtons(buttons) {

            angular.forEach(buttons,
                function (val, index) {

                    if (disableTemplates === true && val.alias === "templates") {
                        buttons.splice(index, 1);
                    }

                });

            vm.page.navigation = buttons;
        }

        /* ---------- SAVE ---------- */

        function save() {
            saveInternal().then(angular.noop, angular.noop);
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
                    //We do not redirect on failure for doc types - this is because it is not possible to actually save the doc
                    // type when server side validation fails - as opposed to content where we are capable of saving the content
                    // item if server side validation fails
                    redirectOnFailure: false,
                    // we need to rebind... the IDs that have been created!
                    rebindCallback: function (origContentType, savedContentType) {
                        vm.contentType.id = savedContentType.id;
                        vm.contentType.groups.forEach(function (group) {
                            if (!group.name) return;
                            var k = 0;
                            while (k < savedContentType.groups.length && savedContentType.groups[k].name != group.name)
                                k++;
                            if (k == savedContentType.groups.length) {
                                group.id = 0;
                                return;
                            }
                            var savedGroup = savedContentType.groups[k];
                            if (!group.id) group.id = savedGroup.id;

                            group.properties.forEach(function (property) {
                                if (property.id || !property.alias) return;
                                k = 0;
                                while (k < savedGroup.properties.length && savedGroup.properties[k].alias != property.alias)
                                    k++;
                                if (k == savedGroup.properties.length) {
                                    property.id = 0;
                                    return;
                                }
                                var savedProperty = savedGroup.properties[k];
                                property.id = savedProperty.id;
                            });
                        });
                    }
                }).then(function (data) {
                    //success
                    // we don't need to sync the tree in infinite mode
                    if(!infiniteMode) {
                        syncTreeNode(vm.contentType, data.path);
                    }

                    // emit event
                    var args = { documentType: vm.contentType };
                    eventsService.emit("editors.documentType.saved", args);
                    
                    vm.page.saveButtonState = "success";

                    if(infiniteMode && $scope.model.submit) {
                        $scope.model.documentTypeAlias = vm.contentType.alias;
                        $scope.model.submit($scope.model);
                    }

                    return $q.resolve(data);
                }, function (err) {
                    //error
                    if (err) {
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

            // set all tab to inactive
            if (contentType.groups.length !== 0) {
                angular.forEach(contentType.groups, function (group) {

                    angular.forEach(group.properties, function (property) {
                        // get data type details for each property
                        getDataTypeDetails(property);
                    });

                });
            }

            // insert template on new doc types
            if (!noTemplate && contentType.id === 0) {
                contentType.defaultTemplate = contentTypeHelper.insertDefaultTemplatePlaceholder(contentType.defaultTemplate);
                contentType.allowedTemplates = contentTypeHelper.insertTemplatePlaceholder(contentType.allowedTemplates);
            }

            // convert icons for content type
            convertLegacyIcons(contentType);

            //set a shared state
            editorState.set(contentType);

            vm.contentType = contentType;
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
                angular.forEach(contentType.allowedTemplates, function (allowedTemplate) {
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

        function getDataTypeDetails(property) {
            if (property.propertyState !== "init") {
                dataTypeResource.getById(property.dataTypeId)
                    .then(function (dataType) {
                        property.dataTypeIcon = dataType.icon;
                        property.dataTypeName = dataType.name;
                    });
            }
        }

        /** Syncs the content type  to it's tree node - this occurs on first load and after saving */
        function syncTreeNode(dt, path, initialLoad) {
            const args = { tree: "documenttypes", path: path.split(","), forceReload: initialLoad !== true };
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

        evts.push(eventsService.on("editors.documentType.saved", function(name, args) {
            if(args.documentType.allowedTemplates.length > 0){
                navigationService.syncTree({ tree: "templates", path: [], forceReload: true })
                    .then(function (syncArgs) {
                        navigationService.reloadNode(syncArgs.node)
                    });
            }
        }));

        //ensure to unregister from all events!
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }
        });

        // #3368 - changes on the other "buttons" do not register on the current form, so we manually have to flag the form as dirty 
        $scope.$watch("vm.contentType.allowedContentTypes.length + vm.contentType.allowAsRoot + vm.contentType.allowedTemplates.length + vm.contentType.isContainer", function (newVal, oldVal) {
            if (oldVal === undefined) {
                // still initializing, ignore
                return;
            }
            angularHelper.getCurrentForm($scope).$setDirty();
        });
    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentTypes.EditController", DocumentTypesEditController);
})();
