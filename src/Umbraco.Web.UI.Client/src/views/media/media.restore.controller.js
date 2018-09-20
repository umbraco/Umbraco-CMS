angular.module("umbraco").controller("Umbraco.Editors.Media.RestoreController",
    function ($scope, relationResource, mediaResource, navigationService, appState, treeService, localizationService) {
		var dialogOptions = $scope.dialogOptions;

		var node = dialogOptions.currentNode;

		$scope.error = null;
	    $scope.success = false;

		relationResource.getByChildId(node.id, "relateParentDocumentOnDelete").then(function (data) {

            if (data.length == 0) {
                $scope.success = false;
                $scope.error = {
                    errorMsg: localizationService.localize('recycleBin_itemCannotBeRestored'),
                    data: {
                        Message: localizationService.localize('recycleBin_noRestoreRelation')
                    }
                }
                return;
            }

		    $scope.relation = data[0];

			if ($scope.relation.parentId == -1) {
				$scope.target = { id: -1, name: "Root" };

			} else {
			    mediaResource.getById($scope.relation.parentId).then(function (data) {
                    $scope.target = data;

                    // make sure the target item isn't in the recycle bin
                    if ($scope.target.path.indexOf("-20") !== -1) {
                        $scope.error = {
                            errorMsg: localizationService.localize('recycleBin_itemCannotBeRestored'),
                            data: {
                                Message: localizationService.localize('recycleBin_restoreUnderRecycled').then(function (value) {
                                    value.replace('%0%', $scope.target.name);
                                })
                            }
                        };
                        $scope.success = false;
                    }

				}, function (err) {
					$scope.success = false;
					$scope.error = err;
				});
			}

		}, function (err) {
			$scope.success = false;
			$scope.error = err;
		});

		$scope.restore = function () {
			// this code was copied from `content.move.controller.js`
			mediaResource.move({ parentId: $scope.target.id, id: node.id })
				.then(function (path) {

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
					$scope.success = false;
					$scope.error = err;
				});
		};
	});
