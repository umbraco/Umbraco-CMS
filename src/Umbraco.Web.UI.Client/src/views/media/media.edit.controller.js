/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.EditController
 * @function
 * 
 * @description
 * The controller for the media editor
 */
function mediaEditController($scope, $routeParams, $q, appState, mediaResource, entityResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, fileManager, formHelper, editorState, umbRequestHelper, $http, eventsService) {
    
    var evts = [];
    var nodeId = null;
    var create = false;
    var infiniteMode = $scope.model && $scope.model.infiniteMode;

    // when opening the editor through infinite editing get the 
    // node id from the model instead of the route param
    if(infiniteMode && $scope.model.id) {
        nodeId = $scope.model.id;
    } else {
        nodeId = $routeParams.id;
    }
    
    // when opening the editor through infinite editing get the 
    // create option from the model instead of the route param
    if(infiniteMode) {
        create = $scope.model.create;
    } else {
        create = $routeParams.create;
    }

    //setup scope vars
    $scope.currentSection = appState.getSectionState("currentSection");
    $scope.currentNode = null; //the editors affiliated node

    $scope.page = {};
    $scope.page.loading = false;
    $scope.page.menu = {};
    $scope.page.menu.currentSection = appState.getSectionState("currentSection");
    $scope.page.menu.currentNode = null; //the editors affiliated node
    $scope.page.listViewPath = null;
    $scope.page.saveButtonState = "init";
    $scope.page.submitButtonLabel = "Save";

    /** Syncs the content item to it's tree node - this occurs on first load and after saving */
    function syncTreeNode(content, path, initialLoad) {

        if (infiniteMode) {
            return;
        }

        if (!$scope.content.isChildOfListView) {
            navigationService.syncTree({ tree: "media", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
                $scope.page.menu.currentNode = syncArgs.node;
            });
        }
        else if (initialLoad === true) {

            //it's a child item, just sync the ui node to the parent
            navigationService.syncTree({ tree: "media", path: path.substring(0, path.lastIndexOf(",")).split(","), forceReload: initialLoad !== true });

            //if this is a child of a list view and it's the initial load of the editor, we need to get the tree node 
            // from the server so that we can load in the actions menu.
            umbRequestHelper.resourcePromise(
                $http.get(content.treeNodeUrl),
                'Failed to retrieve data for child node ' + content.id).then(function (node) {
                    $scope.page.menu.currentNode = node;
                });
        }
    }

    if (create) {

        $scope.page.loading = true;

        mediaResource.getScaffold(nodeId, $routeParams.doctype)
            .then(function (data) {
                $scope.content = data;

                editorState.set($scope.content);

                init();

                $scope.page.loading = false;

            });
    }
    else {
        $scope.page.loading = true;
        loadMedia()
            .then(function(){
                $scope.page.loading = false;
            });
    }

    function init() {

        // set first app to active
        $scope.content.apps[0].active = true;

        // setup infinite mode
        if(infiniteMode) {
            $scope.page.submitButtonLabel = "Save and Close";
        }

    }
    
    $scope.save = function () {

        if (!$scope.busy && formHelper.submitForm({ scope: $scope })) {

            $scope.busy = true;
            $scope.page.saveButtonState = "busy";

            mediaResource.save($scope.content, create, fileManager.getFiles())
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        redirectOnSuccess: !infiniteMode,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    editorState.set($scope.content);
                    $scope.busy = false;
                    
                    syncTreeNode($scope.content, data.path);

                    init();

                    $scope.page.saveButtonState = "success";

                    // close the editor if it's infinite mode
                    if(infiniteMode && $scope.model.submit) {
                        $scope.model.mediaNode = $scope.content;
                        $scope.model.submit($scope.model);
                    }

                }, function(err) {

                    contentEditingHelper.handleSaveError({
                        err: err,
                        redirectOnError: !infiniteMode,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, err.data)
                    });
                    
                    editorState.set($scope.content);
                    $scope.busy = false;
                    $scope.page.saveButtonState = "error";

                });
        }else{
            $scope.busy = false;
        }
        
    };

    function loadMedia() {

        return mediaResource.getById(nodeId)
            .then(function (data) {

                $scope.content = data;
                
                if (data.isChildOfListView && data.trashed === false) {
                    $scope.page.listViewPath = ($routeParams.page)
                        ? "/media/media/edit/" + data.parentId + "?page=" + $routeParams.page
                        : "/media/media/edit/" + data.parentId;
                }

                editorState.set($scope.content);

                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.notifyAndClearAllSubscriptions();

                if(!infiniteMode) {
                    syncTreeNode($scope.content, data.path, true); 
                }
               
                if ($scope.content.parentId && $scope.content.parentId != -1) {
                    //We fetch all ancestors of the node to generate the footer breadcrump navigation
                    entityResource.getAncestors(nodeId, "media")
                        .then(function (anc) {
                            $scope.ancestors = anc;
                        });
                }

                init();

                $scope.page.loading = false;

                $q.resolve($scope.content);

            });

    }

    $scope.close = function() {
        if($scope.model.close) {
            $scope.model.close($scope.model);
        }
    };

    evts.push(eventsService.on("editors.mediaType.saved", function(name, args) {
        // if this media item uses the updated media type we need to reload the media item
        if(args && args.mediaType && args.mediaType.key === $scope.content.contentType.key) {
            loadMedia();
        }
    }));

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });

}

angular.module("umbraco").controller("Umbraco.Editors.Media.EditController", mediaEditController);
