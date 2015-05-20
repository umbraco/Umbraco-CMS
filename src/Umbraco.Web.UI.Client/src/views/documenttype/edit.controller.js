/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.EditController
 * @function
 *
 * @description
 * The controller for the content type editor
 */
function DocumentTypeEditController($scope, $rootScope, $routeParams, $log, contentTypeResource, dataTypeResource) {

	$scope.page = {actions: [], menu: [] };
	$scope.actions = [{name: "Structure", cssClass: "list"},{name: "Structure", cssClass: "list"},{name: "Structure", cssClass: "list"}];
	$scope.sortingMode = false;


	contentTypeResource.getById($routeParams.id).then(function(dt){

		$scope.contentType = dt;

		// set all tab to active
		if( $scope.contentType.groups.length !== 0 ) {
			angular.forEach($scope.contentType.groups, function(group){
				group.tabState = "inActive";
			});
		}

		addInitTab();

	});

	/* ---------- TOOLBAR ---------- */

	$scope.toggleSortingMode = function() {
		$scope.sortingMode = !$scope.sortingMode;
	};


	/* ---------- TABS ---------- */

	$scope.addTab = function(tab){

		$scope.activateTab(tab);

		// push new init tab to the scope
		addInitTab;

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
		tab.properties.push({
			propertyState: "init"
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
		$scope.dialogModel.dataTypes = $scope.dataTypes;
		$scope.dialogModel.view = "views/documentType/dialogs/editPropertySettings/editPropertySettings.html";
		$scope.showDialog = true;

		// set indicator on property to tell the dialog is open - is used to set focus on the element
		property.dialogIsOpen = true;

		$scope.dialogModel.submit = function(dt){

			property.dialogIsOpen = false;
			$scope.showDialog = false;
			$scope.dialogModel = null;

		};

		$scope.dialogModel.close = function(model){
			$scope.showDialog = false;
			$scope.dialogModel = null;
		};

	};

	$scope.choosePropertyType = function(property, tab) {

		$scope.showDialog = true;
		$scope.dialogModel = {};
		$scope.dialogModel.title = "Choose property type";
		$scope.dialogModel.dataTypes = $scope.dataTypes;
		$scope.dialogModel.view = "views/documentType/dialogs/property.html";

		property.dialogIsOpen = true;

		$scope.dialogModel.selectDataType = function(selectedDataType) {

			contentTypeResource.getPropertyTypeScaffold(selectedDataType.id).then(function(propertyType){

				property.config = propertyType.config;
				property.editor = propertyType.editor;
				property.view = propertyType.view;
				property.dataType = selectedDataType;

				property.propertyState = "active";

				// open settings dialog
				$scope.editPropertyTypeSettings(property);

				// push new init property to scope
				addInitProperty(tab);

				// push new init tab to scope
				addInitTab();

			});

		};

		$scope.dialogModel.close = function(model){
			$scope.dialogModel = null;
			$scope.showDialog = false;
		};

	};

	$scope.addItems = function(tab){

		$scope.showDialog = true;
		$scope.dialogModel = {};
		$scope.dialogModel.title = "Add some stuff";
		$scope.dialogModel.dataTypes = $scope.dataTypes;
		$scope.dialogModel.view = "views/documentType/dialogs/property.html";

		var target = tab;
		if(tab.groups && tab.groups.length > 0){
			target = _.last(tab.groups);
		}

		$scope.dialogModel.close = function(model){
			$scope.dialogModel = null;
			$scope.showDialog = false;
		};

		$scope.dialogModel.submit = function(dt){
			contentTypeResource.getPropertyTypeScaffold(dt.id).then(function(pt){

				pt.label = dt.name + " field";
				pt.dataType = dt;
				target.properties.push(pt);

				// open settings dialog
				$scope.editPropertyTypeSettings(pt);

			});
		};
	};

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