angular.module("umbraco")
	.controller("Umbraco.Editors.Content.CopyController",
	function ($scope, eventsService, contentResource, navigationService, appState, treeService) {

	    var dialogOptions = $scope.dialogOptions;

	    $scope.relateToOriginal = false;
	    $scope.dialogTreeEventHandler = $({});

	    var node = dialogOptions.currentNode;

	    function nodeSelectHandler(ev, args) {
	        args.event.preventDefault();
	        args.event.stopPropagation();

	        eventsService.emit("editors.content.copyController.select", args);

	        if ($scope.target) {
	            //un-select if there's a current one selected
	            $scope.target.selected = false;
	        }

	        $scope.target = args.node;
	        $scope.target.selected = true;
	    }

	    $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);

	    $scope.copy = function () {
	        contentResource.copy({ parentId: $scope.target.id, id: node.id, relateToOriginal: $scope.relateToOriginal })
                .then(function (path) {
                    $scope.error = false;
                    $scope.success = true;

                    //get the currently edited node (if any)
                    var activeNode = appState.getTreeState("selectedNode");

                    //we need to do a double sync here: first sync to the copied content - but don't activate the node,
                    //then sync to the currenlty edited content (note: this might not be the content that was copied!!)

                    navigationService.syncTree({ tree: "content", path: path, forceReload: true, activate: false }).then(function (args) {
                        if (activeNode) {
                            var activeNodePath = treeService.getPath(activeNode).join();
                            //sync to this node now - depending on what was copied this might already be synced but might not be
                            navigationService.syncTree({ tree: "content", path: activeNodePath, forceReload: false, activate: true });
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