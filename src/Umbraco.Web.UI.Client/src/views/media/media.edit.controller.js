/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.EditController
 * @function
 * 
 * @description
 * The controller for the media editor
 */
function mediaEditController($scope, $routeParams, $q, userService, appState, mediaResource, entityResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, fileManager, treeService, formHelper, umbModelMapper, editorState, umbRequestHelper, $http) {

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

    function checkSyncingToCurrentNode() {

        //it's not a child of a list view, then we should sync normally
        if (!$scope.content.isChildOfListView) {
            return $q.when(true);
        }

        //it is a child of a list view, we need to check if the user's start node ids
        //this is a user's start node and if so then sync normally
        return userService.getCurrentUser().then(function (userData) {

            if (userData.startMediaIds.indexOf($scope.content.id) > -1) {
                return $q.when(true);
            }
            else {
                return $q.reject(false);
            }
        });
    }

    /** Syncs the content item to it's tree node - this occurs on first load and after saving */
    function syncTreeNode(content, path, initialLoad) {

        //Check if this is a child of a list view and if the user has a start node in within this path,
        //we will still want to sync the tree. This is a special case when the user's start node is a child
        //of a list view.

        checkSyncingToCurrentNode().then(function () {
                //we need to sync to the current node, it's either a normal node OR
                // it's a child of a list view and it's the user's start node
                navigationService.syncTree({ tree: "media", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
                    $scope.page.menu.currentNode = syncArgs.node;
                });
            },
            function () {

                if (initialLoad === true) {

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
            });
    }

    if ($routeParams.create) {

        $scope.page.loading = true;

        mediaResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function (data) {
                $scope.content = data;

                editorState.set($scope.content);

                // We don't get the info tab from the server from version 7.8 so we need to manually add it
                contentEditingHelper.addInfoTab($scope.content.tabs);

                $scope.page.loading = false;

            });
    }
    else {

        $scope.page.loading = true;

        mediaResource.getById($routeParams.id)
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
                serverValidationManager.executeAndClearAllSubscriptions();

                syncTreeNode($scope.content, data.path, true);
               
                if ($scope.content.parentId && $scope.content.parentId != -1) {
                    //We fetch all ancestors of the node to generate the footer breadcrump navigation
                    entityResource.getAncestors($routeParams.id, "media")
                        .then(function (anc) {
                            $scope.ancestors = anc;
                        });
                }

                // We don't get the info tab from the server from version 7.8 so we need to manually add it
                contentEditingHelper.addInfoTab($scope.content.tabs);

                $scope.page.loading = false;

            });
    }
    
    $scope.save = function () {

        if (!$scope.busy && formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            $scope.busy = true;
            $scope.page.saveButtonState = "busy";

            mediaResource.save($scope.content, $routeParams.create, fileManager.getFiles())
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    editorState.set($scope.content);
                    $scope.busy = false;

                    syncTreeNode($scope.content, data.path);

                    $scope.page.saveButtonState = "success";

                }, function(err) {

                    contentEditingHelper.handleSaveError({
                        err: err,
                        redirectOnFailure: true,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, err.data)
                    });
                    
                    //show any notifications
                    if (angular.isArray(err.data.notifications)) {
                        for (var i = 0; i < err.data.notifications.length; i++) {
                            notificationsService.showNotification(err.data.notifications[i]);
                        }
                    }

                    editorState.set($scope.content);
                    $scope.busy = false;
                    $scope.page.saveButtonState = "error";

                });
        }else{
            $scope.busy = false;
        }
        
    };

}

angular.module("umbraco")
    .controller("Umbraco.Editors.Media.EditController", mediaEditController);
