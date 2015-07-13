/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.EditController
 * @function
 *
 * @description
 * The controller for the content type editor
 */
(function() {
	"use strict";

	function DocumentTypeEditController($scope, $routeParams, contentTypeResource, dataTypeResource, editorState, contentEditingHelper, formHelper, navigationService, iconHelper, contentTypeHelper, notificationsService) {

		var vm = this;

		vm.save = save;

		vm.currentNode = null;
		vm.contentType = {};
		vm.page = {};
		vm.page.navigation = [
			{
				"name": "Design",
				"icon": "document-dashed-line",
				"view": "views/documentType/views/design/design.html",
				"active": true
			},
			{
				"name": "List view",
				"icon": "list",
				"view": "views/documentType/views/listview/listview.html"
			},
			{
				"name": "Permissions",
				"icon": "keychain",
				"view": "views/documentType/views/permissions/permissions.html"
			},
			{
				"name": "Templates",
				"icon": "layout",
				"view": "views/documentType/views/templates/templates.html"
			}
		];

		vm.page.keyboardShortcutsOverview = [
		{
			"name": "Design",
			"shortcuts": [
				{
					"description": "Add tab",
					"keys": [{"key": "alt"},{"key": "shift"},{"key": "t"}]
				},
				{
					"description": "Add property",
					"keys": [{"key": "alt"},{"key": "shift"},{"key": "p"}]
				},
				{
					"description": "Add editor",
					"keys": [{"key": "alt"},{"key": "shift"},{"key": "e"}]
				},
				{
					"description": "Edit data type",
					"keys": [{"key": "alt"},{"key": "shift"},{"key": "d"}]
				}
			]
		},
		{
			"name": "List view",
			"shortcuts": [
				{
					"description": "Toggle list view",
					"keys": [{"key": "alt"},{"key": "shift"},{"key": "l"}]
				}
			]
		},
		{
			"name": "Permissions",
			"shortcuts": [
				{
					"description": "Toggle allow as root",
					"keys": [{"key": "alt"},{"key": "shift"},{"key": "r"}]
				}
			]
		}
	];

		if ($routeParams.create) {
			//we are creating so get an empty data type item
			contentTypeResource.getScaffold($routeParams.id)
				.then(function(dt) {
					init(dt);
				});
		}
		else {
			contentTypeResource.getById($routeParams.id).then(function(dt){
				init(dt);

				syncTreeNode(vm.contentType, dt.path, true);
			});
		}


		/* ---------- SAVE ---------- */

		function save() {

			//perform any pre-save logic here
			vm.contentType.allowedContentTypes = contentTypeHelper.reformatAllowedContentTypes(vm.contentType.allowedContentTypes);

			// update template holder on new content types
			if( vm.contentType.id === 0) {
				vm.contentType = contentTypeHelper.updateTemplateHolder(vm.contentType, true, true);
			}

			contentTypeResource.save(vm.contentType).then(function(dt){

				formHelper.resetForm({ scope: $scope, notifications: dt.notifications });
				contentEditingHelper.handleSuccessfulSave({
					scope: $scope,
					savedContent: dt,
					rebindCallback: function() {

					}
				});

				notificationsService.success("Document type save");
				//post save logic here -the saved doctype returns as a new object
				init(dt);

				syncTreeNode(vm.contentType, dt.path);
			});
		}


		function init(contentType){

			// set all tab to inactive
			if( contentType.groups.length !== 0 ) {
				angular.forEach(contentType.groups, function(group){

					angular.forEach(group.properties, function(property){
						// get data type details for each property
						getDataTypeDetails(property);
					});

				});
			}

			// convert legacy icons
			convertLegacyIcons(contentType);

			// insert template holder on new content types
			if(contentType.id === 0) {

				// insert template placeholder as default template
				contentType = contentTypeHelper.insertDefaultTemplateHolder(contentType);

				// insert template placeholder as allowed template
				contentType.allowedTemplates = contentTypeHelper.insertTemplateHolder(contentType, contentType.allowedTemplates);

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
			contentTypeArray.push({"icon":contentType.icon});

			// run through icon method
			iconHelper.formatContentTypeIcons(contentTypeArray);

			// set icon back on contentType
			contentType.icon = contentTypeArray[0].icon;

		}

		function getDataTypeDetails(property) {

			if( property.propertyState !== "init" ) {

				dataTypeResource.getById(property.dataTypeId)
					.then(function(dataType) {
						property.dataTypeIcon = dataType.icon;
						property.dataTypeName = dataType.name;
					});
			}
		}


		/** Syncs the content type  to it's tree node - this occurs on first load and after saving */
		function syncTreeNode(dt, path, initialLoad) {

			navigationService.syncTree({ tree: "documenttype", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
				vm.currentNode = syncArgs.node;
			});

		}

	}

	angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditController", DocumentTypeEditController);

})();
