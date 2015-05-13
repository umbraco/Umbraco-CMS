/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.EditController
 * @function
 *
 * @description
 * The controller for the content type editor
 */
function DocumentTypeEditController($scope, $rootScope, $routeParams, $log, contentTypeResource, dataTypeResource) {
	$scope.page = {action: [], menu: [] };
	//$rootScope.emptySection = true; 

	contentTypeResource.getById($routeParams.id).then(function(dt){

		$scope.contentType = dt;

		// set first tab to active
		if( $scope.contentType.groups.length !== 0 ) {
			$scope.contentType.groups[0].tabIsActive = true;
		}

	});

	//hacking datatypes and their icons
	dataTypeResource.getAll().then(function(data){

		data = _.groupBy(data, function(dt){
			dt.icon = "icon-autofill";

			if(dt.name.indexOf("Dropdown") > -1 || dt.name.indexOf("Checkbox") > -1){
				dt.icon = "icon-bulleted-list";
				return "Lists";
			}

			if(dt.name.indexOf("Grid") > -1 || dt.name.indexOf("List View") > -1){
				dt.icon = "icon-item-arrangement";
				return "Collections";
			}

			if(dt.name.indexOf("picker") > -1){
				dt.icon ="icon-hand-pointer-alt";
				return "Pickers";
			}

			if(dt.name.indexOf("media") > -1 || dt.name.indexOf("Upload") > -1 || dt.name.indexOf("Crop") > -1){
				dt.icon ="icon-picture";
				return "Media";
			}

			return "Fields";
		});

		$scope.dataTypes = data;
	});

	$scope.actions = [{name: "Structure", cssClass: "list"},{name: "Structure", cssClass: "list"},{name: "Structure", cssClass: "list"}];


	/* ---------- TABS ---------- */

	$scope.addTab = function(){

		// set all tabs to inactive
		angular.forEach($scope.contentType.groups, function(group){
			group.tabIsActive = false;
		});

		// push tab
		$scope.contentType.groups.push({
			groups: [],
			properties:[],
			tabIsActive: true
		});

	};

	$scope.deleteTab = function(tabIndex) {

		$scope.contentType.groups.splice(tabIndex, 1);

		// activate previous tab
		if( $scope.contentType.groups.length === 1 ) {
			$scope.contentType.groups[0].tabIsActive = true;
		}

	};

	$scope.activateTab = function(tab) {

		// set all tabs to inactive
		angular.forEach($scope.contentType.groups, function(group){
			group.tabIsActive = false;
		});

		// activate tab
		tab.tabIsActive = true;

	};

	/* ---------- PROPERTIES ---------- */

	$scope.addProperty = function(properties){
		$scope.dialogModel = {};
		$scope.dialogModel.title = "Add property type";
		$scope.dialogModel.datatypes = $scope.dataTypes;
		$scope.dialogModel.addNew = true;
		$scope.dialogModel.view = "views/documentType/dialogs/property.html";

		$scope.dialogModel.close = function(model){
			properties.push(model.property);
			$scope.dialogModel = null;
		};
	};

	$scope.changePropertyName = function(property) {

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

			/*
			contentTypeResource.getPropertyTypeScaffold(dt.id)
				.then(function(pt){
					property.config = pt.config;
					property.editor = pt.editor;
					property.view = pt.view;
					$scope.dialogModel = null;
					$scope.showDialog = false;
				});
			*/

			property.dialogIsOpen = false;

			$scope.showDialog = false;
			$scope.dialogModel = null;

		};

		/*
		$scope.dialogModel.submit = function(){
			$scope.showDialog = false;
			$scope.dialogModel = null;
		};
		*/

		$scope.dialogModel.close = function(model){
			$scope.showDialog = false;
			$scope.dialogModel = null;
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


	$scope.addProperty = function(group){
		$log.log("open dialog");

		$scope.dialogModel = {};
		$scope.dialogModel.title = "Add property type";
		$scope.dialogModel.dataTypes = $scope.dataTypes;
		$scope.dialogModel.view = "views/documentType/dialogs/property.html";

		$scope.dialogModel.close = function(model){
			$scope.dialogModel = null;
		};
	};





	$scope.sortableOptionsFieldset = {
		distance: 10,
		revert: true,
		tolerance: "pointer",
		opacity: 0.7,
		scroll:true,
		cursor:"move",
		placeholder: "ui-sortable-placeholder",
		connectWith: ".edt-tabs",
		handle: ".handle",
		zIndex: 6000,
		start: function (e, ui) {
           	ui.placeholder.addClass( ui.item.attr("class") );
        },
        stop: function(e, ui){
         	ui.placeholder.remove();
        }
	};


	$scope.sortableOptionsEditor = {
		distance: 10,
		revert: true,
		tolerance: "pointer",
		connectWith: ".edt-props-sortable",
		opacity: 0.7,
		scroll: true,
		cursor: "move",
		handle: ".edt-property-handle",
		placeholder: "ui-sortable-placeholder",
		zIndex: 6000,
		start: function (e, ui) {

			// set all tabs to inactive to collapse all content
			angular.forEach($scope.contentType.groups, function(tab){
				$scope.$apply(function () {

					tab.tabIsActive = false;

				});
			});

		},
		stop: function(e, ui){
			console.log(e);
			console.log(ui);
		}
	};

	$scope.sortableOptionsTab = {
		distance: 10,
		revert: true,
		tolerance: "pointer",
		opacity: 0.7,
		scroll:true,
		cursor:"move",
		placeholder: "ui-sortable-placeholder",
		zIndex: 6000,
		handle: ".edt-tab-handle",
		start: function (e, ui) {

			// set all tabs to inactive to collapse all content
			angular.forEach($scope.contentType.groups, function(tab){
				$scope.$apply(function () {

					tab.tabIsActive = false;

				});
			});

		},
		stop: function(e, ui){
			console.log(e);
			console.log(ui);
		}
	};
            
}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditController", DocumentTypeEditController);
