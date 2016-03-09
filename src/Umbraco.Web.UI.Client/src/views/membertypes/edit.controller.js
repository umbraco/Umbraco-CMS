/**
 * @ngdoc controller
 * @name Umbraco.Editors.MemberType.EditController
 * @function
 *
 * @description
 * The controller for the member type editor
 */
(function () {
    "use strict";

    function MemberTypesEditController($scope, $rootScope, $routeParams, $log, $filter, memberTypeResource, dataTypeResource, editorState, iconHelper, formHelper, navigationService, contentEditingHelper, notificationsService, $q, localizationService, overlayHelper, contentTypeHelper) {

        var vm = this;
        var localizeSaving = localizationService.localize("general_saving");

        vm.save = save;

        vm.currentNode = null;
        vm.contentType = {};
        vm.page = {};
        vm.page.loading = false;
        vm.page.saveButtonState = "init";
        vm.page.navigation = [
			{
			    "name": localizationService.localize("general_design"),
			    "icon": "icon-document-dashed-line",
			    "view": "views/membertypes/views/design/design.html",
			    "active": true
			}
        ];

        vm.page.keyboardShortcutsOverview = [
			{
                "name": localizationService.localize("shortcuts_shortcut"),
			    "shortcuts": [
					{
					    "description": localizationService.localize("shortcuts_addTab"),
					    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "t" }]
					},
					{
					    "description": localizationService.localize("shortcuts_addProperty"),
					    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "p" }]
					},
					{
					    "description": localizationService.localize("shortcuts_addEditor"),
					    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "e" }]
					},
					{
					    "description": localizationService.localize("shortcuts_editDataType"),
					    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "d" }]
					}
			    ]
			}
        ];

        contentTypeHelper.checkModelsBuilderStatus().then(function (result) {
            vm.page.modelsBuilder = result;
            if (result) {
                //Models builder mode:
                vm.page.defaultButton = {
                    hotKey: "ctrl+s",
                    hotKeyWhenHidden: true,
                    labelKey: "buttons_save",
                    letter: "S",
                    type: "submit",
                    handler: function () { vm.save(); }
                };
                vm.page.subButtons = [{
                    hotKey: "ctrl+g",
                    hotKeyWhenHidden: true,
                    labelKey: "buttons_saveAndGenerateModels",
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

                                if (result.success) { 

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

        if ($routeParams.create) {

            vm.page.loading = true;

            //we are creating so get an empty data type item
            memberTypeResource.getScaffold($routeParams.id)
				.then(function (dt) {
				    init(dt);

				    vm.page.loading = false;
				});
        }
        else {

            vm.page.loading = true;

            memberTypeResource.getById($routeParams.id).then(function (dt) {
                init(dt);

                syncTreeNode(vm.contentType, dt.path, true);

                vm.page.loading = false;
            });
        }

        function save() {
            // only save if there is no overlays open
            if(overlayHelper.getNumberOfOverlays() === 0) {

                var deferred = $q.defer();

                vm.page.saveButtonState = "busy";

                contentEditingHelper.contentEditorPerformSave({
                    statusMessage: localizeSaving,
                    saveMethod: memberTypeResource.save,
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
                    syncTreeNode(vm.contentType, data.path);

                    vm.page.saveButtonState = "success";

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

            // set all tab to inactive
            if (contentType.groups.length !== 0) {
                angular.forEach(contentType.groups, function (group) {

                    angular.forEach(group.properties, function (property) {
                        // get data type details for each property
                        getDataTypeDetails(property);
                    });

                });
            }

            // convert legacy icons
            convertLegacyIcons(contentType);

            //set a shared state
            editorState.set(contentType);

            vm.contentType = contentType;

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

            navigationService.syncTree({ tree: "membertypes", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
                vm.currentNode = syncArgs.node;
            });

        }


    }

    angular.module("umbraco").controller("Umbraco.Editors.MemberTypes.EditController", MemberTypesEditController);

})();
