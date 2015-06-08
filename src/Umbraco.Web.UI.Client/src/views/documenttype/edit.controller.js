/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.EditController
 * @function
 *
 * @description
 * The controller for the content type editor
 */
function DocumentTypeEditController($scope, $rootScope, $routeParams, $log, contentTypeResource, entityResource, dataTypeResource, editorState, contentEditingHelper, formHelper, navigationService) { 

	$scope.page = {actions: [], menu: [], subViews: [] };
	$scope.sortingMode = false;
	$scope.currentNode = null; //the editors affiliated node

	$scope.page.navigation = [
		{
			"name": "Design",
			"icon": "merge",
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
			"icon": "newspaper-alt",
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

			contentTypeResource.getById(compositeContentType.id).then(function(contentType){

				//merge composition with content type
				if( $scope.contentType.compositeContentTypes.indexOf(compositeContentType.alias) !== -1 ) {

					var groupsArrayLength = $scope.contentType.groups.length;
					var positionToPush = groupsArrayLength - 1;

					angular.forEach(contentType.groups, function(compositionGroup){

						// set inherited state on tab
						compositionGroup.inherited = true;
						compositionGroup.contentTypeId = compositeContentType.id;
						compositionGroup.contentTypeName = compositeContentType.name;

						// set inherited state on properties
						angular.forEach(compositionGroup.properties, function(property){
							property.inherited = true;
							property.contentTypeId = compositeContentType.id;
							property.contentTypeName = compositeContentType.name;
						});

						// set tab state
						compositionGroup.tabState = "inActive";

						// if groups are named the same - merge the groups
						angular.forEach($scope.contentType.groups, function(contentTypeGroup){

							if( contentTypeGroup.name === compositionGroup.name ) {

								// set flag to show if properties has been merged into a tab
								compositionGroup.groupIsMerged = true;

								// add properties to the top of the array
								contentTypeGroup.properties = compositionGroup.properties.concat(contentTypeGroup.properties);

							}

						});

						// if group is not merged - push it to the end of the array - before init tab
						if( compositionGroup.groupIsMerged === false || compositionGroup.groupIsMerged == undefined ) {
							$scope.contentType.groups.splice(positionToPush,0,compositionGroup);
						}

					});

				// split composition from content type
				} else {

					var newGroupsArray = [];

					angular.forEach($scope.contentType.groups, function(contentTypeGroup){

						// remove inherited tabs
						if( contentTypeGroup.contentTypeId === compositeContentType.id ) {

							var newProperties = false;

							// check if group contains properties that are not inherited
							angular.forEach(contentTypeGroup.properties, function(property){
								if(property.inherited === false) {
									newProperties = true;
								}
							});

							// if new properties keep tab in array
							if(newProperties) {
								newGroupsArray.push(contentTypeGroup);
							}

							// remove inherited properties in merged tabs
						} else {

							var newPropertiesArray = [];

							// create new array of properties which are not inherited
							angular.forEach(contentTypeGroup.properties, function(property){
								if(property.contentTypeId !== compositeContentType.id) {
									newPropertiesArray.push(property);
								}
							});

							contentTypeGroup.properties = newPropertiesArray;
							newGroupsArray.push(contentTypeGroup);

						}

					});

					$scope.contentType.groups = newGroupsArray;
				}

			});

		}

	};

	/* ---------- TABS ---------- */

	$scope.addTab = function(tab){

		$scope.activateTab(tab);

		// push new init tab to the scope
		addInitTab();

	};

	$scope.deleteTab = function(tabIndex) {
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

		//set a shared state
        editorState.set($scope.contentType);

		// add init tab
		addInitTab();
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


	$scope.changePropertyLabel = function(property) {

		var str = property.label;

		// capitalize all words
		str = str.replace(/\w\S*/g, function(txt){return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();});

		// remove spaces
		str = str.replace(/\s/g, '');

		property.alias = str;

	};

	$scope.toggleGroupSize = function(group){
		if(group.columns !== 12){
			group.columns = 12;
		}else{
			group.columns = 6;
		}
	};

	$scope.editPropertyTypeSettings = function(property) {
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
