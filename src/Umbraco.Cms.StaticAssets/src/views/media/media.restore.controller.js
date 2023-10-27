angular.module("umbraco").controller("Umbraco.Editors.Media.RestoreController",
    function ($scope, relationResource, mediaResource, entityResource, navigationService, appState, treeService, userService, localizationService) {

        $scope.source = _.clone($scope.currentNode);

		$scope.error = null;
        $scope.loading = true;
        $scope.moving = false;
        $scope.success = false;

        $scope.dialogTreeApi = {};
        $scope.searchInfo = {
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
        $scope.labels = {};
        localizationService.localizeMany(["treeHeaders_media"]).then(function (data) {
            $scope.labels.treeRoot = data[0];
        });

        function nodeSelectHandler(args) {

            if (args && args.event) {
                args.event.preventDefault();
                args.event.stopPropagation();
            }

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

        $scope.onTreeInit = function () {
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

        relationResource.getByChildId($scope.source.id, "relateParentMediaFolderOnDelete").then(function (data) {
            $scope.loading = false;

            if (!data.length) {
                $scope.moving = true;
                return;
            }

		    $scope.relation = data[0];

			if ($scope.relation.parentId === -1) {
                $scope.target = { id: -1, name: $scope.labels.treeRoot };

			} else {
                $scope.loading = true;

			    entityResource.getById($scope.relation.parentId, "media").then(function (data) {
			        $scope.loading = false;
			        $scope.target = data;

			        // make sure the target item isn't in the recycle bin
			        if ($scope.target.path.indexOf("-21") !== -1) {
			            $scope.moving = true;
			            $scope.target = null;
			        }
				}, function (err) {
			        $scope.loading = false;
			        $scope.error = err;
			    });
			}

		}, function (err) {
            $scope.loading = false;
            $scope.error = err;
		});

		$scope.restore = function () {
            $scope.loading = true;

			// this code was copied from `content.move.controller.js`
            mediaResource.move({ parentId: $scope.target.id, id: $scope.source.id })
				.then(function (path) {

                    $scope.loading = false;
					$scope.success = true;

					//first we need to remove the node that launched the dialog
					treeService.removeNode($scope.currentNode);

					//get the currently edited node (if any)
					var activeNode = appState.getTreeState("selectedNode");

					//we need to do a double sync here: first sync to the moved media item - but don't activate the node,
					//then sync to the currenlty edited media item (note: this might not be the media item that was moved!!)

					navigationService.syncTree({ tree: "media", path: path, forceReload: true, activate: false }).then(function (args) {
						if (activeNode) {
							var activeNodePath = treeService.getPath(activeNode).join();
							//sync to this node now - depending on what was copied this might already be synced but might not be
							navigationService.syncTree({ tree: "media", path: activeNodePath, forceReload: false, activate: true });
						}
					});

				}, function (err) {
                    $scope.loading = false;
                    $scope.error = err;
				});
		};

		$scope.close = function() {
			navigationService.hideDialog();
		};

	});
