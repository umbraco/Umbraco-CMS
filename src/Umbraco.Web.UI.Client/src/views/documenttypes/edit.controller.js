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

            // update placeholder template information on new doc types
            if (!$routeParams.notemplate && vm.contentType.id === 0) {
                vm.contentType = contentTypeHelper.updateTemplatePlaceholder(vm.contentType);
            }

            contentEditingHelper.contentEditorPerformSave({
                statusMessage: "Saving...",
                saveMethod: contentTypeResource.save,
                scope: $scope,
                content: vm.contentType,
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

            // sort properties after sort order
            angular.forEach(contentType.groups, function (group) {
                group.properties = $filter('orderBy')(group.properties, 'sortOrder');
            });

            // insert template on new doc types
            if (!$routeParams.notemplate && contentType.id === 0) {
                contentType.defaultTemplate = contentTypeHelper.insertDefaultTemplatePlaceholder(contentType.defaultTemplate);
                contentType.allowedTemplates = contentTypeHelper.insertTemplatePlaceholder(contentType.allowedTemplates);
            }

            //set a shared state
            editorState.set(contentType);

            vm.contentType = contentType;

        }

        function convertLegacyIcons(contentType) {

            // convert icons for composite content types
            iconHelper.formatContentTypeIcons(contentType.availableCompositeContentTypes);

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
