/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.CreateController
 * @function
 * 
 * @description
 * The controller for the content creation dialog
 */
function contentCreateController($scope,
    $routeParams,
    contentTypeResource,
    iconHelper,
    $location,
    navigationService,
    blueprintConfig,
    authResource,
    contentResource,
    $q) {

    var mainCulture = $routeParams.mculture ? $routeParams.mculture : null;

    function initialize() {
        $scope.loading = true;
        $scope.allowedTypes = null;
        $scope.selectContentType = true;
        $scope.selectBlueprint = false;
        $scope.allowBlank = blueprintConfig.allowBlank;
        
        $q.all([contentTypeResource.getAllowedTypes($scope.currentNode.id), authResource.getCurrentUser()])
            .then(resolvedPromises => {
                [let types, let currentUser] = resolvedPromises;
            
                $scope.allowedTypes = iconHelper.formatContentTypeIcons(types);                
            
                if (currentUser.allowedSections.includes('settings')) {
                    $scope.hasSettingsAccess = true;

                    if ($scope.allowedTypes.length === 0) {
                        contentTypeResource.getCount()
                            .then(count => $scope.countTypes = count);                
                    }
                    
                    if ($scope.currentNode.id > -1) {
                        contentResource.getById($scope.currentNode.id)
                            .then(data => $scope.contentTypeId = data.contentTypeId);
                    }                
                }

                $scope.loading = false;
            });
    }

    function close() {
        navigationService.hideMenu();
    }

    function createBlank(docType) {
        $location
            .path("/content/content/edit/" + $scope.currentNode.id)
            .search("doctype", docType.alias)
            .search("create", "true")
            /* when we create a new node we want to make sure it uses the same 
            language as what is selected in the tree */
            .search("cculture", mainCulture)
            /* when we create a new node we must make sure that any previously 
            opened segments is reset */
            .search("csegment", null)
            /* when we create a new node we must make sure that any previously 
            used blueprint is reset */
            .search("blueprintId", null);
        close();
    }

    function createOrSelectBlueprintIfAny(docType) {
        $scope.docType = docType;

        const keys = Object.keys(docType.blueprints);
        if (keys.length === 0) {
            createBlank(docType);
            return;
        }
                
        // map the blueprints into a collection that's sortable in the view
        const blueprints = keys.map(k => ({ id: k, name: docType.blueprints[k] }));
        
        if (blueprintConfig.skipSelect) {
            createFromBlueprint(blueprints[0].id);
        } else {
            $scope.selectContentType = false;
            $scope.selectBlueprint = true;
            $scope.selectableBlueprints = blueprints;
        }       
    }

    function createFromBlueprint(blueprintId) {
        $location
            .path("/content/content/edit/" + $scope.currentNode.id)
            .search("doctype", $scope.docType.alias)
            .search("create", "true")
            .search("blueprintId", blueprintId);
        close();
    }

    $scope.close = function() {
        close();
    };

    $scope.closeDialog = function (showMenu) {
        navigationService.hideDialog(showMenu);
    };

    $scope.createContentType = function () {
        $location.path("/settings/documenttypes/edit/-1").search("create", "true");
        close();
    };

    $scope.editContentType = function () {
        $location.path("/settings/documenttypes/edit/" + $scope.contentTypeId).search("view", "permissions");
        close();
    };

    $scope.createBlank = createBlank;
    $scope.createOrSelectBlueprintIfAny = createOrSelectBlueprintIfAny;
    $scope.createFromBlueprint = createFromBlueprint;

    // the current node changes behind the scenes when the context menu is clicked without closing 
    // the default menu first, so we must watch the current node and re-initialize accordingly
    var unbindModelWatcher = $scope.$watch("currentNode", initialize);
    $scope.$on('$destroy', function () {
        unbindModelWatcher();
    });

}

angular.module("umbraco").controller("Umbraco.Editors.Content.CreateController", contentCreateController);

angular.module("umbraco").value("blueprintConfig", {
    skipSelect: false,
    allowBlank: true
});
