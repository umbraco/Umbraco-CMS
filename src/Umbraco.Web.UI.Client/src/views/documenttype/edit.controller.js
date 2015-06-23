/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.EditController
 * @function
 *
 * @description
 * The controller for the content type editor
 */
function DocumentTypeEditController($scope, $rootScope, $routeParams, $log, contentTypeResource, entityResource, dataTypeResource, editorState, contentEditingHelper, formHelper, navigationService, iconHelper, contentTypeHelper, dataTypeHelper) {

	$scope.page = {actions: [], menu: [], subViews: [] };
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
					$scope.page.openCompositionsDialog();
				}
			},
			{
				"name": "Reorder",
				"icon": "navigation",
				"action": function() {
					$scope.page.toggleSortingMode();
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


	function init(contentType){

		$scope.contentType = contentType;

		// set all tab to inactive
		if( $scope.contentType.groups.length !== 0 ) {
			angular.forEach($scope.contentType.groups, function(group){
				// set state
				group.tabState = "inActive";

				// push init/placeholder property
				contentTypeHelper.addInitProperty(group);

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
		contentTypeHelper.addInitTab($scope.contentType);

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



    /** Syncs the content type  to it's tree node - this occurs on first load and after saving */
	function syncTreeNode(dt, path, initialLoad) {

	    navigationService.syncTree({ tree: "documenttype", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
	        $scope.currentNode = syncArgs.node;
	    });

	}

}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditController", DocumentTypeEditController);
