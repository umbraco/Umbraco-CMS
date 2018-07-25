angular.module("umbraco").controller("Umbraco.Editors.Content.CopyController",
    function ($scope, userService, eventsService, contentResource, navigationService, appState, treeService, localizationService, notificationsService) {

	    var dialogOptions = $scope.dialogOptions;
	    var searchText = "Search...";
	    localizationService.localize("general_search").then(function (value) {
	        searchText = value + "...";
	    });

	    $scope.relateToOriginal = true;
	    $scope.recursive = true;
	    $scope.dialogTreeEventHandler = $({});
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
        $scope.toggle = toggleHandler;
	    userService.getCurrentUser().then(function (userData) {
            $scope.treeModel.hideHeader = userData.startContentIds.length > 0 && userData.startContentIds.indexOf(-1) == -1;
	    });

	    var node = dialogOptions.currentNode;

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

			eventsService.emit("editors.content.copyController.select", args);

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

        function toggleHandler(type){
            // If the relateToOriginal toggle is clicked
            if(type === "relate"){
                if($scope.relateToOriginal){
                    $scope.relateToOriginal = false;
                    return;
                }
                $scope.relateToOriginal = true;
            }

            // If the recurvise toggle is clicked
            if(type === "recursive"){
                if($scope.recursive){
                    $scope.recursive = false;
                    return;
                }
                $scope.recursive = true;
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

	    $scope.copy = function () {

	        $scope.busy = true;
	        $scope.error = false;

	        contentResource.copy({ parentId: $scope.target.id, id: node.id, relateToOriginal: $scope.relateToOriginal, recursive: $scope.recursive })
                .then(function (path) {
                    $scope.error = false;
                    $scope.success = true;
                    $scope.busy = false;

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
                    $scope.busy = false;
                    //show any notifications
                    if (angular.isArray(err.data.notifications)) {
                        for (var i = 0; i < err.data.notifications.length; i++) {
                            notificationsService.showNotification(err.data.notifications[i]);
                        }
                    }
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
