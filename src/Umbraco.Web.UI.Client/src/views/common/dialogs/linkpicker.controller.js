//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.LinkPickerController",
	function ($scope, eventsService, dialogService, entityResource, contentResource, mediaHelper, userService, localizationService) {
	    var dialogOptions = $scope.dialogOptions;

	    var searchText = "Search...";
	    localizationService.localize("general_search").then(function (value) {
	        searchText = value + "...";
	    });

	    $scope.dialogTreeEventHandler = $({});
	    $scope.target = {};
	    $scope.searchInfo = {
	        searchFromId: null,
	        searchFromName: null,
	        showSearch: false,
	        results: [],
	        selectedSearchResults: []
	    }

	    if (dialogOptions.currentTarget) {
	        $scope.target = dialogOptions.currentTarget;

	        //if we have a node ID, we fetch the current node to build the form data
	        if ($scope.target.id) {

	            if (!$scope.target.path) {
	                entityResource.getPath($scope.target.id, "Document").then(function (path) {
	                    $scope.target.path = path;
	                    //now sync the tree to this path
	                    $scope.dialogTreeEventHandler.syncTree({ path: $scope.target.path, tree: "content" });
	                });
	            }

	            contentResource.getNiceUrl($scope.target.id).then(function (url) {
	                $scope.target.url = url;
	            });
	        }
	    }

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
	            eventsService.emit("dialogs.linkPicker.select", args);

	            if ($scope.currentNode) {
	                //un-select if there's a current one selected
	                $scope.currentNode.selected = false;
	            }

	            $scope.currentNode = args.node;
	            $scope.currentNode.selected = true;
	            $scope.target.id = args.node.id;
	            $scope.target.name = args.node.name;

	            if (args.node.id < 0) {
	                $scope.target.url = "/";
	            }
	            else {
	                contentResource.getNiceUrl(args.node.id).then(function (url) {
	                    $scope.target.url = url;
	                });
	            }

	            if (!angular.isUndefined($scope.target.isMedia)) {
	                delete $scope.target.isMedia;
	            }
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

	    $scope.switchToMediaPicker = function () {
	        userService.getCurrentUser().then(function (userData) {
	            dialogService.mediaPicker({
	                startNodeId: userData.startMediaId,
	                callback: function (media) {
	                    $scope.target.id = media.id;
	                    $scope.target.isMedia = true;
	                    $scope.target.name = media.name;
	                    $scope.target.url = mediaHelper.resolveFile(media);
	                }
	            });
	        });
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
	        nodeSelectHandler(evt, {event: evt, node: result});
	    };

        //callback when there are search results 
	    $scope.onSearchResults = function (results) {
	        $scope.searchInfo.results = results;
            $scope.searchInfo.showSearch = true;
	    };

	    $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);
	    $scope.dialogTreeEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);

	    $scope.$on('$destroy', function () {
	        $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
	        $scope.dialogTreeEventHandler.unbind("treeNodeExpanded", nodeExpandedHandler);
	    });
	});