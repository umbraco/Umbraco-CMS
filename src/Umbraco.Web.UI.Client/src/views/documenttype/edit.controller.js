/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.EditController
 * @function
 *
 * @description
 * The controller for the content type editor
 */
function DocumentTypeEditController($scope, $rootScope, $routeParams, $log, contentTypeResource, entityResource, dataTypeResource, editorState, contentEditingHelper, formHelper, navigationService, iconHelper) {

	$scope.page = {actions: [], menu: [], subViews: [] };
	$scope.sortingMode = false;
	$scope.currentNode = null; //the editors affiliated node

	$scope.page.navigation = [
		{
			"name": "Design",
			"icon": "document-dashed-line",
			"view": "views/documentType/views/design/design.html",
			"active": true,
			"tools": [
			{
				"name": "Compositions",
				"icon": "merge",
				"action": function() {
					$scope.openCompositionsDialog();
				}
			},
			{
				"name": "Reorder",
				"icon": "navigation",
				"action": function() {
					$scope.toggleSortingMode();
				}
			}
		]
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

		    syncTreeNode($scope.contentType, dt.path, true);
		});
	}
	

	/* ---------- SAVE ---------- */

	$scope.save = function() {

		//perform any pre-save logic here

		contentTypeResource.save($scope.contentType).then(function(dt){

			formHelper.resetForm({ scope: $scope, notifications: dt.notifications });
            contentEditingHelper.handleSuccessfulSave({
                scope: $scope,
                savedContent: dt,
                rebindCallback: function() {
                    
                }
            });

			//post save logic here -the saved doctype returns as a new object
            init(dt);

            syncTreeNode($scope.contentType, dt.path);
		});
	};

	/* ---------- TOOLBAR ---------- */

	$scope.toggleSortingMode = function() {
		$scope.sortingMode = !$scope.sortingMode;
	};

	$scope.openCompositionsDialog = function() {
		$scope.dialogModel = {};
		$scope.dialogModel.title = "Compositions";
		$scope.dialogModel.availableCompositeContentTypes = $scope.contentType.availableCompositeContentTypes;
		$scope.dialogModel.compositeContentTypes = $scope.contentType.compositeContentTypes;
		$scope.dialogModel.view = "views/documentType/dialogs/compositions/compositions.html";
		$scope.showDialog = true;

		$scope.dialogModel.close = function(){
			$scope.showDialog = false;
			$scope.dialogModel = null;
		};

		$scope.dialogModel.selectCompositeContentType = function(compositeContentType) {

			//merge composition with content type
			if( $scope.contentType.compositeContentTypes.indexOf(compositeContentType.alias) === -1 ) {

				mergeCompositeContentType(compositeContentType);

			// split composition from content type
			} else {

				splitCompositeContentType(compositeContentType);

			}

		}

	};

	function mergeCompositeContentType(compositeContentType) {

		contentTypeResource.getById(compositeContentType.id).then(function(composition){

			var groupsArrayLength = $scope.contentType.groups.length;
			var positionToPush = groupsArrayLength - 1;

			angular.forEach(composition.groups, function(compositionGroup){

				// set inherited state on tab
				compositionGroup.inherited = true;

				// set inherited state on properties
				angular.forEach(compositionGroup.properties, function(compositionProperty){
					compositionProperty.inherited = true;
				});

				// set tab state
				compositionGroup.tabState = "inActive";

				// if groups are named the same - merge the groups
				angular.forEach($scope.contentType.groups, function(contentTypeGroup){

					if( contentTypeGroup.name === compositionGroup.name ) {

						// set flag to show if properties has been merged into a tab
						compositionGroup.groupIsMerged = true;

						// make group inherited
						contentTypeGroup.inherited = true;

						// add properties to the top of the array
						contentTypeGroup.properties = compositionGroup.properties.concat(contentTypeGroup.properties);

						// make parentTabContentTypeNames to an array so we can push values
						if(contentTypeGroup.parentTabContentTypeNames === null || contentTypeGroup.parentTabContentTypeNames === undefined) {
							contentTypeGroup.parentTabContentTypeNames = [];
						}

						// push name to array of merged composite content types
						contentTypeGroup.parentTabContentTypeNames.push(compositeContentType.name);

						// make parentTabContentTypes to an array so we can push values
						if(contentTypeGroup.parentTabContentTypes === null || contentTypeGroup.parentTabContentTypes === undefined) {
							contentTypeGroup.parentTabContentTypes = [];
						}

						// push id to array of merged composite content types
						contentTypeGroup.parentTabContentTypes.push(compositeContentType.id);

					}

				});

				// if group is not merged - push it to the end of the array - before init tab
				if( compositionGroup.groupIsMerged === false || compositionGroup.groupIsMerged == undefined ) {

					// make parentTabContentTypeNames to an array so we can push values
					if(compositionGroup.parentTabContentTypeNames === null || compositionGroup.parentTabContentTypeNames === undefined) {
						compositionGroup.parentTabContentTypeNames = [];
					}

					// push name to array of merged composite content types
					compositionGroup.parentTabContentTypeNames.push(compositeContentType.name);

					// make parentTabContentTypes to an array so we can push values
					if(compositionGroup.parentTabContentTypes === null || compositionGroup.parentTabContentTypes === undefined) {
						compositionGroup.parentTabContentTypes = [];
					}

					// push id to array of merged composite content types
					compositionGroup.parentTabContentTypes.push(compositeContentType.id);

					//push init property to group
					addInitProperty(compositionGroup);

					// push group before placeholder tab
					$scope.contentType.groups.splice(positionToPush,0,compositionGroup);

				}

			});

		});


	}

	function splitCompositeContentType(compositeContentType) {

		angular.forEach($scope.contentType.groups, function(contentTypeGroup){

			if( contentTypeGroup.tabState !== "init" ) {

				var idIndex = contentTypeGroup.parentTabContentTypes.indexOf(compositeContentType.id);
				var nameIndex = contentTypeGroup.parentTabContentTypeNames.indexOf(compositeContentType.name);
				var groupIndex = $scope.contentType.groups.indexOf(contentTypeGroup);


				if( idIndex !== -1  ) {

					var properties = [];

					// remove all properties from composite content type
					angular.forEach(contentTypeGroup.properties, function(property){
						if(property.contentTypeId !== compositeContentType.id) {
							properties.push(property);
						}
					});

					// set new properties array to properties
					contentTypeGroup.properties = properties;

					// remove composite content type name and id from inherited arrays
					contentTypeGroup.parentTabContentTypes.splice(idIndex, 1);
					contentTypeGroup.parentTabContentTypeNames.splice(nameIndex, 1);

					// remove inherited state if there are no inherited properties
					if(contentTypeGroup.parentTabContentTypes.length === 0) {
						contentTypeGroup.inherited = false;
					}

					// remove group if there are no properties left
					if(contentTypeGroup.properties.length <= 1) {
						$scope.contentType.groups.splice(groupIndex, 1);
					}

				}

			}

		});
	}

	/* ---------- TABS ---------- */

	$scope.addTab = function(tab){

		$scope.activateTab(tab);

		// push new init tab to the scope
		addInitTab();

	};

	$scope.removeTab = function(tabIndex) {
		$scope.contentType.groups.splice(tabIndex, 1);
	};

	$scope.activateTab = function(tab) {

		// set all other tabs that are inactive to active
		angular.forEach($scope.contentType.groups, function(group){
			// skip init tab
			if(group.tabState !== "init") {
				group.tabState = "inActive";
			}
		});

		tab.tabState = "active";

	};

	$scope.updateTabTitle = function(tab) {
		if(tab.properties.length === 0) {
			addInitProperty(tab);
		}
	};

	function init(contentType){

		$scope.contentType = contentType;

		// set all tab to inactive
		if( $scope.contentType.groups.length !== 0 ) {
			angular.forEach($scope.contentType.groups, function(group){
				// set state
				group.tabState = "inActive";

				// push init/placeholder property
				addInitProperty(group);

				angular.forEach(group.properties, function(property){

					// get data type detaisl for each property
					getDataTypeDetails(property);

				});


			});
		}

		// convert legacy icons
		convertLegacyIcons();

		//set a shared state
        editorState.set($scope.contentType);

		// add init tab
		addInitTab();
	}

	function convertLegacyIcons() {

		// convert icons for composite content types
		iconHelper.formatContentTypeIcons($scope.contentType.availableCompositeContentTypes);

		// make array to store contentType icon
		var contentTypeArray = [];

		// push icon to array
		contentTypeArray.push({"icon":$scope.contentType.icon});

		// run through icon method
		iconHelper.formatContentTypeIcons(contentTypeArray);

		// set icon back on contentType
		$scope.contentType.icon = contentTypeArray[0].icon;

	}

	function getDataTypeDetails(property) {

		if( property.propertyState !== 'init' ) {

			dataTypeResource.getById(property.dataTypeId)
				.then(function(dataType) {
					property.dataTypeIcon = dataType.icon;
					property.dataTypeName = dataType.name;
				});
		}
	}

	function addInitTab() {

		// check i init tab already exists
		var addTab = true;

		angular.forEach($scope.contentType.groups, function(group){
			if(group.tabState === "init") {
				addTab = false;
			}
		});

		if(addTab) {
			$scope.contentType.groups.push({
				groups: [],
				properties:[],
				name: "",
				tabState: "init"
			});
		}
	}

	function addInitProperty(tab) {

		var addInitProperty = true;

		// check if there already is an init property
		angular.forEach(tab.properties, function(property){
			if(property.propertyState === "init") {
				addInitProperty = false;
			}
		});

		if(addInitProperty) {
			tab.properties.push({
				propertyState: "init"
			});
		}

	}

	function addInitPropertyOnActiveTab() {

		var addInitProperty = true;

		angular.forEach($scope.contentType.groups, function(group){

			if(group.tabState === 'active') {

				angular.forEach(group.properties, function(property){
					if(property.propertyState === "init") {
						addInitProperty = false;
					}
				});

				if(addInitProperty) {
					group.properties.push({
						propertyState: "init"
					});
				}

			}
		});

	}

	/* ---------- PROPERTIES ---------- */

	$scope.toggleGroupSize = function(group){
		if(group.columns !== 12){
			group.columns = 12;
		}else{
			group.columns = 6;
		}
	};

	$scope.editPropertyTypeSettings = function(property) {

		if(!property.inherited) {

			$scope.dialogModel = {};
			$scope.dialogModel.title = "Edit property type settings";
			$scope.dialogModel.property = property;
			$scope.dialogModel.view = "views/documentType/dialogs/editPropertySettings/editPropertySettings.html";
			$scope.showDialog = true;

			// set indicator on property to tell the dialog is open - is used to set focus on the element
			property.dialogIsOpen = true;

			// set property to active
			property.propertyState = "active";

			$scope.dialogModel.changePropertyEditor = function(property) {
				$scope.choosePropertyType(property);
			};

			$scope.dialogModel.editDataType = function(property) {
				$scope.configDataType(property);
			};

			$scope.dialogModel.submit = function(model){

				property.dialogIsOpen = false;

				$scope.showDialog = false;
				$scope.dialogModel = null;

				// push new init property to scope
				addInitPropertyOnActiveTab();

			};

			$scope.dialogModel.close = function(model){
				$scope.showDialog = false;
				$scope.dialogModel = null;

				// push new init property to scope
				addInitPropertyOnActiveTab();
			};

		}
	};

	$scope.choosePropertyType = function(property) {

		$scope.dialogModel = {};
		$scope.dialogModel.title = "Choose property type";
		$scope.dialogModel.view = "views/documentType/dialogs/property.html";
		$scope.showDialog = true;

		property.dialogIsOpen = true;

		$scope.dialogModel.selectDataType = function(selectedDataType) {

			contentTypeResource.getPropertyTypeScaffold(selectedDataType.id).then(function(propertyType){

				property.config = propertyType.config;
				property.editor = propertyType.editor;
				property.view = propertyType.view;
				property.dataTypeId = selectedDataType.id;
				property.dataTypeIcon = selectedDataType.icon;
				property.dataTypeName = selectedDataType.name;

				property.propertyState = "active";

				console.log(property);

				// open data type configuration
				$scope.editPropertyTypeSettings(property);

				// push new init tab to scope
				addInitTab();

			});

		};

		$scope.dialogModel.close = function(model){
			$scope.editPropertyTypeSettings(property);
		};

	};

	$scope.configDataType = function(property) {

		$scope.dialogModel = {};
		$scope.dialogModel.title = "Edit data type";
		$scope.dialogModel.dataType = {};
		$scope.dialogModel.property = property;
		$scope.dialogModel.view = "views/documentType/dialogs/editDataType/editDataType.html";
		$scope.dialogModel.multiActions = [
			{
				label: "Save",
				action: function(dataType) {
					saveDataType(dataType, false);
				}
			},
			{
				label: "Save as new",
				action: function(dataType) {
					saveDataType(dataType, true);
				}
			}
		];
		$scope.showDialog = true;

		function saveDataType(dataType, isNew) {

			var preValues = createPreValueProps(dataType.preValues);

			dataTypeResource.save(dataType, preValues, isNew).then(function(dataType) {

				contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType){

					property.config = propertyType.config;
					property.editor = propertyType.editor;
					property.view = propertyType.view;
					property.dataTypeId = dataType.id;
					property.dataTypeIcon = dataType.icon;
					property.dataTypeName = dataType.name;

					// open settings dialog
					$scope.editPropertyTypeSettings(property);

				});

			});

		}

		$scope.dialogModel.close = function(model){
			$scope.editPropertyTypeSettings(property);
		};

	};

    /** Syncs the content type  to it's tree node - this occurs on first load and after saving */
	function syncTreeNode(dt, path, initialLoad) {

	    navigationService.syncTree({ tree: "documenttype", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
	        $scope.currentNode = syncArgs.node;
	    });

	}

	function createPreValueProps(preVals) {
		var preValues = [];
		for (var i = 0; i < preVals.length; i++) {
			preValues.push({
				hideLabel: preVals[i].hideLabel,
				alias: preVals[i].key,
				description: preVals[i].description,
				label: preVals[i].label,
				view: preVals[i].view,
				value: preVals[i].value
			});
		}
		return preValues;
	}

	$scope.deleteProperty = function(tab, propertyIndex) {
		tab.properties.splice(propertyIndex, 1);
	};

	/* ---------- SORTING OPTIONS ---------- */

	$scope.sortableOptionsTab = {
		distance: 10,
		revert: true,
		tolerance: "pointer",
		opacity: 0.7,
		scroll: true,
		cursor: "move",
		placeholder: "ui-sortable-tabs-placeholder",
		zIndex: 6000,
		handle: ".edt-tab-handle",
		start: function (e, ui) {
			ui.placeholder.height(ui.item.height());
		},
		stop: function(e, ui){

		}
	};

	$scope.sortableOptionsEditor = {
		distance: 10,
		revert: true,
		tolerance: "pointer",
		connectWith: ".edt-property-list",
		opacity: 0.7,
		scroll: true,
		cursor: "move",
		placeholder: "ui-sortable-properties-placeholder",
		zIndex: 6000,
		handle: ".edt-property-handle",
		start: function (e, ui) {
			ui.placeholder.height(ui.item.height());
		},
		stop: function(e, ui){

		}
	};

}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditController", DocumentTypeEditController);
