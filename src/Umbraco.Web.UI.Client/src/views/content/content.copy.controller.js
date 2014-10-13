angular.module("umbraco").controller("Umbraco.Editors.Content.CopyController",
	function ($scope, eventsService, contentResource, navigationService, appState, treeService, localizationService) {

	    var dialogOptions = $scope.dialogOptions;
	    var searchText = "Search...";
	    localizationService.localize("general_search").then(function (value) {
	        searchText = value + "...";
	    });

	    $scope.relateToOriginal = false;
	    $scope.dialogTreeEventHandler = $({});
	    $scope.busy = false;
	    $scope.searchInfo = {
	        searchFromId: null,
	        searchFromName: null,
	        showSearch: false,
	        results: [],
	        selectedSearchResults: []
	    }

	    var node = dialogOptions.currentNode;

	    function nodeSelectHandler(ev, args) {
	        args.event.preventDefault();
	        args.event.stopPropagation();

	        if (args.node.metaData.listViewNode) {
	            //check if list view 'search' node was selected

	            $scope.searchInfo.showSearch = true;
	            $scope.searchInfo.searchFromId = args.node.metaData.listViewNode.id;
	            $scope.searchInfo.searchFromName = args.node.metaData.listViewNode.name;
	        }
	        else {
	            eventsService.emit("editors.content.copyController.select", args);

	            if ($scope.target) {
	                //un-select if there's a current one selected
	                $scope.target.selected = false;
	            }

	            $scope.target = args.node;
	            $scope.target.selected = true;
	        }
	        
	    }

	    function nodeExpandedHandler(ev, args) {
	        if (angular.isArray(args.children)) {

	            //iterate children
	            _.each(args.children, function (child) {
	                //check if any of the items are list views, if so we need to add a custom 
	                // child: A node to activate the search
	                if (child.metaData.isContainer) {
	                    child.hasChildren = true;
	                    child.children = [
	                        {
	                            level: child.level + 1,
	                            hasChildren: false,
	                            name: searchText,
	                            metaData: {
	                                listViewNode: child,
	                            },
	                            cssClass: "icon umb-tree-icon sprTree icon-search",
	                            cssClasses: ["not-published"]
	                        }
	                    ];
	                }
	            });
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

	        contentResource.copy({ parentId: $scope.target.id, id: node.id, relateToOriginal: $scope.relateToOriginal })
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
                });
	    };

	    $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);
	    $scope.dialogTreeEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);

	    $scope.$on('$destroy', function () {
	        $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
	        $scope.dialogTreeEventHandler.unbind("treeNodeExpanded", nodeExpandedHandler);
	    });
	});