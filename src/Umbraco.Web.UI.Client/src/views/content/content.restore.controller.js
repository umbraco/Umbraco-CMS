angular.module("umbraco").controller("Umbraco.Editors.Content.RestoreController",
	function ($scope, relationResource, contentResource, navigationService, appState, treeService) {
		var dialogOptions = $scope.dialogOptions;

		var node = dialogOptions.currentNode;

		relationResource.getByChildId(node.id, "relateParentDocumentOnDelete").then(function (data) {
			var relation = data[0];

			contentResource.getById(relation.ParentId).then(function (data) {
				$scope.target = data;

			}, function (err) {
				$scope.success = false;
				$scope.error = err;
			});

		}, function (err) {
			$scope.success = false;
			$scope.error = err;
		});

		$scope.restore = function () {
			contentResource.move({ parentId: $scope.target.id, id: node.id })
				.then(function (path) {
					$scope.error = false;
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

					// TODO: [LK] Call the relationResource to remove the relation (post-delete)

				}, function (err) {
					$scope.success = false;
					$scope.error = err;
				});
		};
	});