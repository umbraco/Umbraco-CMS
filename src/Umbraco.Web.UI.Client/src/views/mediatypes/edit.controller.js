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

    function MediaTypesEditController($scope, $routeParams, mediaTypeResource, dataTypeResource, editorState, contentEditingHelper, formHelper, navigationService, iconHelper, contentTypeHelper, notificationsService, $filter, $q, localizationService, overlayHelper) {
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
			    "view": "views/mediatypes/views/design/design.html",
			    "active": true
			},
			{
			    "name": localizationService.localize("general_listView"),
			    "icon": "icon-list",
			    "view": "views/mediatypes/views/listview/listview.html"
			},
			{
			    "name": localizationService.localize("general_rights"),
			    "icon": "icon-keychain",
			    "view": "views/mediatypes/views/permissions/permissions.html"
			}
        ];

        vm.page.keyboardShortcutsOverview = [
			{
			    "name": localizationService.localize("main_sections"),
			    "shortcuts": [
					{
					    "description": localizationService.localize("shortcuts_navigateSections"),
					    "keys": [{ "key": "1" }, { "key": "3" }],
					    "keyRange": true
					}
			    ]
			},
			{
			    "name": localizationService.localize("general_design"),
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
			},
		{
		    "name": localizationService.localize("general_listView"),
		    "shortcuts": [
				{
				    "description": localizationService.localize("shortcuts_toggleListView"),
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "l" }]
				}
		    ]
		},
		{
		    "name": localizationService.localize("general_rights"),
		    "shortcuts": [
				{
				    "description": localizationService.localize("shortcuts_toggleAllowAsRoot"),
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "r" }]
				},
				{
				    "description": localizationService.localize("shortcuts_addChildNode"),
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "c" }]
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
                    labelKey: "buttons_save",
                    letter: "S",
                    type: "submit",
                    handler: function () { vm.save(); }
                };
                vm.page.subButtons = [{
                    hotKey: "ctrl+g",
                    labelKey: "buttons_generateModels",
                    letter: "G",
                    handler: function () {

                        vm.page.saveButtonState = "busy";
                        notificationsService.info("Building models", "this can take abit of time, don't worry");

                        contentTypeHelper.generateModels().then(function(result) {
                            vm.page.saveButtonState = "init";
                            //clear and add success
                            notificationsService.success("Models Generated");                            
                        }, function() {
                            notificationsService.error("Models could not be generated");
                            vm.page.saveButtonState = "error";
                        });
                    }
                }];
            }
        });

        if ($routeParams.create) {
            vm.page.loading = true;

            //we are creating so get an empty data type item
            mediaTypeResource.getScaffold($routeParams.id)
                .then(function(dt) {
                    init(dt);

                    vm.page.loading = false;
                });
        }
        else {
            vm.page.loading = true;

            mediaTypeResource.getById($routeParams.id).then(function(dt) {
                init(dt);

                syncTreeNode(vm.contentType, dt.path, true);

                vm.page.loading = false;
            });
        }

        /* ---------- SAVE ---------- */

        function save() {

            // only save if there is no overlays open
            if(overlayHelper.getNumberOfOverlays() === 0) {

                var deferred = $q.defer();

                vm.page.saveButtonState = "busy";

                // reformat allowed content types to array if id's
                vm.contentType.allowedContentTypes = contentTypeHelper.createIdArray(vm.contentType.allowedContentTypes);

                contentEditingHelper.contentEditorPerformSave({
                    statusMessage: localizeSaving,
                    saveMethod: mediaTypeResource.save,
                    scope: $scope,
                    content: vm.contentType,
                    //We do not redirect on failure for doc types - this is because it is not possible to actually save the doc
                    // type when server side validation fails - as opposed to content where we are capable of saving the content
                    // item if server side validation fails
                    redirectOnFailure: false,
                    //no-op for rebind callback... we don't really need to rebind for content types
                    rebindCallback: angular.noop
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

            // convert icons for content type
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
                    .then(function(dataType) {
                        property.dataTypeIcon = dataType.icon;
                        property.dataTypeName = dataType.name;
                    });
            }
        }


        /** Syncs the content type  to it's tree node - this occurs on first load and after saving */
        function syncTreeNode(dt, path, initialLoad) {
            navigationService.syncTree({ tree: "mediatypes", path: path.split(","), forceReload: initialLoad !== true }).then(function(syncArgs) {
                vm.currentNode = syncArgs.node;
            });
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.MediaTypes.EditController", MediaTypesEditController);
})();
