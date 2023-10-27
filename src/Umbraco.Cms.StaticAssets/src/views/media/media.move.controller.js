//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Editors.Media.MoveController",
    function ($scope, userService, eventsService, mediaResource, appState, treeService, navigationService) {

	    $scope.dialogTreeApi = {};
        $scope.source = _.clone($scope.currentNode);

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

        function treeLoadedHandler(args) {
            if ($scope.source && $scope.source.path) {
                $scope.dialogTreeApi.syncTree({ path: $scope.source.path, activate: false });
            }
        }

	    function nodeSelectHandler(args) {

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

		function nodeExpandedHandler(args) {
			// open mini list view for list views
        	if (args.node.metaData.isContainer) {
				openMiniListView(args.node);
			}
	    }

        $scope.onTreeInit = function () {
            $scope.dialogTreeApi.callbacks.treeLoaded(treeLoadedHandler);
            $scope.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
            $scope.dialogTreeApi.callbacks.treeNodeExpanded(nodeExpandedHandler);
        }	  
        
        $scope.close = function() {
            navigationService.hideDialog();
        };
        $scope.hideSearch = function () {
            $scope.searchInfo.showSearch = false;
            $scope.searchInfo.searchFromId = null;
            $scope.searchInfo.searchFromName = null;
            $scope.searchInfo.results = [];
        }

        // method to select a search result 
        $scope.selectResult = function (evt, result) {
            result.selected = result.selected === true ? false : true;
            nodeSelectHandler({ event: evt, node: result });
        };

        //callback when there are search results 
        $scope.onSearchResults = function (results) {
            $scope.searchInfo.results = results;
            $scope.searchInfo.showSearch = true;
        };

	    $scope.move = function () {
	        $scope.busy = true;
            mediaResource.move({ parentId: $scope.target.id, id: $scope.source.id })
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
                    $scope.busy = false;
                    $scope.success = false;
                    $scope.error = err;
                });
	    };

		// Mini list view
		$scope.selectListViewNode = function (node) {
			node.selected = node.selected === true ? false : true;
			nodeSelectHandler({ node: node });
		};

		$scope.closeMiniListView = function () {
			$scope.miniListView = undefined;
		};

		function openMiniListView(node) {
			$scope.miniListView = node;
		}

	});
