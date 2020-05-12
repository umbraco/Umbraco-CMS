//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Editors.Media.MoveController",
    function ($scope, userService, eventsService, mediaResource, appState, treeService, navigationService) {
	    var dialogOptions = $scope.dialogOptions;

	    $scope.dialogTreeEventHandler = $({});
	    var node = dialogOptions.currentNode;

        $scope.busy = false;
        $scope.searchInfo = {
            searchFromId: null,
            searchFromName: null,
            showSearch: false,
            results: [],
            selectedSearchResults: []
        }
        $scope.treeModel = {
            hideHeader: false
        }
        userService.getCurrentUser().then(function (userData) {
            $scope.treeModel.hideHeader = userData.startMediaIds.length > 0 && userData.startMediaIds.indexOf(-1) == -1;
        });

        function treeLoadedHandler(ev, args) {
            if (node && node.path) {
                $scope.dialogTreeEventHandler.syncTree({ path: node.path, activate: false });
            }
        }

	    function nodeSelectHandler(ev, args) {

			if(args && args.event) {
				args.event.preventDefault();
				args.event.stopPropagation();
			}

	        eventsService.emit("editors.media.moveController.select", args);

	        if ($scope.target) {
	            //un-select if there's a current one selected
	            $scope.target.selected = false;
	        }

	        $scope.target = args.node;
	        $scope.target.selected = true;
	    }

		function nodeExpandedHandler(ev, args) {
			// open mini list view for list views
        	if (args.node.metaData.isContainer) {
				openMiniListView(args.node);
			}
	    }

        $scope.hideSearch = function () {
            $scope.searchInfo.showSearch = false;
            $scope.searchInfo.searchFromId = null;
            $scope.searchInfo.searchFromName = null;
            $scope.searchInfo.results = [];
        }

        // method to select a search result 
        $scope.selectResult = function (evt, result) {
            result.selected = result.selected === true ? false : true;
            nodeSelectHandler(evt, { event: evt, node: result });
        };

        //callback when there are search results 
        $scope.onSearchResults = function (results) {
            $scope.searchInfo.results = results;
            $scope.searchInfo.showSearch = true;
        };

	    $scope.move = function () {
	        $scope.busy = true;
	        mediaResource.move({ parentId: $scope.target.id, id: node.id })
                .then(function (path) {
	                $scope.busy = false;
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

        $scope.dialogTreeEventHandler.bind("treeLoaded", treeLoadedHandler);
        $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);
        $scope.dialogTreeEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);

	    $scope.$on('$destroy', function () {
	        $scope.dialogTreeEventHandler.unbind("treeLoaded", treeLoadedHandler);
	        $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
			$scope.dialogTreeEventHandler.unbind("treeNodeExpanded", nodeExpandedHandler);
	    });

		// Mini list view
		$scope.selectListViewNode = function (node) {
			node.selected = node.selected === true ? false : true;
			nodeSelectHandler({}, { node: node });
		};

		$scope.closeMiniListView = function () {
			$scope.miniListView = undefined;
		};

		function openMiniListView(node) {
			$scope.miniListView = node;
		}

	});
