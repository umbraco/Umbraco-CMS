/**
 * @ngdoc controller
 * @name Umbraco.Editors.MediaType.EditController
 * @function
 *
 * @description
 * The controller for the media type editor
 */
(function () {
    "use strict";

    function MediaTypesEditController($scope, $routeParams, $q,
        mediaTypeResource, editorState, contentEditingHelper,
        navigationService, iconHelper, contentTypeHelper, notificationsService,
        localizationService, overlayHelper, eventsService, angularHelper) {

        var vm = this;
        var evts = [];
        var mediaTypeId = $routeParams.id;
        var create = $routeParams.create;
        var infiniteMode = $scope.model && $scope.model.infiniteMode;
        var mediaTypeIcon = "";

        vm.save = save;
        vm.close = close;

        vm.currentNode = null;
        vm.header = {};
        vm.header.editorfor = "content_mediatype";
        vm.header.setPageTitle = true;
        vm.contentType = {};
        vm.page = {};
        vm.page.loading = false;
        vm.page.saveButtonState = "init";
        vm.labels = {};
        vm.saveButtonKey = "buttons_save";
        vm.generateModelsKey = "buttons_saveAndGenerateModels";

        onInit();

        function onInit() {
            // get init values from model when in infinite mode
            if (infiniteMode) {
                mediaTypeId = $scope.model.id;
                create = $scope.model.create;
                vm.saveButtonKey = "buttons_saveAndClose";
                vm.generateModelsKey = "buttons_generateModelsAndClose";
            }
        }

        var labelKeys = [
            "general_design",
            "general_listView",
            "general_rights",

            "main_sections",
            "shortcuts_navigateSections",
            "shortcuts_addGroup",
            "shortcuts_addProperty",
            "shortcuts_addEditor",
            "shortcuts_editDataType",
            "shortcuts_toggleListView",
            "shortcuts_toggleAllowAsRoot",
            "shortcuts_addChildNode"
        ];

        localizationService.localizeMany(labelKeys).then(function (values) {
            // navigation
            vm.labels.design = values[0];
            vm.labels.listview = values[1];
            vm.labels.permissions = values[2];
            // keyboard shortcuts
            vm.labels.sections = values[3];
            vm.labels.navigateSections = values[4];
            vm.labels.addGroup = values[5];
            vm.labels.addProperty = values[6];
            vm.labels.addEditor = values[7];
            vm.labels.editDataType = values[8];
            vm.labels.toggleListView = values[9];
            vm.labels.allowAsRoot = values[10];
            vm.labels.addChildNode = values[11];

            vm.page.navigation = [
                {
                    "name": vm.labels.design,
                    "alias": "design",
                    "icon": "icon-document-dashed-line",
                    "view": "views/mediaTypes/views/design/design.html"
                },
                {
                    "name": vm.labels.listview,
                    "alias": "listView",
                    "icon": "icon-list",
                    "view": "views/mediaTypes/views/listview/listview.html"
                },
                {
                    "name": vm.labels.permissions,
                    "alias": "permissions",
                    "icon": "icon-keychain",
                    "view": "views/mediaTypes/views/permissions/permissions.html"
                }
            ];

            vm.page.keyboardShortcutsOverview = [
                {
                    "name": vm.labels.sections,
                    "shortcuts": [
                        {
                            "description": vm.labels.navigateSections,
                            "keys": [{ "key": "1" }, { "key": "3" }],
                            "keyRange": true
                        }
                    ]
                },
                {
                    "name": vm.labels.design,
                    "shortcuts": [
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
                        }
                    ]
                }
            ];

            initializeActiveNavigationPanel();
        });

        function initializeActiveNavigationPanel() {
            // Initialise first loaded page based on page route paramater
            // i.e. ?view=design|listview|permissions
            var initialViewSetFromRouteParams = false;
            var view = $routeParams.view;
            if (view) {
                var viewPath = "views/mediaTypes/views/" + view + "/" + view + ".html";
                for (var i = 0; i < vm.page.navigation.length; i++) {
                    if (vm.page.navigation[i].view === viewPath) {
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

        contentTypeHelper.checkModelsBuilderStatus().then(function (result) {
            vm.page.modelsBuilder = result;
            if (result) {
                //Models builder mode:
                vm.page.defaultButton = {
                    hotKey: "ctrl+s",
                    hotKeyWhenHidden: true,
                    labelKey: vm.saveButtonKey,
                    letter: "S",
                    handler: function () { vm.save(); }
                };
                vm.page.subButtons = [{
                    hotKey: "ctrl+g",
                    hotKeyWhenHidden: true,
                    labelKey: vm.generateModelsKey,
                    letter: "G",
                    handler: function () {

                        vm.page.saveButtonState = "busy";

                        vm.save().then(function (result) {

                            vm.page.saveButtonState = "busy";

                            localizationService.localize("modelsBuilder_buildingModels").then(function (headerValue) {
                                localizationService.localize("modelsBuilder_waitingMessage").then(function(msgValue) {
                                    notificationsService.info(headerValue, msgValue);
                                });
                            });

                            contentTypeHelper.generateModels().then(function (result) {

                                if (!result.lastError) {

                                    //re-check model status
                                    contentTypeHelper.checkModelsBuilderStatus().then(function (statusResult) {
                                        vm.page.modelsBuilder = statusResult;
                                    });

                                    //clear and add success
                                    vm.page.saveButtonState = "init";
                                    localizationService.localize("modelsBuilder_modelsGenerated").then(function(value) {
                                        notificationsService.success(value);
                                    });

                                } else {
                                    vm.page.saveButtonState = "error";
                                    localizationService.localize("modelsBuilder_modelsExceptionInUlog").then(function(value) {
                                        notificationsService.error(value);
                                    });
                                }

                            }, function () {
                                vm.page.saveButtonState = "error";
                                localizationService.localize("modelsBuilder_modelsGeneratedError").then(function(value) {
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
            mediaTypeResource.getScaffold(mediaTypeId)
                .then(function(dt) {
                    init(dt);

                    vm.page.loading = false;
                });
        }
        else {
            loadMediaType();
        }

        function loadMediaType() {
            vm.page.loading = true;

            mediaTypeResource.getById(mediaTypeId).then(function(dt) {
                init(dt);

                if(!infiniteMode) {
                    syncTreeNode(vm.contentType, dt.path, true);
                }

                vm.page.loading = false;
            });
        }

        /* ---------- SAVE ---------- */

        function save() {

            // only save if there is no overlays open
            if (overlayHelper.getNumberOfOverlays() === 0) {

                var deferred = $q.defer();

                vm.page.saveButtonState = "busy";

                // reformat allowed content types to array if id's
                vm.contentType.allowedContentTypes = contentTypeHelper.createIdArray(vm.contentType.allowedContentTypes);

                contentEditingHelper.contentEditorPerformSave({
                    saveMethod: mediaTypeResource.save,
                    scope: $scope,
                    content: vm.contentType,
                    rebindCallback: function (_, savedContentType) {
                        // we need to rebind... the IDs that have been created!
                        contentTypeHelper.rebindSavedContentType(vm.contentType, savedContentType);
                    }
                }).then(function (data) {
                    //success

                    if(!infiniteMode) {
                        syncTreeNode(vm.contentType, data.path);
                    }

                    // emit event
                    var args = { mediaType: vm.contentType };
                    eventsService.emit("editors.mediaType.saved", args);

                    if (mediaTypeIcon !== vm.contentType.icon) {
                        eventsService.emit("editors.tree.icon.changed", args);
                    }

                    vm.page.saveButtonState = "success";

                    if (infiniteMode && $scope.model.submit) {
                        $scope.model.submit();
                    }

                    deferred.resolve(data);

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

                    deferred.reject(err);
                });

                return deferred.promise;
            }
        }

        function init(contentType) {

            // convert icons for content type
            convertLegacyIcons(contentType);

            //set a shared state
            editorState.set(contentType);

            vm.contentType = contentType;

            mediaTypeIcon = contentType.icon;
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
            navigationService.syncTree({ tree: "mediaTypes", path: path.split(","), forceReload: initialLoad !== true }).then(function(syncArgs) {
                vm.currentNode = syncArgs.node;
            });
        }

        function close() {
            if (infiniteMode && $scope.model.close) {
                $scope.model.close();
            }
        }

        evts.push(eventsService.on("app.refreshEditor", function(name, error) {
            loadMediaType();
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

        // changes on the other "buttons" do not register on the current form, so we manually have to flag the form as dirty
        $scope.$watch("vm.contentType.allowedContentTypes.length + vm.contentType.allowAsRoot + vm.contentType.isContainer + vm.contentType.compositeContentTypes.length", function (newVal, oldVal) {
            if (oldVal === undefined) {
                // still initializing, ignore
                return;
            }
            angularHelper.getCurrentForm($scope).$setDirty();
        });
    }

    angular.module("umbraco").controller("Umbraco.Editors.MediaTypes.EditController", MediaTypesEditController);
})();
