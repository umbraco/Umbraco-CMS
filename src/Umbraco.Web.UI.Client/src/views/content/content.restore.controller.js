angular.module("umbraco").controller("Umbraco.Editors.Content.RestoreController",
	function ($scope, relationResource, contentResource, navigationService, appState, treeService) {
		var dialogOptions = $scope.dialogOptions;

		var node = dialogOptions.currentNode;

		$scope.error = null;
	    $scope.success = false;

		relationResource.getByChildId(node.id, "relateParentDocumentOnDelete").then(function (data) {

            if (data.length == 0) {
                $scope.success = false;
                $scope.error = {
                    errorMsg: "Cannot automatically restore this item",
                    data: {
                        Message: "There is no 'restore' relation found for this node. Use the Move menu item to move it manually."
                    }
                }
                return;
            }

		    $scope.relation = data[0];

			if ($scope.relation.parentId == -1) {
				$scope.target = { id: -1, name: "Root" };

			} else {
			    contentResource.getById($scope.relation.parentId).then(function (data) {
					$scope.target = data;

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
			contentResource.move({ parentId: $scope.target.id, id: node.id })
				.then(function (path) {

					$scope.success = true;

					//first we need to remove the node that launched the dialog
					treeService.removeNode($scope.currentNode);

					//get the currently edited node (if any)
					var activeNode = appState.getTreeState("selectedNode");

					//we need to do a double sync here: first sync to the moved content - but don't activate the node,
					//then sync to the currenlty edited content (note: this might not be the content that was moved!!)

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
	});