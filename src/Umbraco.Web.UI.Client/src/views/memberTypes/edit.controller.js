/**
 * @ngdoc controller
 * @name Umbraco.Editors.MemberTypes.EditController
 * @function
 *
 * @description
 * The controller for the member type editor
 */
(function () {
    "use strict";

    function MemberTypesEditController($scope, $routeParams, $q,
        memberTypeResource, editorState, iconHelper,
        navigationService, contentEditingHelper, notificationsService, localizationService,
        overlayHelper, contentTypeHelper, angularHelper, eventsService) {

        var evts = [];
        var vm = this;
        var infiniteMode = $scope.model && $scope.model.infiniteMode;
        var memberTypeId = $routeParams.id;
        var create = $routeParams.create;
        var memberTypeIcon = "";

        vm.save = save;
        vm.close = close;

        vm.editorfor = "visuallyHiddenTexts_newMember";
        vm.header = {};
        vm.header.editorfor = "content_membergroup";
        vm.header.setPageTitle = true;
        vm.currentNode = null;
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
                memberTypeId = $scope.model.id;
                create = $scope.model.create;
                vm.saveButtonKey = "buttons_saveAndClose";
                vm.generateModelsKey = "buttons_generateModelsAndClose";
            }
        }

        var labelKeys = [
            "general_design",
            "shortcuts_shortcut",
            "shortcuts_addGroup",
            "shortcuts_addProperty",
            "shortcuts_addEditor",
            "shortcuts_editDataType"
        ];

        localizationService.localizeMany(labelKeys).then(function(values){

            vm.labels.design = values[0];
            vm.labels.shortcut = values[1];
            vm.labels.addGroup = values[2];
            vm.labels.addProperty = values[3];
            vm.labels.addEditor = values[4];
            vm.labels.editDataType = values[5];

            vm.page.navigation = [
                {
                    "name": vm.labels.design,
                    "icon": "icon-document-dashed-line",
                    "view": "views/memberTypes/views/design/design.html",
                    "active": true
                }
            ];

            vm.page.keyboardShortcutsOverview = [
                {
                    "name": vm.labels.shortcut,
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
                }
            ];

        });

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
                    labelKey: infiniteMode ? "buttons_generateModelsAndClose" : "buttons_saveAndGenerateModels",
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
            memberTypeResource.getScaffold(memberTypeId)
                .then(function (dt) {
                    init(dt);

                    vm.page.loading = false;
                });
        }
        else {
            loadMemberType();
        }

        function loadMemberType() {

            vm.page.loading = true;

            memberTypeResource.getById(memberTypeId).then(function (dt) {
                init(dt);

                if (!infiniteMode) {
                    syncTreeNode(vm.contentType, dt.path, true);
                }

                vm.page.loading = false;
            });
        }

        /* ---------- SAVE ---------- */

        function save() {

            // only save if there is no overlays open
            if(overlayHelper.getNumberOfOverlays() === 0) {

                var deferred = $q.defer();

                vm.page.saveButtonState = "busy";

                contentEditingHelper.contentEditorPerformSave({
                    saveMethod: memberTypeResource.save,
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
                    var args = { memberType: vm.contentType };
                    eventsService.emit("editors.memberType.saved", args);

                    if (memberTypeIcon !== vm.contentType.icon) {
                        eventsService.emit("editors.tree.icon.changed", args);
                    }

                    vm.page.saveButtonState = "success";

                    if(infiniteMode && $scope.model.submit) {
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

            // convert legacy icons
            convertLegacyIcons(contentType);

            //set a shared state
            editorState.set(contentType);

            vm.contentType = contentType;

            memberTypeIcon = contentType.icon;
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
            navigationService.syncTree({ tree: "memberTypes", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
                vm.currentNode = syncArgs.node;
            });
        }

        function close() {
            if (infiniteMode && $scope.model.close) {
                $scope.model.close();
            }
        }

        evts.push(eventsService.on("app.refreshEditor", function (name, error) {
            loadMemberType();
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
    }

    angular.module("umbraco").controller("Umbraco.Editors.MemberTypes.EditController", MemberTypesEditController);

})();
