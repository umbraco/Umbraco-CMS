angular.module("umbraco").controller("Umbraco.Editors.Content.MoveController",
	function ($scope, userService, eventsService, contentResource, navigationService, appState, treeService, localizationService, notificationsService) {

	    var searchText = "Search...";
	    localizationService.localize("general_search").then(function (value) {
	        searchText = value + "...";
	    });

	    $scope.dialogTreeApi = {};
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
            $scope.treeModel.hideHeader = userData.startContentIds.length > 0 && userData.startContentIds.indexOf(-1) == -1;
        });

	    $scope.source = _.clone($scope.currentNode);

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

			eventsService.emit("editors.content.moveController.select", args);

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
		
		$scope.close = function() {
			navigationService.hideDialog();
		};

	    $scope.move = function () {

	        $scope.busy = true;
	        $scope.error = false;

	        contentResource.move({ parentId: $scope.target.id, id: $scope.source.id })
                .then(function (path) {
                    $scope.error = false;
                    $scope.success = true;
                    $scope.busy = false;

                    //first we need to remove the node that launched the dialog
                    treeService.removeNode($scope.currentNode);

                    //get the currently edited node (if any)
                    var activeNode = appState.getTreeState("selectedNode");

                    //we need to do a double sync here: first sync to the moved content - but don't activate the node,
                    //then sync to the currently edited content (note: this might not be the content that was moved!!)

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
                    $scope.busy = false;
                   
                });
	    };

        $scope.onTreeInit = function () {
            $scope.dialogTreeApi.callbacks.treeLoaded(treeLoadedHandler);
            $scope.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
            $scope.dialogTreeApi.callbacks.treeNodeExpanded(nodeExpandedHandler);
        }	    
        
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
