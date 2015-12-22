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
	
        function DocumentTypesEditController($scope, $routeParams, modelsResource, contentTypeResource, dataTypeResource, editorState, contentEditingHelper, formHelper, navigationService, iconHelper, contentTypeHelper, notificationsService, $filter, $q, localizationService) {

        var vm = this;

        vm.save = save;

        vm.currentNode = null;
        vm.contentType = {};
        vm.page = {};
        vm.page.loading = false;
        vm.page.saveButtonState = "init";
        vm.page.navigation = [
			{
			    "name": "Design",
			    "icon": "icon-document-dashed-line",
			    "view": "views/documenttypes/views/design/design.html",
			    "active": true
			},
			{
			    "name": "List view",
			    "icon": "icon-list",
			    "view": "views/documenttypes/views/listview/listview.html"
			},
			{
			    "name": "Permissions",
			    "icon": "icon-keychain",
			    "view": "views/documenttypes/views/permissions/permissions.html"
			},
			{
			    "name": "Templates",
			    "icon": "icon-layout",
			    "view": "views/documenttypes/views/templates/templates.html"
			}
        ];

        vm.page.keyboardShortcutsOverview = [
			{
			    "name": "Sections",
			    "shortcuts": [
					{
					    "description": "Navigate sections",
					    "keys": [{ "key": "1" }, { "key": "4" }],
					    "keyRange": true
					}
			    ]
			},
			{
			    "name": "Design",
			    "shortcuts": [
				{
				    "description": "Add tab",
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "t" }]
				},
				{
				    "description": "Add property",
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "p" }]
				},
				{
				    "description": "Add editor",
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "e" }]
				},
				{
				    "description": "Edit data type",
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "d" }]
				}
			    ]
			},
		{
		    "name": "List view",
		    "shortcuts": [
				{
				    "description": "Toggle list view",
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "l" }]
				}
		    ]
		},
		{
		    "name": "Permissions",
		    "shortcuts": [
				{
				    "description": "Toggle allow as root",
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "r" }]
				},
				{
				    "description": "Add child node",
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "c" }]
				}
		    ]
		},
		{
		    "name": "Templates",
		    "shortcuts": [
				{
				    "description": "Add template",
				    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "t" }]
				}
		    ]
		}
        ];

        if ($routeParams.create) {
            vm.page.loading = true;

            //we are creating so get an empty data type item
            contentTypeResource.getScaffold($routeParams.id)
				.then(function (dt) {

				    init(dt);

				    vm.page.loading = false;

				});
        }
        else {
            vm.page.loading = true;

            contentTypeResource.getById($routeParams.id).then(function (dt) {
                init(dt);

                syncTreeNode(vm.contentType, dt.path, true);

                vm.page.loading = false;

            });
        }


        /* ---------- SAVE ---------- */

        function save() {
            var deferred = $q.defer();

            vm.page.saveButtonState = "busy";

            // reformat allowed content types to array if id's
            vm.contentType.allowedContentTypes = contentTypeHelper.createIdArray(vm.contentType.allowedContentTypes);

            contentEditingHelper.contentEditorPerformSave({
                statusMessage: "Saving...",
                saveMethod: contentTypeResource.save,
                scope: $scope,
                content: vm.contentType,
                // we need to rebind... the IDs that have been created!
                rebindCallback: function (origContentType, savedContentType) {
                    vm.contentType.id = savedContentType.id;
                    vm.contentType.groups.forEach(function(group) {
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
                        localizationService.localize("speechBubbles_validationFailedMessage").then(function(msgValue) {
                            notificationsService.error(headerValue, msgValue);
                        });
                    });
                }
                vm.page.saveButtonState = "error";

                deferred.reject(err);
            });
            return deferred.promise;

        }

        function init(contentType) {
            //get available composite types
            contentTypeResource.getAvailableCompositeContentTypes(contentType.id).then(function (result) {
                contentType.availableCompositeContentTypes = result;
                // convert icons for composite content types
                iconHelper.formatContentTypeIcons(contentType.availableCompositeContentTypes);
            });

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
            if (!$routeParams.notemplate && contentType.id === 0) {
                contentType.defaultTemplate = contentTypeHelper.insertDefaultTemplatePlaceholder(contentType.defaultTemplate);
                contentType.allowedTemplates = contentTypeHelper.insertTemplatePlaceholder(contentType.allowedTemplates);
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
					.then(function (dataType) {
					    property.dataTypeIcon = dataType.icon;
					    property.dataTypeName = dataType.name;
					});
            }
        }

        /** Syncs the content type  to it's tree node - this occurs on first load and after saving */
        function syncTreeNode(dt, path, initialLoad) {
            navigationService.syncTree({ tree: "documenttypes", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
                vm.currentNode = syncArgs.node;
            });
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentTypes.EditController", DocumentTypesEditController);
})();
