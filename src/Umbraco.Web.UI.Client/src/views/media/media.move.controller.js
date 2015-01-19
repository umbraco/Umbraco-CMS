//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Editors.Media.MoveController",
	function ($scope, eventsService, mediaResource, appState, treeService, navigationService) {
	    var dialogOptions = $scope.dialogOptions;

	    $scope.dialogTreeEventHandler = $({});
	    var node = dialogOptions.currentNode;

	    function nodeSelectHandler(ev, args) {
	        args.event.preventDefault();
	        args.event.stopPropagation();

	        eventsService.emit("editors.media.moveController.select", args);

	        if ($scope.target) {
	            //un-select if there's a current one selected
	            $scope.target.selected = false;
	        }

	        $scope.target = args.node;
	        $scope.target.selected = true;
	    }

	    $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);


	    $scope.move = function () {
	        mediaResource.move({ parentId: $scope.target.id, id: node.id })
                .then(function (path) {
                    $scope.error = false;
                    $scope.success = true;

                    //first we need to remove the node that launched the dialog
                    treeService.removeNode($scope.currentNode);

                    //get the currently edited node (if any)
                    var activeNode = appState.getTreeState("selectedNode");

                    //we need to do a double sync here: first sync to the moved content - but don't activate the node,
                    //then sync to the currenlty edited content (note: this might not be the content that was moved!!)

                    navigationService.syncTree({ tree: "media", path: path, forceReload: true, activate: false }).then(function (args) {
                        if (activeNode) {
                            var activeNodePath = treeService.getPath(activeNode).join();
                            //sync to this node now - depending on what was copied this might already be synced but might not be
                            navigationService.syncTree({ tree: "media", path: activeNodePath, forceReload: false, activate: true });
                        }
                    });

                }, function (err) {
                    $scope.success = false;
                    $scope.error = err;
                });
	    };

	    $scope.$on('$destroy', function () {
	        $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
	    });
	});