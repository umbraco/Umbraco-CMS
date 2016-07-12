angular.module("umbraco")
.controller("Umbraco.Editors.MediaTypes.CopyController",
    function ($scope, mediaTypeResource, treeService, navigationService, notificationsService, appState, eventsService) {
        var dialogOptions = $scope.dialogOptions;
        $scope.dialogTreeEventHandler = $({});

        function nodeSelectHandler(ev, args) {
            args.event.preventDefault();
            args.event.stopPropagation();

            if ($scope.target) {
                //un-select if there's a current one selected
                $scope.target.selected = false;
            }

            $scope.target = args.node;
            $scope.target.selected = true;
        }

        $scope.copy = function () {

            $scope.busy = true;
            $scope.error = false;

            mediaTypeResource.copy({ parentId: $scope.target.id, id: dialogOptions.currentNode.id })
                .then(function (path) {
                    $scope.error = false;
                    $scope.success = true;
                    $scope.busy = false;

                    //get the currently edited node (if any)
                    var activeNode = appState.getTreeState("selectedNode");

                    //we need to do a double sync here: first sync to the copied content - but don't activate the node,
                    //then sync to the currenlty edited content (note: this might not be the content that was copied!!)

                    navigationService.syncTree({ tree: "mediaTypes", path: path, forceReload: true, activate: false }).then(function (args) {
                        if (activeNode) {
                            var activeNodePath = treeService.getPath(activeNode).join();
                            //sync to this node now - depending on what was copied this might already be synced but might not be
                            navigationService.syncTree({ tree: "mediaTypes", path: activeNodePath, forceReload: false, activate: true });
                        }
                    });

                }, function (err) {
                    $scope.success = false;
                    $scope.error = err;
                    $scope.busy = false;
                    //show any notifications
                    if (angular.isArray(err.data.notifications)) {
                        for (var i = 0; i < err.data.notifications.length; i++) {
                            notificationsService.showNotification(err.data.notifications[i]);
                        }
                    }
                });
        };

        $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);

        $scope.$on('$destroy', function () {
            $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
        });
    });
